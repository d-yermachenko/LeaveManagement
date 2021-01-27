using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement {
    public class OperationFailedException : System.Exception {
        public string Operation { get; protected internal set; }

        private string GetOperation() {
            return new StackTrace().GetFrame(2).GetMethod().Name;
        }

        public OperationFailedException() : base() {
            Operation = GetOperation();
        }

        public OperationFailedException(string message): base(message) {
            Operation = GetOperation();
        }


    }
}
