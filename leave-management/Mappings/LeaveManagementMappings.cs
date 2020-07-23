using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Data.Entities;
using LeaveManagement.Models.ViewModels;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Mappings {
    public class LeaveManagementMappings : Profile {
        public LeaveManagementMappings() {
            CreateMap<LeaveType, LeaveTypeEditionViewModel>().ReverseMap();
            CreateMap<LeaveType, LeaveTypeNavigationViewModel>();

            CreateMap<Employee, EmployeePresentationDefaultViewModel>();
            CreateMap<Employee, Areas.Identity.Pages.Account.Manage.IndexModel.InputModel>().ReverseMap();
            CreateMap<IdentityUser, Areas.Identity.Pages.Account.Manage.IndexModel.InputModel>().ReverseMap();

            CreateMap<LeaveAllocation, LeaveAllocationEditionViewModel>();
            CreateMap<LeaveAllocation, LeaveAllocationPresentationViewModel>();

            CreateMap<LeaveHistory, LeaveHistoryDefaultViewModel>();
            CreateMap<LeaveHistory, LeaveHistoryPresentationViewModel>();
        }

    }
}
