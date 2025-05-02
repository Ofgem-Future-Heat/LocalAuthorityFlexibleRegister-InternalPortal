using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_InternalPortal.Extensions;


namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Advanced,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Admin,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Expert,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Standard,
        Ofgem.LAF.SharedLibrary.Models.UserRoles.Basic
    )]
    public class DeclarationDetailEditSummaryModel : PageModel
    {
        private readonly ILogger<DeclarationDetailEditSummaryModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty] public Guid DeclarationId { get; set; }

        [BindProperty] public Guid DeclarationNoteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.Declaration? Declaration { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.Declaration? DeclarationOriginal { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.DeclarationErrorFields DeclarationErrorFields { get; set; }

        [BindProperty]
        public string DisplayMessage { get; set; }

        [BindProperty] public Extensions.Permissions.DeclarationDetailEditSummary Permissions { get; set; }

        public DeclarationDetailEditSummaryModel(
            ILogger<DeclarationDetailEditSummaryModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            DeclarationId = Guid.Empty;
            DeclarationNoteId = Guid.Empty;
            DisplayMessage = string.Empty;

            Permissions = new Extensions.Permissions.DeclarationDetailEditSummary();
            DeclarationErrorFields = new Models.DeclarationErrorFields();
        }

        public Task OnGetWithId()
        {
            _logger.LogInformation("DeclarationDetailEditSummaryModel - OnGetWithId");

            Permissions = new Extensions.Permissions.DeclarationDetailEditSummary(_httpContextAccessor, _httpClientFactory, TempData);

            Declaration = TempData.Get<Models.Declaration>(TempDataKeys.DeclarationEdit);
            DeclarationOriginal = TempData.Get<Models.Declaration>(TempDataKeys.DeclarationEditOriginal);

            TempData.Put(TempDataKeys.DeclarationEdit, Declaration);
            TempData.Put(TempDataKeys.DeclarationEditOriginal, DeclarationOriginal);

            if (Declaration is null || DeclarationOriginal is null)
            {
                Console.WriteLine(@"DeclarationDetailEditSummaryModel - OnGetWithId - declaration is null");

                _logger.LogError(null, "DeclarationDetailEditSummaryModel - OnGetWithId - declaration is null ");

                DisplayMessage = "An issue occurred retrieving data";
            }
            else
            {
                Declaration!.LocalAuthority = DeclarationOriginal.LocalAuthority;
            }

            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPost(Guid declarationId)
        {
            _logger.LogInformation("DeclarationDetailEditSummaryModel - OnPostCreateDeclarationNote");

            Permissions = new Extensions.Permissions.DeclarationDetailEditSummary(_httpContextAccessor, _httpClientFactory, TempData);

            if (Declaration is null)
            {
                Console.WriteLine($@"DeclarationDetailEditSummaryModel - OnPost - declaration is null id: {declarationId}", declarationId);
                _logger.LogError(null, "DeclarationDetailEditSummaryModel - OnPost - declaration is null id: {declarationId}", declarationId);
                DisplayMessage = "An issue occurred retrieving data";

                return Page();
            }

            try
            {
                var rawDeclaration = Models.Declaration.MapToRawDeclaration(Declaration);

                var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteSaveEditedDeclaration,
                        rawDeclaration);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.ValidationResponse>();

                    if (result != null)
                    {
                        return RedirectToPage(
                            LafPages.DeclarationDetailEditCompleted.ROUTE,
                            LafPages.DeclarationDetailEditCompleted.METHOD_GET_WITH_ID_AND_URN_ACTION,
                            new
                            {
                                declarationId = result.NewDeclarationId,
                                urn = Declaration.Urn
                            });
                    }
                }
                else
                {
                    Console.WriteLine($@"DeclarationDetailEditSummaryModel - OnPost - Failed: {httpResponseMessage.StatusCode}");
                    _logger.LogError(null, "DeclarationDetailEditSummaryModel - OnPost - Failed: {HttpStatusCode}", httpResponseMessage.StatusCode);
                    DisplayMessage = "An issue occurred updating the record.";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($@"DeclarationDetailEditSummaryModel - OnPost - Failed Declaration ID: {declarationId}");
                _logger.LogError(e, "DeclarationDetailEditSummaryModel - OnPost - Failed Declaration ID: {DeclarationId}", declarationId);
                DisplayMessage = "An issue occurred updating the record.";
            }

            return Page();
        }
    }
}
