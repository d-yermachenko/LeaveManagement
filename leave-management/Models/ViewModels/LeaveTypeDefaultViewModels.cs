using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Models.ViewModels {
    public class LeaveTypeDefaultViewModel {
        public string LeaveTypeName { get; set; }

        public DateTime DateCreated { get; set; }

    }

    public class LeaveTypeDetailed {
        public int Id { get; set; }

        public string LeaveTypeName { get; set; }

        public DateTime DateCreated { get; set; }
    }

    public class LeaveTypeCreation {

        [Required]
        public string LeaveTypeName { get; set; }

    }
}
