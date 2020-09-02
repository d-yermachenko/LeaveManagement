using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.LeaveRequest {
    public class LeaveSold {
        public int LeaveTypeId { get; set; }

        [Display(Name ="Reason to leave")]
        public string LeaveTypeName { get; set; }

        [Display(Name ="Default allocation", Description ="Base rate of days which can be allocated")]
        public int DefaultDays { get; set; }

        [Display(Name = "Your allocation", Description = "Number of days was allocated to you")]
        public int AllocatedDays { get; set; }

        [Display(Name = "Approuved", Description = "Number of days, that was approuved")]
        public int ApprouvedDays { get; set; }

        [Display(Name = "Not used", Description = @"Number of days, which was approuved, but waiting for being used.
             They became used when period of leave begins")]
        public int ApprouvedNotUsed { get; set; }

        [Display(Name = "Used", Description = @"Number of days, which was approuved, and period of leave began")]
        public int UsedDays { get; set; }

        [Display(Name = "Rejected", Description = @"Number of days, which were rejected")]
        public int RejectedDays { get; set; }

        [Display(Name = "Pending", Description = @"Number of days in not still reviewed request")]
        public int PendingDays { get; set; }

        [Display(Name = "Sold of days", Description = @"Days that you can request for leave")]
        public int RestOfDays { get; set; }
    }

    public class EmployeeLeaveRequestsViewModel {
        public IList<LeaveSold> LeaveAllocations { get; set; }

        public IList<LeaveRequestDefaultViewModel> LeaveRequests { get; set; }
    }
}
