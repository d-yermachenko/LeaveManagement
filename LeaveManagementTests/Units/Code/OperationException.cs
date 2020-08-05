using LeaveManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagementTests.Units {
    [TestClass]
    public class OperationException {
        public void ThrowOperationException() {
            throw new OperationFailedException();
        }

        public void ThrowOperationExceptionFromCallingMethod() {
            ThrowOperationException();
        }

        [TestMethod]
        public void TestOperationExceptionOperationName() {
            bool assert = true;
            try {
                ThrowOperationException();
            }
            catch(OperationFailedException oe1) {
                assert &= oe1.Operation.Equals(nameof(ThrowOperationException));
            }
            try {
                ThrowOperationExceptionFromCallingMethod();
            }
            catch(OperationFailedException oe2) {
                assert &= oe2.Operation.Equals(nameof(ThrowOperationException));
            }
            Assert.IsTrue(assert);
        }

    }
}
