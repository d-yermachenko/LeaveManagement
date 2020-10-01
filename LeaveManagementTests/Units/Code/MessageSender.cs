using LeaveManagement.EmailSender;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagementTests.Units.Code {
    [TestClass]
    public class MessageSender {
        [TestMethod]
        public void SendSimpleMail() {
            IEmailSender emailSender = new SmtpEmailSender(() => {
                SmtpSettings settings = new SmtpSettings();
                settings.SmtpMailServer = "smtp.gmail.com";
                settings.SmtpPort = 465;
                settings.SenderLogin = "dmi.yermachenko@gmail.com";
                settings.SenderDisplayName = "dmi yermachenko";
                settings.SenderPassword = "Gxk~KE7A('@44)Q'";
                settings.SenderEmail = "dmi.yermachenko@gmail.com";
                return settings;
            }, null);
            emailSender.SendEmailAsync("d.yermachenko@gmail.com", "Your test was succees", "Reporting entry").Wait();
        }
    }
}
