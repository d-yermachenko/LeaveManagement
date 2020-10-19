using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Controllers;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Views.Shared {
    public class NotificationsModel  {

        private readonly UserManager<IdentityUser> _UserManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly ILeaveRequestsRepositoryAsync _LeaveRequestsRepository;
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;
        private readonly IStringLocalizer _NotificationLocalizer;
        System.Security.Claims.ClaimsPrincipal _UserData;

        public NotificationsModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILeaveRequestsRepositoryAsync leaveRequestsRepository,
            ILeaveManagementCustomLocalizerFactory stringLocalizerFactory,
            IEmployeeRepositoryAsync employeeRepository,
            System.Security.Claims.ClaimsPrincipal userData
            ) : base() {
            _EmployeeRepository = employeeRepository;
            _UserData = userData;
            _SignInManager = signInManager;
            _UserManager = userManager;
            _LeaveRequestsRepository = leaveRequestsRepository;
            _NotificationLocalizer = stringLocalizerFactory.Create(typeof(LeaveRequestsController));
            LeaveNotificationsLinks = new List<LeaveNotificationLink>();
            LeaveNotifications = new List<LeaveNotification>();
        }

        public class LeaveNotification {
            public DateTime EventDate { get; set; }

            public string EventMessage { get; set; }

            public string EventController => EventArgs.controller;

            public string EventAction => EventArgs.action;

            public dynamic EventArgs { get; set; }


            public static LeaveNotification CreateRequestAdded(LeaveRequest request, IStringLocalizer localizer) {
                LeaveNotification notification = new LeaveNotification() {
                    EventArgs = new { requestId = request.Id, Controller = "LeaveRequests", Action= "Review" },
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
                    if(request.Approuved == true)
                        status = localizer["Approuved"];
                    else if(request.Approuved == false)
                        status = localizer["Rejected"];
                }
                notification.EventMessage = localizer["Your request for {0} was {1}", request.LeaveType.LeaveTypeName, status];
                return notification;
            }

            public static implicit operator Dictionary<string, string>(LeaveNotification ths) {
                return new RouteValueDictionary(ths.EventArgs).ToDictionary(ks=>ks.Key, vs=>vs.Value.ToString());
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
        public int TotalEvents { get; private set; } = 0;

        public int? PendingRequests { get; private set; } = null; 

        public async Task<NotificationsModel> LoadNotificationsAsync() {
            if (!_SignInManager.IsSignedIn(_UserData))
                return this;
            var employee = await _EmployeeRepository.FindByIdAsync(_UserManager.GetUserId(_UserData));
            List<LeaveNotification> Data = new List<LeaveNotification>();
            if (await _UserManager.IsCompanyPrivelegedUser(employee)) {
                var pendingRequestsNotifications = (
                    await _LeaveRequestsRepository.WhereAsync(q => q.RequestedDate.CompareTo(employee.LastConnectionDate) > 0 &&
                q.Approuved == null && !q.RequestCancelled))
                    .Select(x=> LeaveNotification.CreateRequestAdded(x, _NotificationLocalizer))
                    .ToList();
                Data.AddRange(pendingRequestsNotifications);
                PendingRequests = pendingRequestsNotifications.Count();
                TotalEvents += PendingRequests??0;
                if (PendingRequests > 0) {
                    LeaveNotificationsLinks.Add(new LeaveNotificationLink() {
                        Action = "ModeratorIndex",
                        Controller = "LeaveRequests",
                        Message = _NotificationLocalizer["Review requests"],
                        EventsCount = pendingRequestsNotifications.Count()
                    });
                }

            }
            /*var actionedRequestsNotifications = (await _LeaveRequestsRepository.WhereAsync(q => q.RequestedDate.CompareTo(employee.LastConnectionDate) > 0 &&
                q.Approuved != null && !q.RequestCancelled && (q?.RequestingEmployeeId?.Equals(employee.Id)??false)))
                .Select(x=> LeaveNotification.CreateRequestHandled(x, _NotificationLocalizer))
                .ToList();*/
            var requestConcernedCurrentEmployee = (await _LeaveRequestsRepository.WhereAsync(q => q.RequestingEmployeeId?.Equals(employee.Id)??false));

            var actionedRequestsNotifications = new List<LeaveNotification>();
            Data.AddRange(actionedRequestsNotifications);
            if (actionedRequestsNotifications.Count() > 0) {
                LeaveNotificationsLinks.Add(new LeaveNotificationLink() {
                    Action = "EmployeeRequests",
                    Controller = "LeaveRequests",
                    Message = _NotificationLocalizer["Show your leave requests"],
                    EventsCount = actionedRequestsNotifications.Count()
                });
                TotalEvents += actionedRequestsNotifications.Count();
            }
            //EmployeeRequests
            LeaveNotifications = Data.OrderBy(x => x.EventDate).Take(4).ToList();
            return this;
        }
    }
}
