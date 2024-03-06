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

        public static IEnumerable<CallFeature> SecondBid(PositionState ps)
        {
            var bids = new List<CallFeature>
            {
                PartnerBids(OpenBid3.ThirdBid),

                // Opener could have bid 1S.  Support at the right level...
                Nonforcing(Bid._2S, RaisePartner(), Points(MinimumHand)),
                Nonforcing(Bid._3S, RaisePartner(jump: 1), Points(MediumHand)),
                Signoff(Bid._4S, RaisePartner(jump: 1), Points(RaiseTo4M)),

                Nonforcing(Bid._2C, Rebid, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid._2D, Rebid, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid._2H, Rebid, Shape(6, 11), Points(MinimumHand)),
                Nonforcing(Bid._2S, Rebid, Shape(6, 11), Points(MinimumHand)),


				// TODO: Make these dependent on pair points.
                Invitational(Bid._3C, Rebid, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid._3D, Rebid, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid._3H, Rebid, Shape(6, 11), Points(MediumHand)),
                Invitational(Bid._3S, Rebid, Shape(6, 11), Points(MediumHand)),


                Nonforcing(Bid._1NT, Points(MinimumHand)),
                
             /// TODO: MORE PASSING MORE OFTEN...   Signoff(Call.Pass, Points(MinimumHand), ForcedToBid(false), )
                Signoff(Bid._2C, Fit(), NotRebid, ForcedToBid, Points(MinimumHand), ShowsTrump),
                Signoff(Bid._2D, Fit(), NotRebid, ForcedToBid, Points(MinimumHand), ShowsTrump),
                Signoff(Bid._2H, Fit(), NotRebid, ForcedToBid, Points(MinimumHand), ShowsTrump),
                Signoff(Bid._2S, Fit(), NotRebid, ForcedToBid, Points(MinimumHand), ShowsTrump),

                Signoff(Bid._3C, Fit(), NotRebid, NonJump, ForcedToBid, Points(MinimumHand), ShowsTrump),
                Signoff(Bid._3D, Fit(), NotRebid, NonJump, ForcedToBid, Points(MinimumHand), ShowsTrump),
                Signoff(Bid._3H, Fit(), NotRebid, NonJump, ForcedToBid, Points(MinimumHand), ShowsTrump),
                Signoff(Bid._3S, Fit(), NotRebid, NonJump, ForcedToBid, Points(MinimumHand), ShowsTrump)


            };
            bids.AddRange(Compete.CompBids(ps));
            return bids;
        }

        public static IEnumerable<CallFeature> OpenerInvitedGame(PositionState ps)
        {
            var bids = new List<CallFeature>()
            {
                Signoff(Bid._4H, Fit(), PairPoints(PairGame)),
                Signoff(Bid._4S, Fit(), PairPoints(PairGame)),
                Signoff(Call.Pass)
            };
            // TODO: Competative bids here too?  Seems silly since restricted raise
            return bids;
        }
    }

}

 