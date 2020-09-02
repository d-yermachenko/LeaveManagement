using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Areas.Identity.Pages.Account {
    [AllowAnonymous]
    public class RegisterModel : PageModel {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender) {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public EmployeeInputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class EmployeeInputModel {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Pseudo")]
            public string DisplayName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            [DataType(DataType.Text, ErrorMessage = "Enter employee title")]
            [Display(Name = "Employee title", Description = "M., Ms., Mrs., Miss", Prompt = "Employee title")]
            public string Title { get; set; }


            [DataType(DataType.Text, ErrorMessage = "Enter employee name")]
            [Display(Name = "Employee first name", Prompt = "First name")]
            public string FirstName { get; set; }


            [DataType(DataType.Text, ErrorMessage = "Enter employee last(family) name")]
            [Display(Name = "Employee last(family) name", Prompt = "Last(family) name")]
            public string LastName { get; set; }



            [DataType(DataType.Date, ErrorMessage = "Enter employee's date of birth")]
            [Display(Name = "Employees' date of birth", Prompt = "Employees' date of birth")]
            public DateTime DateOfBirth { get; set; }


            [DataType(DataType.Date, ErrorMessage = "Enter employement date")]
            [Display(Name = "Employement date", Prompt = "Enter employement date", Description = "Date when this person joined the company")]
            public DateTime EmployementDate { get; set; }


        }

        public async Task OnGetAsync(string returnUrl = null) {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid) {
                var user = new Employee {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Title = Input.Title,
                    DateOfBirth = Input.DateOfBirth,
                    EmploymentDate = Input.EmployementDate,
                    DisplayName = String.IsNullOrWhiteSpace(Input.DisplayName) ? $"{Input.FirstName} {Input.FirstName}" : Input.DisplayName
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded) {
                    
                    _logger.LogInformation("User created a new account with password.");
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, UserRoles.Employee.ToString());
                    if(addToRoleResult.Succeeded)
                        _logger.LogInformation("Added to role " + UserRoles.Employee.ToString());
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
