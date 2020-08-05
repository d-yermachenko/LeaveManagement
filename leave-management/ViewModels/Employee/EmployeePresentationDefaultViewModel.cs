using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.Employee {
    public class EmployeePresentationDefaultViewModel {

        [HiddenInput()]
        public string Id { get; set; }

        [Display(Name ="User name", Description ="", ShortName ="")]
        public string UserName { get; set; }

        [Display(Name = "Title", Description ="Mr/Ms/Mrs")]
        public string Title { get; set; }

        [Display(Name = "First name", Description = "Users' first name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name", Description = "Last (family) name")]
        public string LastName { get; set; }

        [Display(Name = "Phone number", Description = "Phone")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Tax of employee")]
        public string TaxRate { get; set; }

        [Display(Name = "Date of birth of employee")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Date of employement")]
        public DateTime AncienityDate { get; set; }
    }
}
