using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Controllers;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public class NotificationsModel {

        public NotificationsModel() {
            LeaveNotifications = new List<LeaveNotification>();
            LeaveNotificationsLinks = new List<LeaveNotificationLink>();
        }



        public class LeaveNotification {
            public DateTime EventDate { get; set; }

            public string EventMessage { get; set; }

            public string EventController => EventArgs.controller;

            public string EventAction => EventArgs.action;

            public dynamic EventArgs { get; set; }


            public static LeaveNotification CreateRequestAdded(LeaveRequest request, IStringLocalizer localizer) {
                LeaveNotification notification = new LeaveNotification() {
                    EventArgs = new { requestId = request.Id, Controller = "LeaveRequests", Action = "Review" },
                    EventDate = request.RequestedDate
                };
                notification.EventMessage = localizer["You have new pending request for {0} from {1}", request.LeaveType.LeaveTypeName, request.RequestingEmployee.FormatEmployeeSNT()];
                return notification;

            }

            public static LeaveNotification CreateRequestHandled(LeaveRequest request, IStringLocalizer localizer) {
                LeaveNotification notification = new LeaveNotification() {
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

            public static implicit operator Dictionary<string, string>(LeaveNotification ths) {
                return new RouteValueDictionary(ths.EventArgs).ToDictionary(ks => ks.Key, vs => vs.Value.ToString());
            }
        }

        public class LeaveNotificationLink {
            public string Controller { get; set; }

            public string Action { get; set; }

            public dynamic ActionArgs { get; set; }

            public string Message { get; set; }

            public int EventsCount { get; set; }

        }

        [BindProperty]
        public List<LeaveNotification> LeaveNotifications {
            get;
            private set;
        }

        [BindProperty]
        public List<LeaveNotificationLink> LeaveNotificationsLinks {
            get;
            private set;
        }

        [BindProperty]
        public int TotalEvents { get; set; } = 0;

        public int? PendingRequests { get; set; } = null;

       
    }
}
