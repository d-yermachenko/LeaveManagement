using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Data.Entities {
    public class Company {
        public int Id { get; set; }

        public string CompanyName { get; set; }

        public DateTime CompanyRegistrationDate { get; set; }

        public DateTime CompanyCreationDate { get; set; }

        public string TaxId { get; set; }

        public string CompanyState { get; set; }

        public string CompanyZipCode { get; set; }

        public string CompanyEmail { get; set; }

        public string CompanyPostAddress { get; set; }

        public string CompanyPublicComment { get; set; }

        public string CompanyProtectedComment { get; set; }

        public bool Active { get; set; } = true;

        public bool EnableLockoutForEmployees { get; set; } = false;
    }
}
