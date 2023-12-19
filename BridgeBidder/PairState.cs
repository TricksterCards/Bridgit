using System.Collections.Generic;
using System.Dynamic;

namespace BridgeBidding
{
    public enum Pair { NorthSouth, EastWest }

    public class PairState
    {
        public Pair Pair { get; }
        // TODO: This should ideally not be public Set.  Perhaps PairState becomes part of PositionState...  Then could be protected...
        public PairAgreements Agreements { get; set; }
        public IBiddingSystem BiddingSystem { get; }

        public bool Vulnerable { get; private set;}

        public PairState(Pair pair, IBiddingSystem biddingSystem, HashSet<Pair> vulPairs)
        {
            this.Pair = pair;
            this.Agreements = new PairAgreements();
            this.BiddingSystem = biddingSystem;
            this.Vulnerable = vulPairs.Contains(pair);
        }

    }
}
