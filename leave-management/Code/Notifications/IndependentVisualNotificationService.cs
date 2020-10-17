using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public class IndependentVisualNotificationService : IVisualNotificationService {

        public IndependentVisualNotificationService() {
            ;
        }

        public async Task<NotificationsModel> LoadNotificationsAsync(ClaimsPrincipal currentUser) {
            return await Task.FromResult(new NotificationsModel() { TotalEvents = 2 });
        }
    }
}
