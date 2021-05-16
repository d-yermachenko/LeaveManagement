using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.EmailSender {
    public class SmtpSettings {
        public string SmtpMailServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderDisplayName { get; set; }
        public string SenderLogin { get; set; }
        public string SenderPassword { get; set; }
        public string SenderEmail { get; set; }
    }
}
