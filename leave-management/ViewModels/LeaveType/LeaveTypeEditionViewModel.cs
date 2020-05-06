using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveType {
    public class LeaveTypeEditionViewModel {
        [Key]
        [Display(Name ="Id of leave type", Description ="")]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Leave Type - name required")]
        [DataType(DataType.Text, ErrorMessage = "Expected value: text")]
        [Display(Name = "Name of the type of leave", Description = "What you will see in the history of absences", Prompt = "Insert leave type name, please")]
        public string LeaveTypeName { get; set; }

        [Display(Name = "Date of creation", Description = "Date when this leave type was created")]
        public DateTime DateCreated { get; set; }

    }
}
