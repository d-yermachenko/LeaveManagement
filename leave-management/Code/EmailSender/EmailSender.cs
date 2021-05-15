using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.EmailSender {
    public class SmtpEmailSender : IEmailSender {
        public Task SendEmailAsync(string email, string subject, string htmlMessage) {
            return SendEmail(email, subject, htmlMessage);
        }

        private readonly SmtpSettings _Settings;
        private readonly ILogger<IEmailSender> _Logger;

        public SmtpEmailSender(Func<SmtpSettings> configure,
            ILogger<IEmailSender> logger) {
            _Settings = configure.Invoke();
            _Logger = logger;
        }

        public async Task SendEmail(string email, string subject, string htmlMessage) {
            try {
                using MailKit.IMailTransport mailTransport = await CreateTransport();
                InternetAddressList fromList = new InternetAddressList();
                fromList.Add(new MailboxAddress(_Settings.SenderDisplayName, _Settings.SenderEmail));
                InternetAddressList toList = new InternetAddressList();
                toList.AddRange(email.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(ma => new MailboxAddress(ma, ma)));
                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlMessage;
                bodyBuilder.TextBody = htmlMessage;
                MimeMessage mimeMessage = new MimeMessage(fromList, toList, subject, bodyBuilder.ToMessageBody());
                await mailTransport.SendAsync(mimeMessage);
            }
            catch(AggregateException ae) {
                _Logger.LogError($"Aggregate exception when sending email. Email host {_Settings.SmtpMailServer}:{_Settings.SmtpPort}", ae.Flatten());
                throw;
            }
            catch(AuthenticationException authe) {
                _Logger.LogError($"Authentification failed.\n {authe.Message}\n Email host {_Settings.SmtpMailServer}:{_Settings.SmtpPort}", authe);
                throw;
            }
            catch (Exception e) {
                _Logger.LogError($"Exception of type '{e.GetType().Name}' when sending email. Email host {_Settings.SmtpMailServer}:{_Settings.SmtpPort}", e);
                throw;
            }
        }

        private async Task<MailKit.IMailTransport> CreateTransport() {
            SecureSocketOptions secureSocketOptions = SecureSocketOptions.SslOnConnect;
            MailKit.Net.Smtp.SmtpClient smtpClient = new MailKit.Net.Smtp.SmtpClient();
            smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            try {
                await smtpClient.ConnectAsync(_Settings.SmtpMailServer, _Settings.SmtpPort, secureSocketOptions);
                var authMech = smtpClient.AuthenticationMechanisms;
                if (smtpClient.IsConnected)
                    await smtpClient.AuthenticateAsync(_Settings.SenderLogin, _Settings.SenderPassword);
                if (!smtpClient.IsAuthenticated)
                    return smtpClient;
            }
            catch(AggregateException e) {
                _Logger?.LogError(e, e.Message);
                throw;
            }
            return smtpClient;
        }
    }
}
