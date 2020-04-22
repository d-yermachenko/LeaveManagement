using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveType {
    public class LeaveTypeCreation {
        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Leave Type - name required")]
        [DataType(DataType.Text, ErrorMessage = "Expected value: text")]
        public string LeaveTypeName { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
