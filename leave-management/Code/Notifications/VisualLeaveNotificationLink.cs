using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public class VisualLeaveNotificationLink {
        public string Controller { get; set; }

        public string Action { get; set; }

        public dynamic ActionArgs { get; set; }

        public string Message { get; set; }

        public int EventsCount { get; set; }

    }
}
