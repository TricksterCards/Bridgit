using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
	public class AuctionException: Exception
	{
		public AuctionException(Call call, PositionState by, Contract contract, string description) : 
			base($"{call} is invalid by {by.Direction} over {contract}:  {description}")
		{ }
	}
	
    public class Contract
    {
        public Bid Bid = null;    
        public PositionState LastBidBy = null;
		public PositionState Declarer = null;
        public bool Doubled = false;
        public bool Redoubled = false;
		public int CallsRemaining = 4;
		public Dictionary<Strain, List<PositionState>> FirstToNameStrain = new Dictionary<Strain, List<PositionState>>();


		public bool IsOurs(PositionState ps)
		{
			return (Declarer != null && Declarer.PairState == ps.PairState);
		}


		public bool IsOpponents(PositionState ps)
		{
			return (Declarer != null && Declarer.PairState != ps.PairState);
		}


		public void ValidateCall(Call call, PositionState by)
		{
			var error = CallError(call, by);
			if (error != null)
				throw new AuctionException(call, by, this, error);
		}

		public bool IsValid(Call call, PositionState by)
		{
			return CallError(call, by) == null;
		}

		private string CallError(Call call, PositionState by)
		{
			if (AuctionComplete) 
				return "Auction is complete.  No more calls allowed";
			if (call is Pass)
				return null;
			if (call is Double)
			{
				if (Doubled)
					return "Can not double contract.  Already doubled.";
				if (IsOurs(by))
					return "Can not double own side's contract";
				return null;
			}
			if (call is Redouble)
			{
				if (Redoubled)
					return "Can not redouble already redoubled contract.";
				if (!Doubled)
					return "Can not redouble contract.  Contract has not been doubled";
				if (IsOpponents(by))
					return "Can not redouble contract.  Contract is opponents.";
				return null;
			}
			if (call is Bid newBid)
			{
				if (this.Bid != null && newBid.CompareTo(this.Bid) <= 0)
					return $"Bid {newBid} is lower that current contract of {this.Bid}";
				return null;
			}
			Debug.Assert(false);	// Should never get here...
			return "Unknown internal state error";
		}

		private void MakeBid(Bid bid, PositionState by)
		{
            Bid = bid;
            LastBidBy = by;
            Doubled = false;
            Redoubled = false;
            CallsRemaining = 3;
			// Now figure out who is declarer.  First of our pair to bid a stain.
			// For simplicity we assume the current bidder will be declarer.  Code below
			// will change it if necessary
			Declarer = by;
			if (this.FirstToNameStrain.ContainsKey(bid.Strain))
			{
				foreach (var namedStrain in FirstToNameStrain[bid.Strain])
				{
					if (namedStrain == by) return;
					if (namedStrain == by.Partner)
					{
						Declarer = by.Partner;
						return;
					}
				}
				// If we get here then the current pair (the "by" position) has not
				// bid this strain, but the opponents have.  Add the current bidder
				// to the list for this strain.
				FirstToNameStrain[bid.Strain].Add(by);
			}
			else
			{
				FirstToNameStrain[bid.Strain] = new List<PositionState> { by };
			}
        }

        public void MakeCall(Call call, PositionState by)
		{
			ValidateCall(call, by);	// Throws AuctionExeption if invalid call sequence
			if (call is Pass)
			{
				CallsRemaining -= 1;
			}
			else if (call is Bid bid)
			{
				MakeBid(bid, by);
			}
			else if (call is Double)
			{
				Doubled = true;
				CallsRemaining = 3;
			}
			else if (call is Redouble)
			{
				Redoubled = true;
				Debug.Assert(this.Doubled);
				CallsRemaining = (Bid.Level == 7 && Bid.Strain == Strain.NoTrump) ? 0 : 3;
			}
		}

		public int Jump(Bid bid)
		{
			
			return (this.Bid == null) ? bid.Level - 1 : bid.JumpOver(Bid);
		}

		public bool PassEndsAuction { get { return this.CallsRemaining == 1; } }

		public bool AuctionComplete { get { return this.CallsRemaining == 0; } }
	}
}
