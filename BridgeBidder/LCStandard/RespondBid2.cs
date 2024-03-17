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
                Shows(Bid._2S, RaisePartner(), Points(MinimumHand)),
                Shows(Bid._3S, RaisePartner(jump: 1), Points(MediumHand)),
                Shows(Bid._4S, RaisePartner(jump: 1), Points(RaiseTo4M)),

                Shows(Bid._2C, IsRebid, Shape(6, 11), Points(MinimumHand)),
                Shows(Bid._2D, IsRebid, Shape(6, 11), Points(MinimumHand)),
                Shows(Bid._2H, IsRebid, Shape(6, 11), Points(MinimumHand)),
                Shows(Bid._2S, IsRebid, Shape(6, 11), Points(MinimumHand)),


				// TODO: Make these dependent on pair points.
                Shows(Bid._3C, IsRebid, Shape(6, 11), Points(MediumHand)),
                Shows(Bid._3D, IsRebid, Shape(6, 11), Points(MediumHand)),
                Shows(Bid._3H, IsRebid, Shape(6, 11), Points(MediumHand)),
                Shows(Bid._3S, IsRebid, Shape(6, 11), Points(MediumHand)),


                Shows(Bid._1NT, Points(MinimumHand)),
                
             /// TODO: MORE PASSING MORE OFTEN...   Shows(Call.Pass, Points(MinimumHand), ForcedToBid(false), )
                Shows(Bid._2C, Fit8Plus, IsNotRebid, IsForcedToBid, Points(MinimumHand)),
                Shows(Bid._2D, Fit8Plus, IsNotRebid, IsForcedToBid, Points(MinimumHand)),
                Shows(Bid._2H, Fit8Plus, IsNotRebid, IsForcedToBid, Points(MinimumHand)),
                Shows(Bid._2S, Fit8Plus, IsNotRebid, IsForcedToBid, Points(MinimumHand)),

                Shows(Bid._3C, Fit8Plus, IsNotRebid, IsNonJump, IsForcedToBid, Points(MinimumHand)),
                Shows(Bid._3D, Fit8Plus, IsNotRebid, IsNonJump, IsForcedToBid, Points(MinimumHand)),
                Shows(Bid._3H, Fit8Plus, IsNotRebid, IsNonJump, IsForcedToBid, Points(MinimumHand)),
                Shows(Bid._3S, Fit8Plus, IsNotRebid, IsNonJump, IsForcedToBid, Points(MinimumHand))


            };
            bids.AddRange(Compete.CompBids(ps));
            return bids;
        }

        public static IEnumerable<CallFeature> OpenerInvitedGame(PositionState ps)
        {
            var bids = new List<CallFeature>()
            {
                Shows(Bid._4H, Fit8Plus, PairPoints(PairGame)),
                Shows(Bid._4S, Fit8Plus, PairPoints(PairGame)),
                Shows(Call.Pass)
            };
            // TODO: Competative bids here too?  Seems silly since restricted raise
            return bids;
        }
    }

}

 