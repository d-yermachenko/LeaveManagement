using System;

namespace LeaveManagement.Data.Entities {
    public class LeaveRequest {
        public long Id { get; set; }
        public Employee RequestingEmployee { get; set; }
        public string RequestingEmployeeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ActionedDateTime { get; set; }
        public bool? Approuved { get; set; }
        public Employee ApprouvedBy { get; set; }
        public string ApprouvedById { get; set; }

        public string RequestComment { get; set; }

        public string ValidationComment { get; set; }

        public bool RequestCancelled { get; set; }
    }
}
