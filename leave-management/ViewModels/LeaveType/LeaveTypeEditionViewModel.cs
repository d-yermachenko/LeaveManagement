using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveType {
    public class LeaveTypeEditionViewModel {
        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Leave Type - name required")]
        [DataType(DataType.Text, ErrorMessage = "Expected value: text")]
        [Display(Name = "LeaveTypeName", Description = "LeaveTypeName_Description", Prompt = "Insert leave type name, please")]
        public string LeaveTypeName { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
