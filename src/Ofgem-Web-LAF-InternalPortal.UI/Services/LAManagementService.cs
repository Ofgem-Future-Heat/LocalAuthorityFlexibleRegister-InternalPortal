using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_InternalPortal.Models;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Services;

public interface ILaManagementService
{
    Task<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>?> GetLocalAuthoritiesAsync();

    Task<DashboardView> GetPagedSoiListAsync(Ofgem.LAF.SharedLibrary.Models.SoiFilter soiListFilter);
    Task<(bool Success, string? ErrorMessage)> CreateSoiAsync(Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest? request);
    Task<Ofgem.LAF.SharedLibrary.Models.LocalAuthority?> GetByOnsCodeAsync(string onsCode);
    Task<Models.StatementOfIntentV2?> GetSoiById(Guid statementOfIntentId);

    Task<List<Models.LocalAuthority>> GetSoiLaAsync(Ofgem.LAF.SharedLibrary.Models.ProfilesFilter profilesFilter);
}


public class LaManagementService(HttpClient httpClient,
    ILogger<LaManagementService> logger)
    : ILaManagementService
{
    public async Task<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>?> GetLocalAuthoritiesAsync()
    {
        try
        {
            List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>? result = [];

            var httpResponseMessage = await httpClient.GetAsync(LocalAuthorityApi.Route);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>();
            }

            return result ?? [];
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities, $"An issue occurred retrieving local authority data, {ex.Message}");
            return [];
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateSoiAsync(Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest? request)
    {
        try
        {
            var httpResponseMessage =
                await httpClient.PostAsJsonAsync(LocalAuthorityApi.RouteCreateSoi, request);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem is { Detail: not null })
            {
                return (false, problem.Detail);
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities, $"An issue occurred retrieving local authority data, {ex.Message}");

        }

        return (false, null);
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.LocalAuthority?> GetByOnsCodeAsync(string onsCode)
    {
        try
        {
            var httpResponseMessage =
                await httpClient.GetAsync(LocalAuthorityApi.RouteGetByOnsCode + onsCode);

            if (httpResponseMessage.IsSuccessStatusCode)
            {

                var la = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                return la;
            }

            return null;

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities,
                $"An issue occurred retrieving local authority data, {ex.Message}");
        }

        return null;
    }

    public async Task<DashboardView> GetPagedSoiListAsync(Ofgem.LAF.SharedLibrary.Models.SoiFilter soiListFilter)
    {
        try
        {
            var returnModel = new DashboardView();

            var httpResponseMessage = await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteSoiList, soiListFilter);

            if (!httpResponseMessage.IsSuccessStatusCode) return returnModel;

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>>();

            if (result == null) return returnModel;

            foreach (var item in result.Results)
            {
                returnModel.StatementOfIntents.Add(Models.StatementOfIntentV2.MapFromDtoStatementOfIntent(item));
            }

            returnModel.CurrentPage = result.CurrentPage;
            returnModel.FirstRowOnPage = result.FirstRowOnPage;
            returnModel.LastRowOnPage = result.LastRowOnPage;
            returnModel.PageCount = result.PageCount;
            returnModel.PageSize = result.PageSize;
            returnModel.RowCount = result.RowCount;

            return returnModel;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetSoi);
            throw;
        }
    }

    public async Task<Models.StatementOfIntentV2?> GetSoiById(Guid statementOfIntentId)
    {
        try
        {
            var returnModel = new Models.StatementOfIntentV2();

            var httpResponseMessage =
                await httpClient.GetAsync(LocalAuthorityApi.RouteSoiById + statementOfIntentId);

            if (!httpResponseMessage.IsSuccessStatusCode) return null;

            var result = await httpResponseMessage.Content
                .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>();

            if (result is null) return returnModel;

            returnModel= StatementOfIntentV2.MapFromDtoStatementOfIntent(result);

            return returnModel;

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities,
                $"An issue occurred retrieving statement of intent data, {ex.Message}");
        }

        return null;
    }

    public async Task<List<Models.LocalAuthority>> GetSoiLaAsync(Ofgem.LAF.SharedLibrary.Models.ProfilesFilter profilesFilter)
    {
        try
        {
            profilesFilter.IncludeSoi = false;

            List<Models.LocalAuthority> LocalAuthorities = [];

            var httpResponseMessage =
                        await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteGetFiltered, profilesFilter);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>();

                if (result != null)
                {

                    foreach (var item in result.Results)
                    {
                        var localAuthority = new Models.LocalAuthority
                        {
                            Email = item.Email,
                            LocalAuthorityId = item.LocalAuthorityId,
                            Name = item.Name,
                            OnsCode = item.OnsCode,
                            Status = item.StatementOfIntents!.MaxBy(x => x.PublishedDate)?.Status 
                                     ?? Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment
                        };

                        LocalAuthorities.Add(localAuthority);
                    }
                }
            }

            return LocalAuthorities;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetSoi);
            throw;
        }

    }


}