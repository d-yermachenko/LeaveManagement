using LeaveManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveManagementTests.Units.Code {
    [TestClass]
    public class UserRolesStringConvertion {

        [TestMethod]
        public void TestUserRolesToString() {
            UserRoles roles = UserRoles.AppAdministrator | UserRoles.HRManager;
            string expected = "HRManager, Administrator";
            Assert.AreEqual(expected, roles.ToString());
        }

        [TestMethod]
        public void TestStringToUserRoles() {
            UserRoles expected = UserRoles.AppAdministrator | UserRoles.HRManager;
            string etringValues = "HRManager,Administrator";
            UserRoles obtained = (UserRoles)Enum.Parse(typeof(UserRoles), etringValues);
            Assert.AreEqual(expected, obtained);
        }

        [TestMethod]
        public void TestStringArrayToUserRolesWrongValuesDefault() {
            UserRoles expected = UserRoles.AppAdministrator | UserRoles.HRManager;
            string[] stringValues = new string[] { "HRManager", "Administrator", "CustomRole" };
            Assert.ThrowsException<ArgumentException>(() => {
                UserRoles obtained = (UserRoles)Enum.Parse(typeof(UserRoles), stringValues.Aggregate((one, two) => one + ", " + two));
                Assert.AreEqual(expected, obtained);
            });

    }

        [TestMethod]
        public void TestStringArrayToUserRolesWrongValuesCustom() {
            UserRoles expected = UserRoles.AppAdministrator | UserRoles.HRManager;
            string[] stringValues = new string[] { "HRManager", "Administrator", "CustomRole" };
            UserRoles obtained = LeaveManagementExtensions.ToUserRoles(stringValues);
            Assert.AreEqual(expected, obtained);
        }

        [TestMethod]
        public void TestRolesToRemove() {
            UserRoles oldRoles = UserRoles.AppAdministrator | UserRoles.HRManager;
            UserRoles newRoles = UserRoles.AppAdministrator | UserRoles.Employee;
            UserRoles removedRolesActual = (oldRoles ^ newRoles) & oldRoles;
            UserRoles removedRolesExpected = UserRoles.HRManager;
            Assert.AreEqual(removedRolesExpected, removedRolesActual);
        }

        [TestMethod]
        public void TestRolesToAdd() {
            UserRoles oldRoles = UserRoles.AppAdministrator | UserRoles.HRManager;
            UserRoles newRoles = UserRoles.AppAdministrator | UserRoles.Employee;
            UserRoles addedRolesActual = (oldRoles ^ newRoles) & newRoles;
            UserRoles addedRolesExpected = UserRoles.Employee;
            Assert.AreEqual(addedRolesExpected, addedRolesActual);
        }

        #region Convertion between roles and strings
        public static string[] FromUserRoles(UserRoles userRoles) {
            return userRoles.ToString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static UserRoles ToUserRoles(IEnumerable<string> rolesNames) {
            StringBuilder userRolesBuilder = new StringBuilder();
            userRolesBuilder.AppendJoin(',', rolesNames);
            if (Enum.TryParse<UserRoles>(userRolesBuilder.ToString(), out UserRoles result))
                return result;
            else
                throw new ArgumentException("List of userRoles contains invalid values");
        }
        #endregion


    }
}
