using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveManagementTests.Units.Code {
    [TestClass]
    public class MyPasswordGenerator {

        [TestMethod]
        public void TestSimpleMyPassword() {
            LeaveManagement.PasswordGenerator.MyPasswordGenerator myPasswordGenerator = new LeaveManagement.PasswordGenerator.MyPasswordGenerator(
                ()=>new LeaveManagement.PasswordGenerator.MyPasswordGeneratorOptions() { LengthOfPassword = 15, MinSpecial = 3, MinSpaces=0 });
            string password = myPasswordGenerator.GeneratePassword();
            System.Diagnostics.Trace.WriteLine(password);
            Assert.AreEqual(15, password.Length);
        }

        [TestMethod]
        public void TestShortMyPassword() {
            LeaveManagement.PasswordGenerator.MyPasswordGenerator myPasswordGenerator = new LeaveManagement.PasswordGenerator.MyPasswordGenerator(
                () => new LeaveManagement.PasswordGenerator.MyPasswordGeneratorOptions() { LengthOfPassword = 10, MinSpecial = 4, MinSpaces = 0 });
            string password = myPasswordGenerator.GeneratePassword();
            System.Diagnostics.Trace.WriteLine(password);
            Assert.AreEqual(10, password.Length);
        }
    }
}
