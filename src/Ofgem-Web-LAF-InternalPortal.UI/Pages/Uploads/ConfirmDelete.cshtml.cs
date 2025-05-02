using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;
using System.Text.Json;
using Upload = Ofgem.LAF.SharedLibrary.Models.Upload;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Uploads
{

    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Admin,
        UserRoles.Expert,
        UserRoles.Standard,
        UserRoles.Basic
        )]
    public class ConfirmDeleteModel(
        ILogger<ConfirmDeleteModel> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Upload? Upload { get; set; }

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        [BindProperty] public Extensions.Permissions.ConfirmDelete Permissions { get; set; } = new();


        public void OnGet()
        {
            logger.LogInformation("ConfirmDeleteModel - OnGet");

            Permissions = new Extensions.Permissions.ConfirmDelete(httpContextAccessor, httpClientFactory, TempData);
            Upload = JsonSerializer.Deserialize<Upload>(((string)TempData.Peek(TempDataKeys.UploadToDelete)!));
        }

        public async Task<IActionResult> OnGetDeleteUpload(int uploadId)
        {
            var httpClient = httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);
            try
            {
                _ = await httpClient.DeleteAsync($"{Services.DeclarationApi.RouteUploadDelete}/{uploadId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                logger.LogError(ex, "ConfirmDeleteModel - DeleteUpload");
                DisplayMessage = "An issue occurred deleting an upload.";
            }

            return RedirectToPage(LafPages.Dashboard.ROUTE);
        }

    }
}