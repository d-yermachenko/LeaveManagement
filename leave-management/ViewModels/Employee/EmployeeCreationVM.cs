using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace LeaveManagement.ViewModels.Employee {

    public class EmployeeCreationVM  {

        [HiddenInput]
        public string Id { get; set; }

        [HiddenInput]
        public string ReturnUrl { get; set; }

        [Required]
        [Display(Name = "Email", Prompt ="Your email", Description = "Your email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /*[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Pseudo")]
        public string DisplayName { get; set; }*/

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Your password", Description ="Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Prompt = "Confirm your password", Description = "Confirm your password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "User name", Description = "The main user name which will be displayed like your login", ShortName = "Login", Prompt ="Enter user name")]
        [DataType(DataType.Text)]
        [MinLength(6)]
        [MaxLength(50)]
        [Required (ErrorMessage = "User name is required")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Employee title", Description = "M., Ms., Mrs., Miss", Prompt = "Employee title")]
        public string Title { get; set; }

        [Required(ErrorMessage ="First name required")]
        [Display(Name = "First name", Description = "Users' first name", Prompt = "Enter first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name required")]
        [Display(Name = "Last name", Description = "Last (family) name", Prompt = "Enter last(family) name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number required")]
        [Display(Name = "Phone number", Description = "Phone", Prompt ="Enter phone number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Tax of employee", Prompt = "Enter tax rate", Description = "Tax rate")]
        public string TaxRate { get; set; }

        [Required(ErrorMessage ="Date of birth cant be empty")]
        [Display(Name = "Date of birth", Prompt ="Date of birth of employee", Description = "Date of birth of employee")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Date of employement", Prompt = "Employement date", Description = "Date of employement")]
        [DataType(DataType.Date)]
        public DateTime EmploymentDate { get; set; } = DateTime.Now;

        [Display(Name = "Roles", Prompt = "Roles", Description ="Function of this employee")]
        public IEnumerable<SelectListItem> RolesList { get; set; }

        public IList<string> Roles { get; set; }

        [Required(ErrorMessage ="You must accept contract")]
        [Display(Name = "Agree to all conditions", Prompt = "Accept th conditions", Description = "Accept the conditions of thez contract")]
        public bool AcceptContract { get; set; }
    }
}
