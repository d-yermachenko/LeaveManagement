using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveType {
    public class LeaveTypeNavigationViewModel {

        [Display(Name = "LeaveTypeId")]
        public int Id { get; set; }

        [Display(Name = "LeaveTypeName")]
        public string LeaveTypeName { get; set; }

        [Display(Name = "LeaveTypeDateCreated")]
        public DateTime DateCreated { get; set; }

    }
}
