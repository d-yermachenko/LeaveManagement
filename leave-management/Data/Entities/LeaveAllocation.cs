using System;


namespace LeaveManagement.Data.Entities {
    public class LeaveAllocation {

        public long Id { get; set; }

        public uint NumberOfDays { get; set; }

        public DateTime DateCreated { get; set; }

        public Employee AllocationEmployee { get; set; }

        public string AllocationEmployeeId { get; set; }

        public LeaveType AllocationLeaveType { get; set; }

        public int AllocationLeaveTypeId { get; set; }
    }
}
