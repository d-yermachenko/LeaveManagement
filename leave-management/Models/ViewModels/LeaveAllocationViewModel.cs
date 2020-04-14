using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagement.Models.ViewModels {
    public class LeaveAllocationPresentationViewModel {
        public long Id { get; set; }

        public uint NumberOfDays { get; set; }

        public DateTime DateCreated { get; set; }

        public EmployeePresentationDefaultViewModel AllocationEmployee { get; set; }

        public string AllocationEmployeeId { get; set; }

        public LeaveTypeNavigationViewModel AllocationLeaveType { get; set; }

        public int AllocationLeaveTypeId { get; set; }

    }


    public class LeaveAllocationEditionViewModel {
        public long Id { get; set; }

        [Required]
        public uint NumberOfDays { get; set; }

        public DateTime DateCreated { get; set; }

        public EmployeePresentationDefaultViewModel AllocationEmployee { get; set; }

        public string AllocationEmployeeId { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; }

        public LeaveTypeNavigationViewModel AllocationLeaveType { get; set; }

        public int AllocationLeaveTypeId { get; set; }

        public IEnumerable<SelectListItem> AlocationLeaveTypes { get; set; }

    }
}
