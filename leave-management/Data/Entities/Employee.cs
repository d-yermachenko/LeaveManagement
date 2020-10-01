using Microsoft.AspNetCore.Identity;
using System;


namespace LeaveManagement.Data.Entities {
    public class Employee : IdentityUser {

        public Employee() : base() {
            ;
        }

        public string DisplayName { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string TaxRate  { get; set; }

        public string ContactMail { get; set; }
        
        public DateTime DateOfBirth  { get; set; }
        
        public DateTime EmploymentDate  { get; set; }

        public DateTime CurrentConnectionDate { get; set; }

        public DateTime LastConnectionDate { get; set; }

        public Employee Manager { get; set; }

        public string ManagerId { get; set; }

        public Company Company { get; set; }

        public int? CompanyId { get; set; }
    }
}
