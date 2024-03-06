namespace BridgeBidding
{
    internal class Break : HandConstraint
    {
        // TODO: Implement static break class !!!
        public string Name { get; private set; }
        public int CountPublic = 0;
        public int CountPrivate = 0;
        public Break(string name)
        {
            this.Name = name;
        }
        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {
            var pairSummary = new PairSummary(ps);
            var oppsSummary = new PairSummary(ps.LeftHandOpponent);
            if (hs == ps.PublicHandSummary)
            {
                CountPublic++;
            }
            else
            {
                CountPrivate++;
            }
            return true;
        }
    }

    public class StaticBreak : StaticConstraint
    {
        public string Name { get; private set; }
        public StaticBreak(string name)
        {
            this.Name = name;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            return true;
        }
    }
}
