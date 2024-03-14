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
        public CallProperties(Call call, PositionCallsFactory partnerBids, bool forcing1Round, bool forcingToGame, 
                             params StaticConstraint[] constraints) :
            base(call, constraints)
        {
            this.PartnerBids = partnerBids;
            this.Forcing1Round = forcing1Round;
            this.ForcingToGame = forcingToGame;
        }
    }


}
