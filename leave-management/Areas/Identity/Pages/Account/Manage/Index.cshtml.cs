using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private bool IsPrivelegedUser = false;
        private bool IsCurrentUser = false;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel {

            [HiddenInput(DisplayValue = false)]
            public string Id { get; set; }

            [ReadOnly(true)]
            [Display(Name ="User name", Description = "Login name")]
            public string UserName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private void Load(IdentityUser user) {
            Input = new InputModel {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
        }

        public async Task<IdentityUser> LoadUserAndCheckPermissions(string userId = "") {
            IdentityUser currentUser = await _userManager.GetUserAsync(User);
            IsPrivelegedUser = await _userManager.IsCompanyPrivelegedUser(currentUser);
            IdentityUser concernedUser;
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

            Load(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync() {
            var user = await LoadUserAndCheckPermissions(Input.Id);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (result.Succeeded) {
                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Your profile has been updated";
                
            }
            return RedirectToPage();
        }
    }
}
