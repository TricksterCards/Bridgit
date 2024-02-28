namespace BridgeBidding
{
    // TODO: For now this just allows the last bid to be examined...  In the future may need to look back in
    // bid history.  But this works for now...
    public class BidHistory : StaticConstraint, IDescribeConstraint
	{
		private int _bidIndex;
		private Call _call;
	//	private Suit? _suit;
	//	private int _level;
	//	private bool _compareSuit;


		// If call is null then this class will return true if the spcified call
		// is the same suit as the previous bid.
		// If call is non-null then the calls must be equal
		// TODO: IS THIS USED AN1YWHERE?  SEEMS CONFUSING...   REMOVE IF IT ISNT USED. (null call)
		public BidHistory(int bidIndex, Call call)
		{
			this._bidIndex = bidIndex;
			this._call = call;

		}

		public override bool Conforms(Call call, PositionState ps) 
		{
			var previousCall = ps.GetBidHistory(_bidIndex);
			if (previousCall != null)
			{
				if (_call != null)
				{
					return previousCall.Equals(_call);
				}
				if (call is Bid bid && previousCall is Bid previousBid)
				{
					return (bid.Suit == previousBid.Suit);
				}
			}
			return false;
		}
		public string Describe(Call call, PositionState ps)
		{
			if (_call != null && _bidIndex == 0) return $"last call was {_call}";
			if (_call == null && call is Bid bid && bid.Suit is Suit suit) return $"last bid suit was {suit.ToSymbol()}";
			// TODO: Need to handle other cases...
			return null;
		}
	}

}
