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

        internal void UpdateShownSuits(Call call, PositionState ps, HandSummary hs)
        {
            foreach (var suit in Card.Suits)
            {
                if (!_firstToShow.ContainsKey(suit) && hs.Suits[suit].Shape != null)
                {
                    int minRequired = (call is Bid bid && bid.Suit == suit) ? 2 : 4;
                    (int Min, int Max) s = ((int, int))hs.Suits[suit].Shape;
                    if (s.Min >= minRequired)
                    {
                        _firstToShow[suit] = ps;
                    }
                }
            }
        }

        public PositionState FirstToShow(Suit suit)
        {
            PositionState firstToShow;
            _firstToShow.TryGetValue(suit, out firstToShow);
            return firstToShow;
        }

        // TODO: Probably update this after MakeCall() instead of this silly search...
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
