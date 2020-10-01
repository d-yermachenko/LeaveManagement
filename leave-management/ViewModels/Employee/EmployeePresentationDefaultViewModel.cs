using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Company;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.Employee {
    public class EmployeePresentationDefaultViewModel {

        [HiddenInput()]
        public string Id { get; set; }

        #region Identity data
        [Display(Name = "Title", Description = "Mr/Ms/Mrs", Prompt = "Title")]
        public string Title { get; set; }

        [Display(Name ="User name", Description ="", ShortName ="", Prompt = "User name")]
        public string UserName { get; set; }

        [Display(Name = "First name", Description = "Users' first name", Prompt = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name", Description = "Last (family) name", Prompt = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Date of birth", Description = "Date of birth of employee", Prompt = "Enter date of birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        #endregion

        #region Contact data

        [Display(Name = "Phone number", Description = "Phone", Prompt = "Please enter the phone number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email", Description ="Email", Prompt = "Email")]
        public string Email { get; set; }

        [Display(Name = "Contact mail", Prompt = "Please enter contact mail", Description = "Contact mail can be used as alternative mail")]
        [DataType(DataType.EmailAddress)]
        public string ContactMail { get; set; }
        #endregion

        #region Employement-related data
        [Display(Name = "Tax of employee", Description ="Tax rate", Prompt ="Enter tax of employee")]
        public string TaxRate { get; set; }

        [Display(Name = "Date of employement", Description = "Date of the beginning of the contract", Prompt ="Enter day of the beginning of the contract")]
        [DataType(DataType.Date)]
        [Required]
        [ReadOnly(true)]
        public DateTime EmploymentDate { get; set; }

        [Display(Name ="RH Manager", Description ="Manager who can validate his ", Prompt = "RH manager")]
        public EmployeePresentationDefaultViewModel Manager { get; set; }

        [Display(Name = "RH Manager", Description = "Manager who can validate his leave", Prompt = "RH manager")]
        public string ManagerId { get; set; }

        [Required(ErrorMessage = "Each employee must be attached to company")]
        [Display(Name = "Company", Prompt = "Please assign employee to the company", Description = "Employees company")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Each employee must be attached to company")]
        [Display(Name = "Company", Prompt = "Please assign employee to the company", Description = "Employees company")]
        public CompanyVM Company { get; set; }

        [Display(Name="User roles", Description ="Performed roles")]
        public UserRoles Roles { get; set; }

        public bool CanAllocateLeave = true;
        #endregion
    }
}
