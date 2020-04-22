using LeaveManagement.ViewModels.LeaveType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Models.ViewModels {
    public class LeaveHistoryDefaultViewModel {

        public long Id { get; set; }
        public EmployeePresentationDefaultViewModel RequestingEmployee { get; set; }
        public string RequestingEmployeeId { get; set; }
        public LeaveTypeNavigationViewModel LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDateDate { get; set; }
        public DateTime RequestedDateDate { get; set; }
        public DateTime ActionedDateTime { get; set; }
        public bool? Approuved { get; set; }
        public EmployeePresentationDefaultViewModel ApprouvedBy { get; set; }
        public string ArrpouvedById { get; set; }

    }

    public class LeaveHistoryPresentationViewModel {
        public long Id { get; set; }

        public string RequestingEmployee {
            get =>
                RequestingEmployeeDatails?.Title ?? "?" + " "
                + RequestingEmployeeDatails?.FirstName ?? "??" + " "
                + RequestingEmployeeDatails?.LastName ?? "????";
        }

        public EmployeePresentationDefaultViewModel RequestingEmployeeDatails { get; set; }
        public string LeaveType { get => LeaveTypeDetails.LeaveTypeName; }

        public LeaveTypeNavigationViewModel LeaveTypeDetails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDateDate { get; set; }
        public DateTime RequestedDateDate { get; set; }
        public DateTime ActionedDateTime { get; set; }
        public bool? Approuved { get; set; }

        public string ApprouvedBy {
            get =>
                ApprouvedByDetails?.Title ?? "?" + " "
                + ApprouvedByDetails?.FirstName ?? "??" + " "
                + ApprouvedByDetails?.LastName ?? "????";
        }

        public EmployeePresentationDefaultViewModel ApprouvedByDetails { get; set; }
        public string ArrpouvedById { get; set; }
    }
}
