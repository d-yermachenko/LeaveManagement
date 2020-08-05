using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using LeaveManagement.ViewModels.LeaveType;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using LeaveManagement.ViewModels.Employee;

namespace LeaveManagement.ViewModels.LeaveAllocation {
    public class LeaveAllocationPresentationViewModel {

        [Display(Name ="ID", Description ="Identity number of this allocation", ShortName ="#")]
        public long Id { get; set; }

        [Display(Name = "Duration of leave", Description = "Duration of leave", ShortName = "Duration")]
        public uint NumberOfDays { get; set; }

        [Display(Name = "Date of apply", Description = "Date when employee asked for leave", ShortName = "Appl. Date")]
        public DateTime DateCreated { get; set; }

        [Display(Name ="Employee", Description ="Employee asking for leave")]
        public EmployeePresentationDefaultViewModel AllocationEmployee { get; set; }

        [Display(Name = "Date of apply", Description = "Date when employee asked for leave", ShortName = "Appl. Date")]
        public string AllocationEmployeeId { get; set; }

        [Display(Name = "Leave type", Description = "Reason to leave")]
        public LeaveTypeNavigationViewModel AllocationLeaveType { get; set; }

        public int AllocationLeaveTypeId { get; set; }

        [Display(Name ="Period of leave", Description = "Year of leave")]
        public int Period { get; set; }

    }


    public class LeaveAllocationEditionViewModel {
        [HiddenInput(DisplayValue = false)]
        public long Id { get; set; }

        [Required]
        [Display(Name = "Duration of leave", Description ="Number of days of this vacation", ShortName ="# Days", Prompt ="")]
        public uint NumberOfDays { get; set; }

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

    public class LeaveAllocationLeaveTypesListViewModel {
        public LeaveAllocationLeaveTypesListViewModel() {
            AvalableLeaveTypes = new List<LeaveTypeNavigationViewModel>();
        }

        public List<LeaveTypeNavigationViewModel> AvalableLeaveTypes { get; set; }
    }
}
