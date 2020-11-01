using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public class VisualLeaveNotification {
        public DateTime EventDate { get; set; }

        public string EventMessage { get; set; }

        public string EventController => EventArgs.controller;

        public string EventAction => EventArgs.action;

        public dynamic EventArgs { get; set; }


        public static VisualLeaveNotification CreateRequestAdded(LeaveRequest request, IStringLocalizer localizer) {
            VisualLeaveNotification notification = new VisualLeaveNotification() {
                EventArgs = new { requestId = request.Id, Controller = "LeaveRequests", Action = "Review" },
                EventDate = request.RequestedDate
            };
            notification.EventMessage = localizer["You have new pending request for {0} from {1}", request.LeaveType.LeaveTypeName, request.RequestingEmployee.FormatEmployeeSNT()];
            return notification;

        }

        public static VisualLeaveNotification CreateRequestHandled(LeaveRequest request, IStringLocalizer localizer) {
            VisualLeaveNotification notification = new VisualLeaveNotification() {
                EventArgs = new { Controller = "LeaveRequests", Action = "EmployeeRequests" },
                EventDate = request.RequestedDate
            };
            string status = String.Empty;
            if (request.RequestCancelled) {
                status = localizer["Cancelled"];
            }
            else {
                if (request.Approuved == true)
                    status = localizer["Approuved"];
                else if (request.Approuved == false)
                    status = localizer["Rejected"];
            }
            notification.EventMessage = localizer["Your request for {0} was {1}", request.LeaveType.LeaveTypeName, status];
            return notification;
        }

        public static implicit operator Dictionary<string, string>(VisualLeaveNotification ths) {
            return new RouteValueDictionary(ths.EventArgs).ToDictionary(ks => ks.Key, vs => vs.Value.ToString());
        }
    }
}
