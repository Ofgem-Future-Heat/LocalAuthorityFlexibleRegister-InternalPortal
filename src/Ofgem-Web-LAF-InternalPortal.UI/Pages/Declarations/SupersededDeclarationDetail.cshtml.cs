using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeRoles(
        UserRoles.Advanced,
        UserRoles.Admin,
        UserRoles.Expert,
        UserRoles.Standard,
        UserRoles.Basic
    )]
    public class SupersededDeclarationDetailModel : PageModel
    {
        private readonly ILogger<SupersededDeclarationDetailModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty] public Guid DeclarationId { get; set; }

        [BindProperty] public Guid ActiveDeclarationId { get; set; }

        [BindProperty] public Guid DeclarationNoteId { get; set; }

        [BindProperty(SupportsGet = true)] public Models.Declaration? Declaration { get; set; }

        [BindProperty(SupportsGet = true)] public Models.DeclarationErrorFields? DeclarationErrorFields { get; set; }

        [BindProperty] public string SubmissionNote { get; set; }

        [BindProperty] public string DisplayMessage { get; set; }

        [BindProperty] public string? Version { get; set; }
        
        [BindProperty] public Extensions.Permissions.SupersededDeclarationDetail Permissions { get; set; }

        public SupersededDeclarationDetailModel(
            ILogger<SupersededDeclarationDetailModel> logger,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            DeclarationId = Guid.Empty;
            DeclarationNoteId = Guid.Empty;
            SubmissionNote = string.Empty;

            DisplayMessage = string.Empty;

            Permissions = new Extensions.Permissions.SupersededDeclarationDetail();
        }

        public async Task OnGetWithUrnAndVersion(Guid parentDeclarationId, string urn, string version)

        {
            _logger.LogInformation("SupersededDeclarationDetailModel - OnGetWithUrnAndVersion");

            Permissions = new Extensions.Permissions.SupersededDeclarationDetail(_httpContextAccessor, _httpClientFactory, TempData);

            ActiveDeclarationId = parentDeclarationId;
            Version = version;

            await RefreshData(urn, version);
        }

        private async Task RefreshData(string urn, string version)
        {
            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{Services.DeclarationApi.RouteSupersededDeclarationDetail}/{urn}/{version}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Models.Declaration>();

                    if (result != null)
                    {
                        Declaration = result;

                        Declaration.DateOfHouseholderEligibility = Declaration.DateOfHouseholderEligibilityForView;
                        Declaration.DateOfStatementOfIntentPublication = Declaration.DateOfStatementOfIntentPublicationForView;

                        DeclarationErrorFields = new Models.DeclarationErrorFields(Declaration.DeclarationErrors);

                        await GetSupersededDeclarationNotes(Declaration.DeclarationId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError("SupersededDeclarationDetail - RefreshData, Error: {Message}", ex.Message);
                DisplayMessage = "An issue occurred retrieving data";
            }
        }


        private async Task GetSupersededDeclarationNotes(Guid declarationId)
        {
            _logger.LogInformation("SupersededDeclarationDetail - GetSupersededDeclarationNotes");

            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{Services.DeclarationApi.RouteGetDeclarationNote}/{declarationId}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Models.DeclarationNote>>();

                    if (result != null)
                    {
                        var declarationNotesViewModel = new List<Models.DeclarationNote>();

                        foreach (var item in result)
                        {
                            declarationNotesViewModel.Add(new Models.DeclarationNote()
                            {
                                DeclarationNoteId = item.DeclarationNoteId,
                                DeclarationId = item.DeclarationId,
                                CreatedDate = item.CreatedDate,
                                Text = item.Text,
                                CreatedByName = item.CreatedByName
                            });
                        }
                        Declaration!.DeclarationNotes = declarationNotesViewModel;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError("SupersededDeclarationDetail - GetSupersededDeclarationDetail, Error: {Message}", ex.Message);
                DisplayMessage = "An issue occurred retrieving data";
            }
        }
    }
}