using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public interface IVisualNotificationService {
        Task<NotificationsModel> LoadNotificationsAsync(System.Security.Claims.ClaimsPrincipal currentUser);
    }
}
