using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;

namespace BridgeBidding
{


    public class PairState
    {
        public BiddingState BiddingState { get; }
        public Pair Pair { get; }
        // TODO: This should ideally not be public Set.  Perhaps PairState becomes part of PositionState...  Then could be protected...
     //   public PairAgreements Agreements { get; set; }
        public IBiddingSystem BiddingSystem { get; }

        public bool AreVulnerable { get; }

        public bool ForcedToGame { get; private set; } = false;
       
        private PositionState _forcedPosition = null;
        private int _forcedThroughRound = 0;

        public bool IsForcedToBid(PositionState ps) => (ps == _forcedPosition && ps.BidRound <= _forcedThroughRound);

        private Dictionary<Suit, PositionState> _firstToShow = new Dictionary<Suit, PositionState>();

        public Suit? LastShownSuit { get; private set; }

        public bool HaveShownSuit(Suit suit) => _firstToShow.ContainsKey(suit);

        public IEnumerable<Suit> ShownSuits => _firstToShow.Keys;

        public PairState(BiddingState biddingState, Pair pair, IBiddingSystem biddingSystem, Vulnerable vulnerable)
        {
            this.BiddingState = biddingState;
            this.Pair = pair;
          //  this.Agreements = new PairAgreements();
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
                if (!_firstToShow.ContainsKey(suit) && hs.Suits[suit].Shape.HasValue)
                {
                    int minRequired = (call is Bid bid && bid.Suit == suit) ? 2 : 4;
                    if (hs.Suits[suit].Shape.Value.Min >= minRequired)
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

        internal void UpdateLastShownSuit(Call call, PositionState ps, HandSummary hs)
        {
            foreach (var suit in Card.Suits)
            {
                if (hs.Suits[suit].Shape.HasValue)
                {
                    int minRequired = (call is Bid bid && bid.Suit == suit) ? 2 : 4;
                    if (hs.Suits[suit].Shape.Value.Min >= minRequired)
                    {
                        LastShownSuit = suit;
                    }
                }
            }
        }


        public void UpdatePairProperties(CallDetails callDetails)
        {
            if (callDetails.Properties != null)
            {
                if (callDetails.Properties.ForcingToGame)
                {
                    ForcedToGame = true;
                }
                // TODO: Is this right?  Shoule it be BidRound + 1?
                if (callDetails.Properties.Forcing1Round)
                {
                    _forcedPosition = callDetails.PositionState.Partner;
                    _forcedThroughRound = callDetails.PositionState.Partner.BidRound;
                }
            }
        }
/*
        // TODO: Probably update this after MakeCall() instead of this silly search...
        // TODO: For transfers and michales/unusual 2NT this does not show the strain of the bid.  This is a problem.
        // This really needs to be LastShownStrain() and it needs to be updated when the call is made.
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
*/
    }
}
