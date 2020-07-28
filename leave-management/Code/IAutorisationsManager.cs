using LeaveManagement.Data.Entities;


namespace LeaveManagement.Code {
    /// <summary>
    /// Checks autorisation to perform actions
    /// </summary>
    /// <remarks>
    /// Some methods includes User instance, because some action can be allowed only own data or for user of specific roles for other's data.
    /// If user is null, it means ownself.
    /// Some methods includes objects, to verify is action is allowed on these objects
    /// </remarks>
    public interface IAutorisationsManager {
        #region User-related Autorisation getters
        bool CanChangePrincipalInfo(Employee employee = null);

        bool CanChangeContactInfo(Employee employee = null);

        bool CanChangeSecondaryInfo(Employee employee = null);

        bool CanRemoveUser(Employee employee = null);

        bool CanSelectUsers();

        bool CanViewProfile(Employee employee = null);
        #endregion

        #region Leave Type-related Autorisation getters
        bool CanCreateLeaveType(LeaveType leaveType = null);

        bool CanEditLeaveType(LeaveType leaveType = null);

        bool CanDeleteLeaveType(LeaveType leaveType = null);

        bool CanBrowseLeaveTypes();
        #endregion
        #region Leave
        bool CanPostLeave();

        bool CanEditLeave(Employee employee);

        bool CanEditLeaveExceptValidation(LeaveType leaveType = null);

        bool CanValidateLeave(Employee employee);

        bool CanRemoveLeave(Employee employee);
        #endregion

        #region Leave history
        bool CanBrowseLeaveHistory(Employee employee);
        #endregion



    }
}
