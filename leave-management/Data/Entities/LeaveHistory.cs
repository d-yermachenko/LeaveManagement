using System;

namespace LeaveManagement.Data.Entities {
    public class LeaveHistory {
        public long Id { get; set; }
        public Employee RequestingEmployee { get; set; }
        public string RequestingEmployeeId { get; set; }
        public LeaveType LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDateDate { get; set; }
        public DateTime RequestedDateDate { get; set; }
        public DateTime ActionedDateTime { get; set; }
        public bool? Approuved { get; set; }
        public Employee ApprouvedBy { get; set; }
        public string ArrpouvedById { get; set; }
    }
}
