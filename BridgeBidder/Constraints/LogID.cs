using System.Dynamic;
using System.Runtime.InteropServices;

namespace BridgeBidding
{
    // This needs to be it's own class because the log logic searches for this constraint and uses
    // the LogDescription to log the call.
    public class LogID : StaticConstraint
    {
        string ID;
        public LogID(string id)
        {   
            ID = id;
        }
        public override bool Conforms(Call call, PositionState ps)
        {
            return true;
        }
        
        public static string GetID(BidRule rule)
        {
            foreach (var constraint in rule.Constraints)
            {
                if (constraint is LogID logID)
                {
                    // TODO: Multiple LOGID constraints are not supported.  Add in the future if it is needed.
                    return logID.ID;
                }
            }
            return null;
        }

        // We dont want this showing up in the log constraints.  The Log logic uses the GetID method to retrieve the ID
        public override string GetLogDescription(Call call, PositionState ps)
        {
            return null;
        }
    }
}