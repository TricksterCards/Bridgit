using System;
using System.Diagnostics;

namespace BridgeBidding
{
    public class PassIn4thSeat : HandConstraint
    {
        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            if (ps.Seat != 4) return false;
            if (hs.HighCardPoints == null) return true;

            (int Min, int Max) hcp = ((int, int))hs.HighCardPoints;
            var spadeShape = hs.Suits[Suit.Spades].GetShape();

      
            return (hcp.Max + spadeShape.Max < 15);            
        }
    }
}
