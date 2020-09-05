using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagementTests.Units.Code {
    [TestClass]
    public class PasswordGenerator {

        [TestMethod]
        public void TestConfiguration() {
            int expectedLength = 15;

            var passwordGenerator = new LeaveManagement.PasswordGenerator.PasswordGenerator(() => new LeaveManagement.PasswordGenerator.PasswordGeneratorOptions() { LengthOfPassword = expectedLength });

            string password = passwordGenerator.GeneratePassword();
            Assert.AreEqual(expectedLength, password.Length);
        }

    }
}
