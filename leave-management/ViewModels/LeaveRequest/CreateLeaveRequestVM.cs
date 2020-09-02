using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.LeaveRequest {
    public class CreateLeaveRequestVM {
        [Display(Name ="Start date", Description = "Date of leaving", ShortName ="Begins")]
        [Required(ErrorMessage ="Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End date", Description = "Date of return", ShortName = "Ends")]
        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Comment", Description = "Comment of employee who posted request")]
        [DataType(DataType.Text)]
        public string RequestComment { get; set; }

        [Required(ErrorMessage = "Leave Type is required")]
        [Display(Prompt = "Please select leaveType")]
        public int? LeaveTypeId { get; set; }

        [Display(Name = "Leave type", ShortName = "Reason", Prompt ="Please select leaveType")]
        public IEnumerable<SelectListItem> LeaveTypes { get; set; }
    }
}
