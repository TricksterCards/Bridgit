namespace BridgeBidding
{


    public abstract class Constraint
    {
      //  public bool StaticConstraint = false;
      //  public abstract bool Conforms(Call call, PositionState ps, HandSummary hs);


        public static Suit? GetSuit(Suit? s, Call call)
        {
            if (s != null) { return s; }
            if (call is Bid bid)
            {
                return bid.Suit;
            }
            return null;
        }

        public static Strain? GetStrain(Strain? strain, Call call)
        {
            if (strain != null) { return strain; }
            if (call is Bid bid) { return bid.Strain; }
            return null;
        }

    }

    public abstract class StaticConstraint: Constraint
    {
        public abstract bool Conforms(Call call, PositionState ps);
    }

    public abstract class DynamicConstraint: Constraint
    {
        public abstract bool Conforms(Call call, PositionState ps, HandSummary hs);
    }


    public interface IShowsState 
    {
        void ShowState(Call call, PositionState ps, HandSummary.ShowState showHand, PairAgreements.ShowState showAgreements);
    }


}


