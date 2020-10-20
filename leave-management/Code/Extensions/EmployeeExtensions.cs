using LeaveManagement.Data.Entities;
using LeaveManagement.ViewModels.Employee;

namespace LeaveManagement {
    public static class EmployeeExtensions {
        public static string FormatEmployeeSNT(this Employee employee) {
            return $"{employee.LastName.ToUpper()} {employee.FirstName}, {employee.Title}";
        }

        public static string FormatEmployeeSNT(this EmployeePresentationDefaultViewModel employee) {
            return $"{employee.LastName.ToUpper()} {employee.FirstName}, {employee.Title}";
        }
    }

    
}