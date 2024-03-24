using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
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

        public static PositionCalls SecondBid2Over1(PositionState ps)
        {
            var choices = new PositionCalls(ps);
            if (ps.PairState.TrumpSuit is Suit trump)
            {
                Debug.Assert(trump.IsMajor());  // For the 2nd bid we only expect major suits for trump.
                // We have agreed on a trump suit.  We go quickly to game with a minimum hand.
                // Bid controls with a better hand.
                choices.AddRules(
                    Shows(new Bid(4, trump), PairPoints(25, 27))
                );
            }
            else
            {
                // It is possible that we artificially bid 2/1 with support for opener's major suit
                // If that is the case show support now.
                choices.AddRules(
                    Properties(new[] {Bid._2H, Bid._2S, Bid._3H, Bid._3S, Bid._4H, Bid._4S }, agreeTrump: true, onlyIf: IsPartnersSuit),
                    Shows(Bid._4H, IsPartnersSuit, Fit8Plus, DummyPoints(12, 13)),
                    Shows(Bid._4S, IsPartnersSuit, Fit8Plus, DummyPoints(12, 13)),
                    Shows(Bid._2H, IsPartnersSuit, Fit8Plus, DummyPoints(14, 40)),
                    Shows(Bid._2S, IsPartnersSuit, Fit8Plus, DummyPoints(14, 40)),
                    Shows(Bid._3H, IsPartnersSuit, IsNonJump, Fit8Plus, DummyPoints(14, 40)),
                    Shows(Bid._3S, IsPartnersSuit, IsNonJump, Fit8Plus, DummyPoints(14, 40))
                );

            }
            // We want to do blackwood LAST since strong control showing bids take prioirity.
            choices.AddRules(Blackwood.InitiateConvention);
            return choices;
        }
    }

}

 