using LeaveManagement.Contracts;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Code {
    public class AutorisationsManager : IAutorisationsManager {
        public const string AdministratorRole = "Administrator";
        public const string EmployeeRole = "Employee";
        public const string HRStaffRole = "HRStaff";


        private IdentityUser _User;

        private IdentityUser User {
            get {
                if (_User == null) {
                    var currentUser = _SignInManager.Context.User;
                    _User = _UserManager.FindByIdAsync(_UserManager.GetUserId(currentUser)).Result;
                }
                return _User;
            }
        }


        private readonly UserManager<IdentityUser> _UserManager;


        private readonly SignInManager<IdentityUser> _SignInManager;

        private readonly IEmployeeRepositoryAsync _EmployeeRepository;

        public AutorisationsManager(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmployeeRepositoryAsync employeeRepository) {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _EmployeeRepository = employeeRepository;
        }


        public bool CanBrowseLeaveHistory(Employee employee) {
            if (User == null)
                return false;
            bool isOwn = User.Id.Equals(employee?.Id);
            if (isOwn)
                return true;
            Task <IList<string>> roles = null;
            if (employee == null) {
                roles = _UserManager.GetRolesAsync(User);
            }
            else 
                roles = _UserManager.GetRolesAsync(employee);
            if (roles == null)
                return false;
            return roles.Result.Any(x => new string[] { AdministratorRole, HRStaffRole }.Contains(x));
        }


        #region Leave type
        public bool CanBrowseLeaveTypes() {
            if (User == null)
                return false;
            Task<IList<string>> roles = _UserManager.GetRolesAsync(User);
            if (roles == null)
                return false;
            return roles.Result.Any(x => new string[] { AdministratorRole, HRStaffRole }.Contains(x));
        }

        public bool CanEditLeaveType(LeaveType leaveType = null) {
            if (User == null)
                return false;
            Task<IList<string>> roles = _UserManager.GetRolesAsync(User);
            if (roles == null)
                return false;
            return roles.Result.Any(x => new string[] { AdministratorRole }.Contains(x));
        }

        public bool CanCreateLeaveType(LeaveType leaveType = null) {
            if (User == null)
                return false;
            Task<IList<string>> roles = _UserManager.GetRolesAsync(User);
            if (roles == null)
                return false;
            return roles.Result.Any(x => new string[] { AdministratorRole, HRStaffRole }.Contains(x));
        }

        public bool CanDeleteLeaveType(LeaveType leaveType = null) {
            if (User == null)
                return false;
            Task<IList<string>> roles = _UserManager.GetRolesAsync(User);
            if (roles == null)
                return false;
            return roles.Result.Any(x => new string[] { AdministratorRole }.Contains(x));
        }
        #endregion

        public bool CanChangeContactInfo(Employee employee = null) {
            if (User == null)
                return false;
            bool isOwn = User.Id.Equals(employee?.Id);
            if (isOwn)
                return true;
            Task<IList<string>> roles = null;
            if (employee == null) {
                roles = _UserManager.GetRolesAsync(User);
            }
            else
                roles = _UserManager.GetRolesAsync(employee);
            if (roles == null)
                return false;
            return roles.Result.Any(x => new string[] { AdministratorRole, HRStaffRole }.Contains(x));
        }

        public bool CanChangePrincipalInfo(Employee employee = null) {
            throw new NotImplementedException();
        }

        public bool CanChangeSecondaryInfo(Employee employee = null) {
            throw new NotImplementedException();
        }


        public bool CanEditLeave(Employee employee) {
            throw new NotImplementedException();
        }

        public bool CanEditLeaveExceptValidation(LeaveType leaveType = null) {
            throw new NotImplementedException();
        }

        public bool CanPostLeave() {
            throw new NotImplementedException();
        }

        public bool CanRemoveLeave(Employee employee) {
            throw new NotImplementedException();
        }

        public bool CanRemoveUser(Employee employee = null) {
            throw new NotImplementedException();
        }

        public bool CanSelectUsers() {
            throw new NotImplementedException();
        }

        public bool CanValidateLeave(Employee employee) {
            throw new NotImplementedException();
        }

        public bool CanViewProfile(Employee employee = null) {
            throw new NotImplementedException();
        }

        public Employee GetCurrentEmployee() {
            return _EmployeeRepository.FindByIdAsync(User.Id).Result;
        }

        public async Task<Employee> GetCurrentEmployeeAsync() {
            return await _EmployeeRepository.FindByIdAsync(User.Id);
        }


    }
}
