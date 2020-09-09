using System;


namespace LeaveManagement.Data.Entities {
    public class LeaveType {
        public int Id { get; set; }

        public string LeaveTypeName { get; set; }

        public DateTime DateCreated { get; set; }

        public int DefaultDays { get; set; }

        public int CompanyId { get; set; }

        public Company Company { get; set; }

    }
}
