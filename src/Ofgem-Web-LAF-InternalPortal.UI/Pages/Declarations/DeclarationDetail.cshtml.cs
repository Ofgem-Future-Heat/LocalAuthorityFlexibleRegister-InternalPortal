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
    public class DeclarationDetailModel : PageModel
    {
        private readonly ILogger<DeclarationDetailModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty] public Guid DeclarationId { get; set; }

        [BindProperty] public Guid DeclarationNoteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.Declaration? Declaration { get; set; }

        [BindProperty(SupportsGet = true)]
        public Models.DeclarationErrorFields DeclarationErrorFields { get; set; }

        [BindProperty]
        public string SubmissionNote { get; set; }

        [BindProperty]
        public string DisplayMessage { get; set; }

        [BindProperty]
        public string? SubmissionNoteError { get; set; }

        [BindProperty] public bool HasSubmissionNoteError => SubmissionNoteError?.Length > 0;

        [BindProperty] public Extensions.Permissions.DeclarationDetail Permissions { get; set; }


        public DeclarationDetailModel(
            ILogger<DeclarationDetailModel> logger,
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

            DeclarationErrorFields = new Models.DeclarationErrorFields();
            Permissions = new Extensions.Permissions.DeclarationDetail();
        }

        public async Task OnGetWithId(Guid declarationId)
        {
            _logger.LogInformation("DeclarationDetailModel - OnGetWithId");

            Permissions = new Extensions.Permissions.DeclarationDetail(_httpContextAccessor, _httpClientFactory, TempData);

            DeclarationId = declarationId;

            await RefreshData(declarationId);
        }

        public async Task OnPostCreateDeclarationNote(Guid declarationId)
        {
            _logger.LogInformation("DeclarationDetailModel - OnPostCreateDeclarationNote");

            Permissions = new Extensions.Permissions.DeclarationDetail(_httpContextAccessor, _httpClientFactory, TempData);

            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            if (!string.IsNullOrEmpty(SubmissionNote))
            {
                if (!TextValidation.IsValidCharacterCount(SubmissionNote))
                {
                    SubmissionNoteError = "Submission note must be 200 characters or less";

                    await RefreshData(declarationId);
                    return;
                }
                var declarationNote = new Models.DeclarationNote()
                {
                    DeclarationId = DeclarationId,
                    Text = SubmissionNote,
                };

                var httpResponseMessage =
                    await httpClient.PostAsJsonAsync(Services.DeclarationApi.RouteCreateDeclarationNote,
                        declarationNote);

                if (!httpResponseMessage.IsSuccessStatusCode)
                    throw new BadHttpRequestException("CreateDeclarationNote - OnPostCreateDeclarationNote : Failed");
            }

            await RefreshData(declarationId);

            SubmissionNote = string.Empty;

            ModelState.Clear();
        }

        public async Task OnGetDeleteDeclarationNote(Guid declarationNoteId, Guid declarationId)
        {
            _logger.LogInformation("DeclarationDetailModel - OnPostDeleteDeclarationNote");

            Permissions = new Extensions.Permissions.DeclarationDetail(_httpContextAccessor, _httpClientFactory, TempData);

            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            var httpResponseMessage =
                await httpClient.DeleteAsync($"{Services.DeclarationApi.RouteDeleteDeclarationNote}/{declarationNoteId}");

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new BadHttpRequestException("DeclarationDetailModel - OnPostDeleteDeclarationNote : Failed");

            await RefreshData(declarationId);
        }


        private async Task RefreshData(Guid declarationId)
        {
            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{Services.DeclarationApi.Route}/{declarationId}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<Models.Declaration>();

                    if (result != null)
                    {
                        Declaration = result;

                        Declaration.DateOfHouseholderEligibility = Declaration.DateOfHouseholderEligibilityForView;
                        Declaration.DateOfStatementOfIntentPublication = Declaration.DateOfStatementOfIntentPublicationForView;

                        DeclarationErrorFields = new Models.DeclarationErrorFields(Declaration.DeclarationErrors);

                        await GetDeclarationNote(declarationId);
                        if (Declaration.Urn != null) await GetSupersededList(Declaration.Urn);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError(ex, "Dashboard - RefreshData");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private async Task GetDeclarationNote(Guid declarationId)
        {
            _logger.LogInformation("DeclarationDetailModel - GetDeclarationNote");

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
                                CreatedDate = LafLocalTimezone.ToLocalTime(item.CreatedDate??DateTime.Today),
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
                _logger.LogError(ex, "DashboardDetail - GetDeclarationNote, Error");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private async Task GetSupersededList(string urn)
        {
            _logger.LogInformation("DeclarationDetailModel - GetDeclarationNote");

            var httpClient = _httpClientFactory.CreateClient(Services.DeclarationApi.ApiName);

            try
            {
                var httpResponseMessage = await httpClient.GetAsync($"{Services.DeclarationApi.RouteAllSupersededDeclarations}/{urn}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Models.SupersededDeclaration>>();

                    if (result != null)
                    {
                        var supersededDeclarationViewModel = new List<Models.SupersededDeclaration>();

                        foreach (var item in result)
                        {
                            supersededDeclarationViewModel.Add(new Models.SupersededDeclaration()
                            {
                                DeclarationId = item.DeclarationId,
                                Urn = item.Urn,
                                CreatedDate = item.CreatedDate,
                                Version = item.Version,
                            });
                        }

                        Declaration!.SupersededDeclarations = supersededDeclarationViewModel;
                    }
                }
                else
                {
                    _logger.LogError(message: "Service call: {Route}/{Urn} failed with: {Status}", Services.DeclarationApi.RouteAllSupersededDeclarations, urn, httpResponseMessage.IsSuccessStatusCode);
                    DisplayMessage = "An issue occurred retrieving superseded data";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logger.LogError(ex, "DashboardDetail - GetDeclarationNote, Error: {ErrorMessage}", ex.Message);
                DisplayMessage = "An issue occurred retrieving data";
            }
        }
    }
}
