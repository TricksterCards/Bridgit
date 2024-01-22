using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{

	public class PartnerCalls : CallFeature
	{
		public PositionCallsFactory PartnerBids{ get; private set; }

        public PartnerCalls(Call call, PositionCallsFactory partnerBids, params StaticConstraint[] constraints) :
			base(call, constraints)
        {
			Debug.Assert(partnerBids != null);
            this.PartnerBids = partnerBids;
        }
    }

}
