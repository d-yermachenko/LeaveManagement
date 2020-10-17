using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public class VisualNotificationService : IVisualNotificationService , IDisposable {

        private readonly UserManager<IdentityUser> _UserManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly IStringLocalizer _NotificationLocalizer;

        public VisualNotificationService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILeaveManagementUnitOfWork unitOfWork,
            ILeaveManagementCustomLocalizerFactory stringLocalizerFactory
            ) 
        {
            _UnitOfWork = unitOfWork;
            _SignInManager = signInManager;
            _UserManager = userManager;
            _NotificationLocalizer = stringLocalizerFactory.Create(typeof(LeaveRequestsController));
        }

        public async Task<NotificationsModel> LoadNotificationsAsync(System.Security.Claims.ClaimsPrincipal currentUser) {
            if (!_SignInManager.IsSignedIn(currentUser))
                return new NotificationsModel() { TotalEvents = 0 };
            var result = new NotificationsModel();
            var currentUserId = _UserManager.GetUserId(currentUser);
            var employee = await _UnitOfWork.Employees.FindAsync(x => x.Id.Equals(currentUserId));
            List<NotificationsModel.LeaveNotification> leaveNotificationList = new List<NotificationsModel.LeaveNotification>();
            if (await _UserManager.IsCompanyPrivelegedUser(employee)) {
                var pendingRequestsNotifications = (
                    await _UnitOfWork.LeaveRequest.WhereAsync(q => q.RequestedDate.CompareTo(employee.LastConnectionDate) > 0 &&
                q.Approuved == null && !q.RequestCancelled))
                    .Select(x => NotificationsModel.LeaveNotification.CreateRequestAdded(x, _NotificationLocalizer))
                    .ToList();
                leaveNotificationList.AddRange(pendingRequestsNotifications);
                result.PendingRequests = pendingRequestsNotifications.Count();
                result.TotalEvents += result.PendingRequests ?? 0;
                if (result.PendingRequests > 0) {
                    result.LeaveNotificationsLinks.Add(new NotificationsModel.LeaveNotificationLink() {
                        Action = "ModeratorIndex",
                        Controller = "LeaveRequests",
                        Message = _NotificationLocalizer["Review requests"],
                        EventsCount = pendingRequestsNotifications.Count()
                    });
                }

            }

            var requestConcernedCurrentEmployee = (await _UnitOfWork.LeaveRequest.WhereAsync(q => q.RequestingEmployeeId != null && q.RequestingEmployeeId.Equals(employee.Id)));

            var actionedRequestsNotifications = new List<NotificationsModel.LeaveNotification>();
            leaveNotificationList.AddRange(actionedRequestsNotifications);
            if (actionedRequestsNotifications.Count() > 0) {
                result.LeaveNotificationsLinks.Add(new NotificationsModel.LeaveNotificationLink() {
                    Action = "EmployeeRequests",
                    Controller = "LeaveRequests",
                    Message = _NotificationLocalizer["Show your leave requests"],
                    EventsCount = actionedRequestsNotifications.Count()
                });
                result.TotalEvents += actionedRequestsNotifications.Count();
            }
            //EmployeeRequests
            var leaveNotifications = leaveNotificationList.OrderBy(x => x.EventDate).Take(4).ToList();
            return result;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Disposing
        bool _Disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (_Disposed)
                return;
            if (disposing) {
                _UserManager.Dispose();
                _UnitOfWork.Dispose();
            }

            _Disposed = true;

        }

        #endregion
    }
}
