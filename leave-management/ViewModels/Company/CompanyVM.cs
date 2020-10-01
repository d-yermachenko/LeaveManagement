using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.ViewModels.Company {
    public class CompanyVM {

        [HiddenInput]
        public int Id { get; set; }

        [Display(Name = "Company name", Description = "Company name", Prompt = "Please enter company name", GroupName ="Principal")]
        [Required(ErrorMessage ="Company name is required field")]
        [DataType(DataType.Text)]
        public string CompanyName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Registration date", Description = "Date when company was registered on our site", GroupName = "CompanyData")]
        public DateTime CompanyRegistrationDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Creation date", Description = "Date when company was created", Prompt = "Please enter date", GroupName = "CompanyData")]
        public DateTime CompanyCreationDate { get; set; }

        [Display(Name = "Company fiscal number", Description = "Company fiscal number in the country of its registration", 
            Prompt = "Please enter fiscal number", GroupName = "CompanyData")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Fiscal data is required")]
        public string TaxId { get; set; }


        [DataType(DataType.Text)]
        [Display(Name = "Registration country", Description = "Country in which the company has its headquarter",
            Prompt = "Please enter registration country", GroupName = "Contact")]
        public string CompanyState { get; set; }


        [DataType(DataType.PostalCode)]
        [Required(ErrorMessage = "Company zip code is required")]
        [Display(Name ="Zip code", Description ="Postal code", Prompt = "Please enter zip code", GroupName = "Contact")]
        public string CompanyZipCode { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage ="Company email is required")]
        [Display(Name = "Company email", Description = "Contact email", Prompt = "Please enter company email", GroupName = "Contact")]
        public string CompanyEmail { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Postal address is required")]
        [Display(Name = "Company postal address", Description = "Company postal address", Prompt = "Please enter postal address", GroupName = "Contact")]
        public string CompanyPostAddress { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name ="Public comment", Description ="Public comment visible by anyone of the company", Prompt ="Please enter public comment", GroupName ="Comments")]
        public string CompanyPublicComment { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Protected comment", Description = "Protected comment visible by App and company admibistrators", Prompt = "Please enter protected comment", GroupName = "Comments")]
        public string CompanyProtectedComment { get; set; }

        [Display(Name = "Company active", Description = "Is company active or suspended", GroupName = "CompanyData")]
        public bool Active { get; set; }

        [Display(Name = "Company enables lockout for its employees", Description = "Is company locked out", GroupName = "CompanyData")]
        public bool EnableLockoutForEmployees { get; set; }
    }
}
