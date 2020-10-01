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

        public bool AccountDataEnabled { get; set; } = true;

        [Display(Name = "User name", Description = "The main user name which will be displayed like your login", ShortName = "Login", Prompt = "Enter user name")]
        [DataType(DataType.Text)]
        [MinLength(6)]
        [MaxLength(50)]
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Principal email", Prompt ="Your email", Description = "Your principal email, which you can ")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        public bool ProfileDataEnabled { get; set; } = true;

        [Required]
        [Display(Name = "Employee title", Description = "M., Ms., Mrs., Miss", Prompt = "Employee title")]
        public string Title { get; set; }

        [Required(ErrorMessage ="First name required")]
        [Display(Name = "First name", Description = "Users' first name", Prompt = "Enter first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name required")]
        [Display(Name = "Last name", Description = "Last (family) name", Prompt = "Enter last(family) name")]
        public string LastName { get; set; }

        [Display(Name = "Tax of employee", Prompt = "Enter tax rate", Description = "Tax rate")]
        public string TaxRate { get; set; }

        [Required(ErrorMessage = "Date of birth cant be empty")]
        [Display(Name = "Date of birth", Prompt = "Date of birth of employee", Description = "Date of birth of employee")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Date of employement", Prompt = "Employement date", Description = "Date of employement")]
        [DataType(DataType.Date)]
        public DateTime EmploymentDate { get; set; } = DateTime.Now;

        public bool ContactDataEnabled { get; set; } = true;

        [Required(ErrorMessage = "Phone number required")]
        [Display(Name = "Phone number", Description = "Phone", Prompt ="Enter phone number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Contact mail", Prompt = "Please enter contact mail", Description = "Contact mail can be used as alternative mail")]
        [DataType(DataType.EmailAddress)]
        public string ContactMail { get; set; }

        public bool AccessDataEnabled { get; set; } = true;

        [Display(Name = "Roles", Prompt = "Roles", Description ="Function of this employee")]
        public IEnumerable<SelectListItem> RolesList { get; set; }

        public IList<string> Roles { get; set; }

        [Required(ErrorMessage ="You must accept contract")]
        [Display(Name = "Agree to all conditions", Prompt = "Accept th conditions", Description = "Accept the conditions of thez contract")]
        public bool AcceptContract { get; set; }

        public virtual bool CompanyEnabled { get; set; } = false;

        [Required(ErrorMessage = "Each employee must be attached to company")]
        [Display(Name ="Company", Prompt="Please assign employee to the company", Description ="Employees company")]
        public int CompanyId { get; set; }

        public Data.Entities.Company Company { get; set; }

        [Display(Name = "Company", Prompt = "Please assign employee to the company", Description = "Employees company")]
        public IEnumerable<SelectListItem> Companies { get; set; }

        public bool ManagerEnabled { get; set; }

        [Display(Name = "Manager id", Prompt = "Please assign employee to the company", Description = "Employees' manager")]
        public string ManagerId { get; set; }

        [Display(Name = "Manager id", Prompt = "Please assign employee to the company", Description = "Employees' manager")]
        public EmployeeCreationVM Manager { get; set; }

        [Display(Name = "Manager", Prompt = "Please choose the manager", Description = "Employee manager")]
        public IEnumerable<SelectListItem> Managers { get; set; }
    }
}
