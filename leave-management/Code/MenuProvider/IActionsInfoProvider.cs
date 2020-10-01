using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.MenuProvider {
    public interface IActionsInfoProvider {
        ICollection<ActionInfo> GetAllowedActions();
    }
}
