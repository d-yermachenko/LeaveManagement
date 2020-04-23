using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveType {
    public class LeaveTypeNavigationViewModel {

        [Display(Name = "LeaveTypeId")]
        public int Id { get; set; }

        [Display(Name = "LeaveTypeName", Description = "LeaveTypeName_Description")]
        public string LeaveTypeName { get; set; }

        [Display(Name = "LeaveTypeDateCreated", Description = "LeaveTypeDateCreated_Description")]
        public DateTime DateCreated { get; set; }

    }
}
