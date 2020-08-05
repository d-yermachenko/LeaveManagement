﻿using AutoMapper;
using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Employee;
using LeaveManagement.ViewModels.LeaveAllocation;
using LeaveManagement.ViewModels.LeaveHistory;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Mappings {
    public class LeaveManagementMappings : Profile {
        public LeaveManagementMappings() {
            CreateMap<LeaveType, LeaveTypeEditionViewModel>().ReverseMap();
            CreateMap<LeaveType, LeaveTypeNavigationViewModel>().ReverseMap();

            CreateMap<Employee, EmployeePresentationDefaultViewModel>();
            CreateMap<Employee, Areas.Identity.Pages.Account.Manage.IndexModel.InputModel>().ReverseMap();
            CreateMap<IdentityUser, Areas.Identity.Pages.Account.Manage.IndexModel.InputModel>().ReverseMap();

            CreateMap<LeaveAllocation, LeaveAllocationEditionViewModel>().ReverseMap();
            CreateMap<LeaveAllocation, LeaveAllocationPresentationViewModel>().ReverseMap();
            

            CreateMap<LeaveHistory, LeaveHistoryDefaultViewModel>().ReverseMap();
            CreateMap<LeaveHistory, LeaveHistoryPresentationViewModel>().ReverseMap();
        }

    }
}
