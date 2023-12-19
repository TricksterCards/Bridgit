namespace BridgeBidding
{
    public class PassEndsAuction : StaticConstraint
    {
        private bool _desiredValue;
        public PassEndsAuction(bool desiredValue) 
        {
            this._desiredValue = desiredValue;
        }

        public override bool Conforms(Call call, PositionState ps)
        {
            return (_desiredValue == ps.BiddingState.Contract.PassEndsAuction);
        }
    }
}
