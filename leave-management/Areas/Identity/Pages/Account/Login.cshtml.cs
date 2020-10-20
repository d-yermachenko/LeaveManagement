using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using LeaveManagement.Repository.Entity;
using LeaveManagement.Contracts;
using Microsoft.Extensions.Localization;

namespace LeaveManagement.Areas.Identity.Pages.Account {
    [AllowAnonymous]
    public class LoginModel : PageModel {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly IStringLocalizerFactory _stringLocalizerFactory;
        private readonly IStringLocalizer _stringLocalizer;

        public LoginModel(SignInManager<IdentityUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager,
            ILeaveManagementUnitOfWork unitOfWork,
            IStringLocalizerFactory stringLocalizerFactory) {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _UnitOfWork = unitOfWork;
            _stringLocalizerFactory = stringLocalizerFactory;
            _stringLocalizer = stringLocalizerFactory.Create(typeof(LoginModel));
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel {
            [Required]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null) {
            if (!string.IsNullOrEmpty(ErrorMessage)) {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid) {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                Microsoft.AspNetCore.Identity.SignInResult result = null;
                try {
                    var user = await _userManager.FindByNameAsync(Input.Email);
                    if (user == null)
                        user = await _userManager.FindByEmailAsync(Input.Email);
                    result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                }
                catch (Exception e) {
                    _logger.LogError(e, e.Message);
                    ModelState.AddModelError(string.Empty, _stringLocalizer["Cant connect sign in manager. Please contact your system administrator"]);
                    return Page();
                }
                if (result.Succeeded) {
                    _logger.LogInformation("User logged in.");
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user == null)
                        return LocalRedirect(returnUrl);
                    var employee = (await _UnitOfWork.Employees.FindAsync(x=>x.Id.Equals(user.Id)));
                    if (employee != null) {
                        employee.LastConnectionDate = employee.CurrentConnectionDate;
                        employee.CurrentConnectionDate = DateTime.Now;
                        await _UnitOfWork.Employees.UpdateAsync(employee);
                        try {
                            await _UnitOfWork.Save();
                        }
                        catch(AggregateException ae) {
                            _logger.LogError(ae.Flatten(), ae.Message);
                        }
                        catch(Exception e) {
                            _logger.LogError(e, e.Message);
                        }

                    }
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor) {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut) {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
