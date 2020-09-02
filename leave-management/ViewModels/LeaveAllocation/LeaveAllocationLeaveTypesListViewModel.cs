using System.Collections.Generic;
using LeaveManagement.ViewModels.LeaveType;

namespace LeaveManagement.ViewModels.LeaveAllocation {
    public class LeaveAllocationLeaveTypesListViewModel {
        public LeaveAllocationLeaveTypesListViewModel() {
            AvalableLeaveTypes = new List<LeaveTypeNavigationViewModel>();
        }

        public List<LeaveTypeNavigationViewModel> AvalableLeaveTypes { get; set; }
    }
}
