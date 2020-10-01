using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Contracts;
using LeaveManagement.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Notifications {
    public class VisualNotificationService {

        private readonly UserManager<IdentityUser> _UserManager;
        private readonly SignInManager<IdentityUser> _SignInManager;
        private readonly ILeaveRequestsRepositoryAsync _LeaveRequestsRepository;
        private readonly IEmployeeRepositoryAsync _EmployeeRepository;
        private readonly IStringLocalizer _NotificationLocalizer;
        System.Security.Claims.ClaimsPrincipal _UserData;

        public VisualNotificationService(
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

        }
    }
}
