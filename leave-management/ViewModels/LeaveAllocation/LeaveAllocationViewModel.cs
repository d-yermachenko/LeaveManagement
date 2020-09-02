using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.ViewModels.LeaveType;
using System.ComponentModel;
using LeaveManagement.ViewModels.Employee;

namespace LeaveManagement.ViewModels.LeaveAllocation {
    public class LeaveAllocationPresentationViewModel {

        [Display(Name ="ID", Description ="Identity number of this allocation", ShortName ="#")]
        public long Id { get; set; }

        [Display(Name = "Duration of leave", Description = "Duration of leave", ShortName = "Duration")]
        public uint NumberOfDays { get; set; }

        [DataType(DataType.Date)]
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
}
