using LeaveManagement.Data.Entities;
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

        [Display(Name = "Default number of days", Description = "Number of days for this leave type for period")]
        public int DefaultDays { get; set; }

        [Display(Name = "Author of this leave type", Description = "Employee created this leave type")]
        public string AuthorLastName { get; set; }

    }
}
