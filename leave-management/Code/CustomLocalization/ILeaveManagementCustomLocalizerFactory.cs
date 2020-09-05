using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;


namespace LeaveManagement.Code.CustomLocalization {
    public interface ILeaveManagementCustomLocalizerFactory : IStringLocalizerFactory{

        IStringLocalizer CommandsLocalizer { get; }

        IStringLocalizer MenuLocalizer { get;  }

        IHtmlLocalizer HtmlIdentityLocalizer { get;  }

        IStringLocalizer StringIdentityLocalizer { get;  }

    }
}
