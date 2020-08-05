using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveManagement.Areas.Identity.Pages.Account.Manage {
    public partial class IndexModel : PageModel {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmployeeRepositoryAsync _employeeRepository;

        private bool IsPrivelegedUser = false;
        private bool IsCurrentUser = false;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmployeeRepositoryAsync employeeRepository) {
            _userManager = userManager;
            _signInManager = signInManager;
            _employeeRepository = employeeRepository;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel {

            [HiddenInput(DisplayValue = false)]
            public string Id { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }


            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Pseudo")]
            public string DisplayName { get; set; }


            [Required]
            [DataType(DataType.Text, ErrorMessage = "Enter employee title")]
            [Display(Name = "Employee title", Description = "M., Ms., Mrs., Miss", Prompt = "Employee title")]
            public string Title { get; set; }

            [Required]
            [DataType(DataType.Text, ErrorMessage = "Enter employee name")]
            [Display(Name = "Employee first name", Prompt = "First name")]
            public string FirstName { get; set; }

            [Required]
            [DataType(DataType.Text, ErrorMessage = "Enter employee last(family) name")]
            [Display(Name = "Employee last(family) name", Prompt = "Last(family) name")]
            public string LastName { get; set; }


            [Required]
            [DataType(DataType.Date, ErrorMessage = "Enter employee's date of birth")]
            [Display(Name = "Employees' date of birth", Prompt = "Employees' date of birth")]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [DataType(DataType.Date, ErrorMessage = "Enter employement date")]
            [Display(Name = "Employement date", Prompt = "Enter employement date", Description = "Date when this person joined the company")]
            public DateTime EmployementDate { get; set; }

            [DataType(DataType.Text, ErrorMessage = "Enter the tax rate")]
            [Display(Name = "Tax rate", Prompt = "Enter the tax rate", Description = "Tax percentage")]
            public string TaxRate { get; set; }

            public bool CanChangeBirthDate { get; set; }

            public bool CanChangeDateOfEmployement { get; set; }
        }

        private async Task LoadAsync(IdentityUser user) {
            var employee = await _employeeRepository.FindByIdAsync(user.Id);
            Username = employee.UserName;
            Input = new InputModel {
                Id = employee.Id,
                PhoneNumber = employee.PhoneNumber,
                DateOfBirth = employee.DateOfBirth,
                DisplayName = employee.DisplayName,
                Title = employee.Title,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                EmployementDate = employee.EmploymentDate,
                TaxRate = employee.TaxRate,
                CanChangeBirthDate = IsCurrentUser || IsPrivelegedUser,
                CanChangeDateOfEmployement = IsPrivelegedUser
            };
        }

        public async Task<IdentityUser> LoadUserAndCheckPermissions(string userId = "") {
            IdentityUser concernedUser = null;
            IdentityUser currentUser = await _userManager.GetUserAsync(User);
            IsPrivelegedUser = await _userManager.IsUserHasOneRoleOfAsync(currentUser, SeedData.AdministratorRole, SeedData.EmployeeRole);
            if (string.IsNullOrWhiteSpace(userId)) {
                concernedUser = currentUser;
            }
            else
                concernedUser = await _userManager.FindByIdAsync(userId);

            IsCurrentUser = currentUser?.Id.Equals(concernedUser?.Id) ?? false;

            if (!IsCurrentUser || !IsPrivelegedUser)
                Forbid();
            return concernedUser;
        }

        public async Task<IActionResult> OnGetAsync(string userId = "") {
            var user = await LoadUserAndCheckPermissions(userId);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync() {
            var user = await LoadUserAndCheckPermissions(Input.Id);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) {
                await LoadAsync(user);
                return Page();
            }

            var employee = await _employeeRepository.FindByIdAsync(user.Id);
            #region Update values from Input  - mapping can be usefull here
            employee.Title = Input.Title;
            employee.DisplayName = Input.DisplayName;
            employee.FirstName = Input.FirstName;
            employee.LastName = Input.LastName;
            employee.PhoneNumber = Input.PhoneNumber;
            employee.TaxRate = Input.TaxRate;
            if (!employee.DateOfBirth.Equals(Input.DateOfBirth) && (IsCurrentUser || IsPrivelegedUser))
                employee.DateOfBirth = Input.DateOfBirth;
            if (!employee.EmploymentDate.Equals(Input.EmployementDate) && IsPrivelegedUser)
                employee.EmploymentDate = Input.EmployementDate;
            #endregion
            var setPhoneResult = await _employeeRepository.UpdateAsync(employee);
            if (!setPhoneResult) {
                var userId = await _userManager.GetUserIdAsync(employee);
                throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
