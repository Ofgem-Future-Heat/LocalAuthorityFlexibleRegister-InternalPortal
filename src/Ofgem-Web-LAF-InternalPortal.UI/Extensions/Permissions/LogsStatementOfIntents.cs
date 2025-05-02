using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem_Web_LAF_InternalPortal.Extensions.Permissions
{
    public class LogsStatementOfIntents : PermissionBase
    {
        public bool CanDownLoadLogs { get; }

        public LogsStatementOfIntents() { }

        public LogsStatementOfIntents(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            ITempDataDictionary tempData)
            : base(httpContextAccessor, httpClientFactory, tempData)
        {
            switch (Role)
            {
                case UserRoles.Basic:
                    CanDownLoadLogs = false;
                    break;
                case UserRoles.Admin:
                case UserRoles.Standard:
                case UserRoles.Advanced:
                case UserRoles.Expert:
                    CanDownLoadLogs = true;
                    break;

                default:
                    CanDownLoadLogs = false;
                    break;
            }
        }
    }
}
