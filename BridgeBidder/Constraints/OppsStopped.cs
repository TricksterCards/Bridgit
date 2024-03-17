using System.Data;
using System.Diagnostics;
using System.Linq;

namespace BridgeBidding
{

    public class HasOppsStopped : HandConstraint
    {
        protected bool _desiredValue;
        public HasOppsStopped(bool desiredValue)
        {
            this._desiredValue = desiredValue;
        }

        public override bool Conforms(Call call, PositionState ps, HandSummary hs)
        {

           // var oppsSummary = PairSummary.Opponents(ps);
            foreach (var suit in ps.OppsPairState.ShownSuits)
            {
                // These variables can be true, false, or null, so look for specific values.
                var thisStop = hs.Suits[suit].Stopped;
                var partnerStop = ps.Partner.PublicHandSummary.Suits[suit].Stopped;
                // If either hand has it stopped then we are good
                if (thisStop != true && partnerStop != true)
                {
                    if (thisStop == false) { return !_desiredValue; }
                    // At this point, we know that thisStop is null (unknown value) and partnerStop is either false or unknown.
                    // This means that the hand summary COULD have the suit stopped - we don't know.  If we were dealing with the
                    // actual hand then thisStop will be either true or false.
                    Debug.Assert(thisStop == null);
                }
            }
            return _desiredValue;
        }
    }

    public class ShowsOppsStopped : HasOppsStopped, IShowsHand, IDescribeConstraint
    {
        public ShowsOppsStopped(bool desiredValue) : base(desiredValue) { }

        public void ShowHand(Call call, PositionState ps, HandSummary.ShowState showHand)
        {
            if (_desiredValue)
            {
                foreach (var suit in ps.OppsPairState.ShownSuits)
                {
                    // These variables can be true, false, or null, so look for specific values.
                    var partnerStop = ps.Partner.PublicHandSummary.Suits[suit].Stopped;
                    if (partnerStop == null)
                    {
                        Debug.Assert(_desiredValue);    // TODO: How to show suits not stopped.  Complex if more than one suit...
                        showHand.Suits[suit].ShowStopped(true);
                    }
                }
            }
        }

        string IDescribeConstraint.Describe(Call call, PositionState ps)
        {
            var numOppsSuits = ps.OppsPairState.ShownSuits.Count();
            if (numOppsSuits > 0) 
            {
                if (_desiredValue)
                {
                    return numOppsSuits == 1 ? "opponents suit stopped" : "opponents suits stopped";
                }
                else 
                {
                    return "opponent suit not stopped";
                }
            }
            return null;
        }
    }
}
