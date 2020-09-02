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
        [Display(Name = "Local administrator", Description = "Section admin")]
        LocalAdministrator = 4,
        [Display(Name = "Global administrator", Description = "App admin")]
        Administrator = 8
    }
}