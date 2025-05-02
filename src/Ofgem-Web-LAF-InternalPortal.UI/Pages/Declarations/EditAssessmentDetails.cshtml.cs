using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert
    )]
    public class EditAssessmentDetailsModel(
        ILogger<EditAssessmentDetailsModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty] public Extensions.Permissions.EditAssessmentDetails Permissions { get; set; } = new();

        [TempData]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty(SupportsGet = true)]
        public Models.Declaration Declaration { get; set; } = new();

        [BindProperty]
        public DeclarationStatus Status { get; set; }

        [BindProperty]
        public string DisplayMessage { get; set; } = string.Empty;


        public async Task OnGetWithId(Guid declarationId)
        {
            DeclarationId = declarationId;

            Permissions = new Extensions.Permissions.EditAssessmentDetails(httpContextAccessor, httpClientFactory, TempData);

            await RefreshData(declarationId);
        }

        public async Task<IActionResult> OnPostSaveAndContinue()
        {
            logger.LogInformation("EditAssessmentDetailsModel - OnPostSaveAndContinue");

            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            Permissions = new Extensions.Permissions.EditAssessmentDetails(httpContextAccessor, httpClientFactory, TempData);

            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            Declaration.Status = Status;
            Declaration.DeclarationId = DeclarationId;

            var httpResponseMessage = await httpClient.PutAsJsonAsync(Services.DeclarationApi.Route, Declaration);

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new DataException("EditAssessmentDetailsModel - OnPostSaveAndContinue : Failed");

            var result = await httpResponseMessage.Content
                .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.Declaration>();

            return RedirectToPage(
                LafPages.DeclarationDetail.ROUTE,
                LafPages.DeclarationDetail.METHOD_GET_WITH_ID_ACTION,
                new
                {
                    declarationId = result!.DeclarationId
                });

        }

        private async Task RefreshData(Guid declarationId)
        {
            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{Services.DeclarationApi.Route}/{declarationId}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Models.Declaration>();

                    if (result != null)
                    {
                        Declaration = result;
                        Status = Declaration.Status;
                        DeclarationId = Declaration.DeclarationId;
                        TempData.Keep();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "EditAssessmentDetail - RefreshData, Error");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }
    }
}
