using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Models.ViewModels {
    public class LeaveTypeNavigationViewModel {
        public int Id { get; set; }

        public string LeaveTypeName { get; set; }

        public DateTime DateCreated { get; set; }

    }
    public class LeaveTypeCreation {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LeaveTypeName { get; set; }

    }

    public class LeaveTypeNotFoundViewModel {
        public LeaveTypeNotFoundViewModel() {
            ;
        }

        public LeaveTypeNotFoundViewModel(object id, string objectClass) {
            Id = id;
            ObjectClass = objectClass;
        }

        public object Id { get; set; }

        public string ObjectClass { get; set; }

        public string ObjectName { get; set; }

        public string Message {
            get {
                return (string.IsNullOrWhiteSpace(ObjectName) ? $"with id {Id}" : $"named {ObjectName}");
            }
        }


    }
}
