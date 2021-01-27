using System;

namespace LeaveManagement.Models {
    public class ErrorViewModel {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorTitle { get; set; }

        public string ErrorMessage { get; set; }

        public bool ErrorDescribed => !String.IsNullOrWhiteSpace(ErrorTitle) && !String.IsNullOrWhiteSpace(ErrorMessage);

        public string StartUrl { get; set; }

        public string StartText { get; set; }
    }
}
