using System.Runtime.CompilerServices;

namespace BridgeBidding
{
    public class IsReverseBid : StaticConstraint
    {
     
        bool _desiredValue;

        // Round == 0 means only check for role, otherwise check for both.
        public IsReverseBid(bool desiredValue)
        {
            _desiredValue = desiredValue;
        }

        // This method is also used by ReverseShape (which is a DynamicConstraint) to make sure
        // the call is actually an opener's reverse.
        public static bool IsOpenerReverseBid(Call call, PositionState ps)
        {
            return  (ps.Role == PositionRole.Opener &&
                     ps.BiddingState.OpeningBid is Bid openingBid &&
                    call is Bid thisBid &&
                    thisBid.Level > openingBid.Level &&
                    thisBid.Strain != Strain.NoTrump && 
                    openingBid.Strain != Strain.NoTrump &&
                    thisBid.Suit > openingBid.Suit &&
                    !ps.PairState.Agreements.Strains[thisBid.Strain].Shown);
        }

        public override bool Conforms(Call call, PositionState ps)
        {
            // TODO: For now this only works for an opener.  If we want to do reverses for overcaller or responder
            // this logic will need to change.
            return (_desiredValue == IsOpenerReverseBid(call, ps));
        }
    }
}
