using System.Diagnostics;

namespace BridgeBidding
{
    public class HasReverseShape : HandConstraint
    {

        protected Suit? OpenSuit(PositionState ps)
        {
            if (ps.BiddingState.OpeningBid is Bid openingBid)
                return openingBid.Suit;
            return null;
        }

        protected Suit? BidSuit(Call call)
        {
            if (call is Bid bid)
                return bid.Suit;
            return null;
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            if (ps.IsReverse(call))
            {
                // TODO: This only works for opener...  Needs to work for advancer, responder, overecaller, etc.
                if (OpenSuit(ps) is Suit openSuit &&
                    BidSuit(call) is Suit bidSuit)
                {
                    var openingShape = hs.Suits[openSuit].GetShape();
                    var reverseSuitShape = hs.Suits[bidSuit].GetShape();
                    return (reverseSuitShape.Max > 3 && reverseSuitShape.Min < openingShape.Max);
                }
            }
            return false;
        }
    }

	public class ShowsReverseShape : HasReverseShape, IShowsHand
	{
	    public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
		{
           if (OpenSuit(ps) is Suit openSuit &&
               BidSuit(call) is Suit bidSuit)
            {
                var openingShape = ps.PublicHandSummary.Suits[openSuit].GetShape();
                // TODO: Should be min of known current or 5 and opener min shouod be 1 more!
                showHand.Suits[bidSuit].ShowShape(4, openingShape.Max - 1);
                showHand.Suits[openSuit].ShowShape(5, openingShape.Max);
            }
		}

    }

}
