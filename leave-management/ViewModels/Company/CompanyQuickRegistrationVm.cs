using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.Company {
    public class CompanyQuickRegistrationVm {

        [Required(ErrorMessage = "Company name is required field")]
        [Display(Name = "Company name", Description = "This is name of your company.")]
        [DataType(DataType.EmailAddress)]
        public string CompanyName { get; set; }


        [Required(ErrorMessage = "This email is required for registration purposes.")]
        [Display(Name = "Email of company admin", Description = @"Email used for registration company. 
            This email will be used like corporate email, but later you can change it.
                Also it will used like user name of company admin")]
        public string CompanyEmail { get; set; }

        [Required(ErrorMessage = "Company administrator user name")]
        [Display(Name = "Admin user name", Description = @"Administrator user name")]
        public string CompanyAdminUserName { get; set; }

    }
}
