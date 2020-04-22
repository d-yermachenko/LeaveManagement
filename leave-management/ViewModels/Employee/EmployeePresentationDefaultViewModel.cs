using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Models.ViewModels {
    public class EmployeePresentationDefaultViewModel {

        public string UserName { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string TaxRate { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime AncienityDate { get; set; }
    }
}
