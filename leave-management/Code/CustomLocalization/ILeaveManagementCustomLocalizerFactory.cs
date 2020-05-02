using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;


namespace LeaveManagement.Code.CustomLocalization {
    public interface ILeaveManagementCustomLocalizerFactory {
        IStringLocalizer CreateStringLocalizer(Type type);

        IStringLocalizer CreateStringLocalizer(string baseName, string location);

        IHtmlLocalizer CreateHtmlLocalizer(Type type);

        IHtmlLocalizer CreateHtmlLocalizer(string baseName, string location);

    }
}
