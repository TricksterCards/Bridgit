using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
	public class AuctionException: Exception
	{
		public AuctionException(Call call, Direction by, Contract contract, string description) : 
			base($"{call} is invalid by {by} over {contract}:  {description}")
		{ }
	}

	public enum Risk { Undoubled, Doubled, Redoubled }
	
    public class Contract
    {
        public Bid Bid = null;    
		public Risk Risk = Risk.Undoubled;

		public static Contract Parse(string s)
		{
			var contract = new Contract();
			if (s != "Pass")	// The default is a passed out contract.
			{
				if (s.Length < 2)
				{
					throw new FormatException($"Invlaid constract {s}");
				}
				int bidLength = 2;
				if (s.Substring(1, 1) == "N")
				{
					bidLength += 1;
				}
				var call = Bid.Parse(s.Substring(0, bidLength));
				if (call is Bid bid)
				{
					contract.Bid = bid;
				}
				else
				{
					throw new FormatException($"Contract can not be {s}");
				}
				var risk = s.Substring(bidLength).Trim();
				if (risk == "X")
				{
					contract.Risk = Risk.Doubled;
				}
				else if (risk == "XX")
				{
					contract.Risk = Risk.Redoubled;
				}
				else if (!string.IsNullOrEmpty(risk))
				{
					throw new FormatException($"Invalid risk {risk}.  Must be X or XX or nothing.");
				}
			}
			return contract;
		}

        public override string ToString()
        {
			var s = "Pass";
            if (Bid != null)
			{
				s = Bid.ToString();
				if (Risk != Risk.Undoubled)
				{
					s += Risk == Risk.Doubled ? "X" : "XX";
				}
			}
			return s;
        }
    }



	public class ContractState: Contract
	{
        public Direction? LastBidBy = null;
		public Direction? Declarer = null;
		public int CallsRemaining = 4;
		public Dictionary<Strain, List<Direction>> FirstToNameStrain = new Dictionary<Strain, List<Direction>>();

		public bool PassEndsAuction => this.CallsRemaining == 1; 

		public bool AuctionComplete => this.CallsRemaining == 0;

		public bool PassedOut => this.CallsRemaining == 0 && Bid == null;
		public bool IsOurs(Direction direction)
		{
			return (Declarer != null && (Declarer == direction || Declarer == BridgeBidder.Partner(direction)));
		}


		public bool IsOpponents(Direction direction)
		{
			return (Declarer != null && !IsOurs(direction));
		}


		public void ValidateCall(Call call, Direction by)
		{
			var error = CallError(call, by);
			if (error != null)
				throw new AuctionException(call, by, this, error);
		}

		public bool IsValid(Call call, Direction by)
		{
			return CallError(call, by) == null;
		}

		private string CallError(Call call, Direction by)
		{
			if (AuctionComplete) 
				return "Auction is complete.  No more calls allowed";
			if (call is Pass)
				return null;
			if (call is Double)
			{
				if (Risk != Risk.Undoubled)
					return $"Can not double contract that is currently {Risk}.";
				if (IsOurs(by))
					return "Can not double own side's contract";
				return null;
			}
			if (call is Redouble)
			{
				if (Risk != Risk.Doubled)
					return $"Can not redouble contract that is currently {Risk}.";
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

		private void MakeBid(Bid bid, Direction by)
		{
            Bid = bid;
            LastBidBy = by;
            Risk = Risk.Undoubled;
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
					if (namedStrain == BridgeBidder.Partner(by))
					{
						Declarer = BridgeBidder.Partner(by);
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
				FirstToNameStrain[bid.Strain] = new List<Direction> { by };
			}
        }

        public void MakeCall(Call call, Direction by)
		{
			//var call = callDetails.Call;
			//var by = callDetails.PositionState.Direction;
			ValidateCall(call, by);	// Throws AuctionExeption if invalid call sequence
			if (call.Equals(Call.Pass))
			{
				CallsRemaining -= 1;
			}
			else if (call is Bid bid)
			{
				MakeBid(bid, by);
			}
			else if (call is Double)
			{
				Risk = Risk.Doubled;
				CallsRemaining = 3;
			}
			else if (call is Redouble)
			{
				Risk = Risk.Redoubled;
				CallsRemaining = (Bid.Level == 7 && Bid.Strain == Strain.NoTrump) ? 0 : 3;
			}
		}

		public int IsJump(Bid bid)
		{
			
			return (this.Bid == null) ? bid.Level - 1 : bid.JumpOver(Bid);
		}

		public static ContractState FromCalls(Direction dealer, IEnumerable<Call> calls)
		{
			var contract = new ContractState();
			Direction d = dealer;
			foreach (var call in calls)
			{
				contract.MakeCall(call, d);
			}
			return contract;
		}

		public static bool IsValidAuction(Direction dealer, IEnumerable<Call> calls, out string error)
		{
			error = null;
			var contract = new ContractState();
			Direction d = dealer;
			foreach (var call in calls)
			{
				error = contract.CallError(call, d);
				if (error != null)
				{
					return false;
				}
				contract.MakeCall(call, d);
			}
			return true;
		}

	}

}
