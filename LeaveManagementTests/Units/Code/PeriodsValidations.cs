using LeaveManagement.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LeaveManagementTests.Units.Code {
    [TestClass]
    public class PeriodsValidations {

        public Func<DateTime, DateTime, int[], bool> GetValidationMethod() {
            var methodInfo = typeof(LeaveRequestsController).GetMethod("ValidateDays", BindingFlags.NonPublic | BindingFlags.Static);
            Func<DateTime, DateTime, int[], bool> result = (startDate, endDate, periods) => {
                return (bool)methodInfo?.Invoke(null, new object[] { startDate, endDate, periods });
            };
            return result;
        }

        [TestMethod]
        public void TestLegitimeRequesInOneYear() {
            bool result = GetValidationMethod().Invoke(new DateTime(2012, 5, 3), new DateTime(2012, 5, 14), new int[] { 20 });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestIlegitimeRequesInOneYear() {
            bool result = GetValidationMethod().Invoke(new DateTime(2012, 5, 3), new DateTime(2012, 5, 14), new int[] { 8 });
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestLegitimeRequesBetweenYears() {
            bool result = GetValidationMethod().Invoke(new DateTime(2012, 12, 27), new DateTime(2013,1, 6), new int[] { 5, 8 });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestIlegitimeRequesBetweenYears() {
            bool result = GetValidationMethod().Invoke(new DateTime(2012, 12, 27), new DateTime(2013, 1, 6), new int[] {3, 8 });
            Assert.IsFalse(result);
        }
    }


}
