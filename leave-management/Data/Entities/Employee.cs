using Microsoft.AspNetCore.Identity;
using System;


namespace LeaveManagement.Data.Entities {
    public class Employee : IdentityUser {
        public string Title { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string TaxRate  { get; set; }
        
        public DateTime DateOfBirth  { get; set; }
        
        public DateTime AncienityDate  { get; set; }
    }
}
