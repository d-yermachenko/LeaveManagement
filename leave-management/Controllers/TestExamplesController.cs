using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.PasswordGenerator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Controllers {
    public class TestExamplesController : Controller {
        private readonly Contracts.ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly IStringLocalizer _StringLocalizer;
        private readonly IPasswordGenerator _PasswordGenerator;
        private readonly IEmailSender _EmailSender;

        public TestExamplesController(
            Contracts.ILeaveManagementUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            IPasswordGenerator passwordGenerator,
            IEmailSender emailSender
            ) {
            _UnitOfWork = unitOfWork;
            _UserManager = userManager;
            _StringLocalizer = localizerFactory.Create(this.GetType());
            _PasswordGenerator = passwordGenerator;
            _EmailSender = emailSender;
        }

    }
}
