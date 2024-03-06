using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace BridgeBidding
{


    public class PairState
    {
        public BiddingState BiddingState { get; }
        public Pair Pair { get; }
        // TODO: This should ideally not be public Set.  Perhaps PairState becomes part of PositionState...  Then could be protected...
        public PairAgreements Agreements { get; set; }
        public IBiddingSystem BiddingSystem { get; }

        private bool _inFirstToShow = false;
        public bool AreVulnerable { get; }

        private Dictionary<Suit, PositionState> _firstToShow = new Dictionary<Suit, PositionState>();

        // TODO: Is this part of the pair agreements?? 
     //   public bool InGameForcingAuction { get; set; }

        public PairState(BiddingState biddingState, Pair pair, IBiddingSystem biddingSystem, Vulnerable vulnerable)
        {
            this.BiddingState = biddingState;
            this.Pair = pair;
            this.Agreements = new PairAgreements();
            this.BiddingSystem = biddingSystem;
            this.AreVulnerable = (vulnerable == Vulnerable.All ||
                        (vulnerable == Vulnerable.NS && pair == Pair.NS) ||
                        (vulnerable == Vulnerable.EW && pair == Pair.EW));
        //    this.InGameForcingAuction = false;
        }

        public PositionState FirstToShow(Suit suit)
        {
            if (_firstToShow.ContainsKey(suit)) return _firstToShow[suit];
            if (_inFirstToShow) return null;
            // Now we must search for the first position that has shown a suit.  "Shown" is
            // defined as bidding a suit and showing a minimum of 2 cards in that suit alone,
            // or any bid that shows a minium of 4 cards in a suit.  All suits are considered
            // shown if they are 4 cards or more (so michaels over a minor shows Hearts and Spades).
            // To determint this, each CallDetails HandSummary is checked.
            _inFirstToShow = true;  // Kind of a hack but good enough for now...
            foreach (var callDetails in BiddingState.GetAuction())
            {
                // TODO: This HasRules check may be unnecessary...
                if (callDetails.PositionState.PairState == this && callDetails.HasRules)
                {
                    var handSummary = callDetails.ShowHand();
                    int minRequired = (callDetails.Call is Bid bid && bid.Suit == suit) ? 2 : 4;
                    var shape = handSummary.Suits[suit].Shape;
                    if (shape != null)
                    {
                        (int Min, int Max) s = ((int, int))shape;
                        if (s.Min >= minRequired)
                        {
                            _firstToShow[suit] = callDetails.PositionState;
                            _inFirstToShow = false;
                            return callDetails.PositionState;
                        }
                    }
                }
            }
            _inFirstToShow = false;
            return null;
        }

        public Strain? LastBidStrain()
        {
            var auction = BiddingState.GetAuction().Reverse<CallDetails>();
            foreach (var callDetails in auction)
            {
                if (callDetails.PositionState.PairState == this && callDetails.Call is Bid bid)
                {
                    return bid.Strain;
                }
            }
            return null;
        }

    }
}
