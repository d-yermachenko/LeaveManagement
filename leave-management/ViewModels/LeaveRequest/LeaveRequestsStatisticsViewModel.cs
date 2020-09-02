using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.LeaveRequest {
    public class LeaveRequestsStatisticsViewModel {

        public bool Moderation { get; set; }

        [Display(Name ="Total number of leave requests", Description = "List of requests without any sorting")]
        public int TotalRequests { get; set; }

        [Display(Name = "Pending requests", Description = "Requests to handle by manager")]
        public int PendingRequests { get; set; }

        [Display(Name = "Accepted requests", Description = "Requests, accepted by manager. Employee can take his leave in indicated period")]
        public int AcceptedRequests { get; set; }

        [Display(Name = "Rejected requests requests", Description = "Requests, rejected by manager. Employee cant leave the company in indicated period")]
        public int RejectedRequests { get; set; }

        public IList<LeaveRequestDefaultViewModel> LeaveRequests { get; set; }
    }
}
