using System.Diagnostics;

namespace BridgeBidding
{
    public class ForcedToBid : StaticConstraint
    {
        public bool _desiredValue;
        public ForcedToBid(bool desiredValue)
        {
            this._desiredValue = desiredValue;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            return _desiredValue == ps.ForcedToBid;
        }
    }
}