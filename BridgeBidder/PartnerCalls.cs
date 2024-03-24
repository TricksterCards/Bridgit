using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BridgeBidding
{

	public class CallProperties : CallFeature
	{
		public PositionCallsFactory PartnerBids{ get; private set; }
        public bool Forcing1Round { get; }
        public bool ForcingToGame { get; }

        public Suit? TrumpSuit { get; }
        public CallProperties(Call call, PositionCallsFactory partnerBids, bool forcing1Round, bool forcingToGame, 
                             bool agreeTrump, Suit? trump = null, params StaticConstraint[] constraints) :
            base(call, constraints)
        {
            this.PartnerBids = partnerBids;
            this.Forcing1Round = forcing1Round;
            this.ForcingToGame = forcingToGame;
            if (agreeTrump)
            {
                Debug.Assert(trump == null);
                if (call is Bid bid)
                {
                    this.TrumpSuit = bid.Suit;
                }
                else
                {
                    Debug.Fail("AgreeTrump is only valid for bids.");
                }
            }
            else if (trump != null)
            {
                this.TrumpSuit = trump;
            }
        }
    }


}
