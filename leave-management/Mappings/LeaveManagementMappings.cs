using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LeaveManagement.Data.Entities;
using LeaveManagement.Models.ViewModels;

namespace LeaveManagement.Mappings {
    public class LeaveManagementMappings : Profile {
        public LeaveManagementMappings() {
            CreateMap<LeaveType, LeaveTypeDetailed>();
            CreateMap<LeaveType, LeaveTypeCreation>().ReverseMap();
            CreateMap<LeaveType, LeaveTypeDefaultViewModel>();

            CreateMap<Employee, EmployeePresentationDefaultViewModel>();

            CreateMap<LeaveAllocation, LeaveAllocationEditionViewModel>();
            CreateMap<LeaveAllocation, LeaveAllocationPresentationViewModel>();

            CreateMap<LeaveHistory, LeaveHistoryDefaultViewModel>();
            CreateMap<LeaveHistory, LeaveHistoryPresentationViewModel>();
        }

    }
}
