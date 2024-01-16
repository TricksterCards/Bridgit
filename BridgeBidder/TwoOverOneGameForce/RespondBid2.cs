using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;


namespace BridgeBidding
{
    public class RespondBid2 : Respond
    {

        public static IEnumerable<BidRule> Rebid(PositionState ps)
        {
            var bids = new List<BidRule>
            {
                PartnerBids(OpenBid3.ThirdBid),

                // Opener could have bid 1S.  Support at the right level...
                Nonforcing(Bid.TwoSpades, RaisePartner(), Points(MinimumHand)),
                Nonforcing(Bid.ThreeSpades, RaisePartner(2), Points(MediumHand)),
                Signoff(Bid.FourSpades, RaisePartner(2), Points(RaiseTo4M)),

                Nonforcing(Bid.TwoClubs, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid.TwoDiamonds, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid.TwoHearts, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid.TwoSpades, Shape(6, 11), Points(MinimumHand)),


				// TODO: Make these dependent on pair points.
                Invitational(Bid.ThreeClubs, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid.ThreeDiamonds, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid.ThreeHearts, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid.ThreeSpades, Shape(6, 11), Points(MediumHand)),


                Nonforcing(Bid.OneNoTrump, Points(MinimumHand)),
                
             /// TODO: MORE PASSING MORE OFTEN...   Signoff(Call.Pass, Points(MinimumHand), ForcedToBid(false), )
                Signoff(Bid.TwoClubs, Fit(), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.TwoDiamonds, Fit(), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.TwoHearts, Fit(), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.TwoSpades, Fit(), ForcedToBid(), Points(MinimumHand)),

                Signoff(Bid.ThreeClubs, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.ThreeDiamonds, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.ThreeHearts, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand)),
                Signoff(Bid.ThreeSpades, Fit(), Jump(0), ForcedToBid(), Points(MinimumHand))


            };
            bids.AddRange(Compete.CompBids(ps));
            return bids;
        }

        public static IEnumerable<BidRule> OpenerInvitedGame(PositionState ps)
        {
            var bids = new List<BidRule>()
            {
                Signoff(Bid.FourHearts, Fit(), PairPoints(PairGame)),
                Signoff(Bid.FourSpades, Fit(), PairPoints(PairGame))
            };
            // TODO: Competative bids here too?  Seems silly since restricted raise
            return bids;
        }
    }

}

 