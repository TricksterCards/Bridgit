namespace BridgeBidding
{

    public class LCStandard : Bidder, IBiddingSystem
    {

        public PositionCalls GetPositionCalls(PositionState ps)
        {
            if (ps.Role == PositionRole.Opener && ps.RoleRound == 1)
            {
                return Open.GetPositionCalls(ps);
            }
            else if (ps.Role == PositionRole.Overcaller && ps.RoleRound == 1)
            {
                return Overcall.GetPositionCalls(ps);
            }
            else
            {
                var calls = new PositionCalls(ps);
                calls.AddRules(Compete.CompBids);
                return calls;
            }
        }


		public static (int, int) PairGameInvite = (23, 24);
		public static (int, int) PairGame = (25, 31);


        // TODO: This is not a great name.  Not exactly right.  Fix later.....
        public static (int, int) LessThanOvercall = (0, 17);
        public static (int, int) Overcall1Level = (7, 17);
        public static (int, int) OvercallStrong2Level = (13, 17);
        public static (int, int) OvercallWeak2Level = (7, 11);
        public static (int, int) OvercallWeak3Level = (7, 11);




    }
}
