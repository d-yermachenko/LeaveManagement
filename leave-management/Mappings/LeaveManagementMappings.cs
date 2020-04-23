using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Data.Entities;
using LeaveManagement.Models.ViewModels;
using LeaveManagement.ViewModels.LeaveType;

namespace LeaveManagement.Mappings {
    public class LeaveManagementMappings : Profile {
        public LeaveManagementMappings() {
            CreateMap<LeaveType, LeaveTypeEditionViewModel>().ReverseMap();
            CreateMap<LeaveType, LeaveTypeNavigationViewModel>();

            CreateMap<Employee, EmployeePresentationDefaultViewModel>();

            CreateMap<LeaveAllocation, LeaveAllocationEditionViewModel>();
            CreateMap<LeaveAllocation, LeaveAllocationPresentationViewModel>();

            CreateMap<LeaveHistory, LeaveHistoryDefaultViewModel>();
            CreateMap<LeaveHistory, LeaveHistoryPresentationViewModel>();
        }

    }
}
