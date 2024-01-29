using System.Collections.Generic;
using System.Dynamic;

namespace BridgeBidding
{
    public enum Pair { NS, EW }

    public class PairState
    {
        public Pair Pair { get; }
        // TODO: This should ideally not be public Set.  Perhaps PairState becomes part of PositionState...  Then could be protected...
        public PairAgreements Agreements { get; set; }
        public IBiddingSystem BiddingSystem { get; }

        public bool AreVulnerable { get; }

        // TODO: Is this part of the pair agreements?? 
        public bool InGameForcingAuction { get; set; }

        public PairState(Pair pair, IBiddingSystem biddingSystem, Vulnerable vulnerable)
        {
            this.Pair = pair;
            this.Agreements = new PairAgreements();
            this.BiddingSystem = biddingSystem;
            this.AreVulnerable = (vulnerable == Vulnerable.All ||
                        (vulnerable == Vulnerable.NS && pair == Pair.NS) ||
                        (vulnerable == Vulnerable.EW && pair == Pair.EW));
            this.InGameForcingAuction = false;
        }

    }
}
