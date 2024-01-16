namespace BridgeBidding
{
    public interface IBiddingSystem
    {

        BidChoices GetBidChoices(PositionState positionState);  
    }

}
