using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Mvc;
using LeaveManagement.ViewModels.Employee;

namespace LeaveManagement.ViewModels.LeaveAllocation {
    public class LeaveAllocationEditionViewModel {
        [HiddenInput(DisplayValue = false)]
        public long Id { get; set; }

        [Required]
        [Display(Name = "Duration of leave", Description ="Number of days of this vacation", ShortName ="# Days", Prompt ="")]
        public int NumberOfDays { get; set; }

        [Required(AllowEmptyStrings =false, ErrorMessage ="Must be not empty")]
        [Display(Name ="Allocation date")]
        [DataType(DataType.Date, ErrorMessage ="Must be valid Date")]
        public DateTime DateCreated { get; set; }

        [Display(Description ="Employee who wants to leave", Name ="Employee")]
        public EmployeePresentationDefaultViewModel AllocationEmployee { get; set; }

        [Display(Description = "Employee who wants to leave", Name = "Employee")]
        public string AllocationEmployeeId { get; set; }

        [Display(Description = "List of employees", Name = "Employees")]
        public IEnumerable<SelectListItem> Employees { get; set; }

        [Display(Description = "Employee who wants to leave", Name = "Employee")]
        public LeaveTypeNavigationViewModel AllocationLeaveType { get; set; }

        [Display(Description = "Leave type", Name = "Leave type")]
        public int AllocationLeaveTypeId { get; set; }

        [Display(Description = "Leace Types", Name = "Leave types")]
        public IEnumerable<SelectListItem> AllocationLeaveTypes { get; set; }

        [Display(Description = "Period of leave", Name = "Period")]
        public int Period { get; set; }

    }
}
