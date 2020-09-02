using AutoMapper;
using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Employee;
using LeaveManagement.ViewModels.LeaveAllocation;
using LeaveManagement.ViewModels.LeaveRequest;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagement.Mappings {
    public class LeaveManagementMappings : Profile {
        public LeaveManagementMappings() {
            CreateMap<LeaveType, LeaveTypeEditionViewModel>().ReverseMap();
            CreateMap<LeaveType, LeaveTypeNavigationViewModel>().ReverseMap();
            CreateMap<LeaveType, SelectListItem>()
                .ForMember(txt => txt.Text, act => act.MapFrom(src => src.LeaveTypeName))
                .ForMember(val => val.Value, act => act.MapFrom(src => src.Id))
                .ForAllOtherMembers(act => act.Ignore());


            CreateMap<Employee, EmployeePresentationDefaultViewModel>();
            CreateMap<Employee, IdentityUser>().ReverseMap();
            CreateMap<Employee, Areas.Identity.Pages.Account.Manage.IndexModel.InputModel>().ReverseMap();
            CreateMap<IdentityUser, Areas.Identity.Pages.Account.Manage.IndexModel.InputModel>().ReverseMap();
            CreateMap<Employee, EmployeeCreationVM>().ReverseMap();

            CreateMap<LeaveAllocation, LeaveAllocationEditionViewModel>().ReverseMap();
            CreateMap<LeaveAllocation, LeaveAllocationPresentationViewModel>().ReverseMap();
            

            CreateMap<LeaveRequest, LeaveRequestDefaultViewModel>().ReverseMap();
        }

    }
}
