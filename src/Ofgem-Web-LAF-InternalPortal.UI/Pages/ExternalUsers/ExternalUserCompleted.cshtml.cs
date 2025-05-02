using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.ExternalUsers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert
    )]
    public class ExternalUserCompletedModel(
        ILogger<ExternalUserCompletedModel> logger)
        : PageModel
    {
        [BindProperty(SupportsGet = true)] public Ofgem.LAF.SharedLibrary.Models.User ExternalUser { get; set; } = new();

        [BindProperty] public string EmailAddress { get; set; } = string.Empty;

        [BindProperty] public bool IsSuccessfulAdd { get; set; }
        [BindProperty] public bool IsSuccessfulEdit { get; set; }
        [BindProperty] public bool IsSuccessfulDeactivate { get; set; }
        [BindProperty] public bool IsSuccessfulReactivate { get; set; }


        public async Task OnGetSuccessAdd()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessAdd");
                IsSuccessfulAdd = true;
            });
        }
        public async Task OnGetSuccessEdit(string emailAddress)
        {
            EmailAddress = emailAddress;

            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessEdit");
                IsSuccessfulEdit = true;
            });
        }
        public async Task OnGetSuccessReactivate()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessReactivate");
                IsSuccessfulReactivate = true;
            });
        }
        public async Task OnGetSuccessDeactivation()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessDeactivation");
                IsSuccessfulDeactivate = true;
            });
        }

    }
}
