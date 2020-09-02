using LeaveManagement.ViewModels.Employee;
using LeaveManagement.ViewModels.LeaveType;
using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.ViewModels.LeaveRequest {
    public class LeaveRequestDefaultViewModel {

        [Display(Name ="Id", ShortName ="#")]
        public long Id { get; set; }

        [Display(Name = "Requesting employee", ShortName = "Allocator")]
        public EmployeePresentationDefaultViewModel RequestingEmployee { get; set; }
        
        public string RequestingEmployeeId { get; set; }

        [Display(Name = "Leave type", ShortName = "LT")]
        public LeaveTypeNavigationViewModel LeaveType { get; set; }
        
        public int LeaveTypeId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of beginning", ShortName = "Begins")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of ending", ShortName = "Ends")]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Requested at", ShortName = "Req")]
        public DateTime RequestedDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Actionned at", ShortName = "Act")]
        public DateTime ActionedDateTime { get; set; }

        [Display(Name = "Approuved", ShortName = "Apr")]
        public bool? Approuved { get; set; }

        [Display(Name = "Moderator", ShortName = "Mod")]
        public EmployeePresentationDefaultViewModel ApprouvedBy { get; set; }

        [Display(Name = "Moderator", ShortName = "Mod")]
        public string ApprouvedByName { get; set; }

        [Display(Name = "Comment", Description = "Comment of employee who posted request")]
        [DataType(DataType.Text)]
        public string RequestComment { get; set; }

        [Display(Name = "Comment", Description = "Comment of employee who validated request")]
        [DataType(DataType.Text)]
        public string ValidationComment { get; set; }

        [Display(Name = "Cancelled", Description = "Flag means that request was cancelled")]
        public bool RequestCancelled { get; set; } = false;

    }
}
