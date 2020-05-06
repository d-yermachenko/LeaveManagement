using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveType {
    public class LeaveTypeNavigationViewModel {

        [Display(Name = "Id of leave type")]
        public int Id { get; set; }

        [Display(Name = "Name of the type of leave", Description = "What you will see in the history of absences", Prompt = "Insert leave type name, please")]
        public string LeaveTypeName { get; set; }

        [Display(Name = "Date of creation", Description = "Date when this leave type was created")]
        public DateTime DateCreated { get; set; }

    }
}
