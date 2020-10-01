using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement {

    [Flags]
    public enum UserRoles {
        None = 0,
        [Display(Name = "Employee")]
        Employee = 1,
        [Display(Name = "HR Manager", Description = "Human resources manager")]
        HRManager = 2,
        [Display(Name = "Local administrator", Description = "Company administrator. Can manage all data inside the company, but can't create company")]
        CompanyAdministrator = 4,
        [Display(Name = "Application administrator", Description = "Application administrator. Can add or remove company, but cant manage leaves and normal employees ")]
        AppAdministrator = 8
    }
}