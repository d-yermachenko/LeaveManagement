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

        public string CompanyData { get; set; }
    }
}
