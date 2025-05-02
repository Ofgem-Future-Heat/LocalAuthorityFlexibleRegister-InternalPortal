using Microsoft.AspNetCore.Mvc;
using System.Net;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_InternalPortal.Models;

namespace Ofgem_Web_LAF_InternalPortal.Services;
public interface IUserService
{
    Task<(Ofgem.LAF.SharedLibrary.Models.User? DasUser, bool Found, string ErrorMessage)> AuthorisedSignatoryExistsAsync(
        string homeBaseLocalAuthority,
        string selectedLa);

    Task<(bool Success, string ErrorMessage)> CreateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority);

    Task<(bool Success, string ErrorMessage)> UpdateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority);

    Task<Ofgem.LAF.SharedLibrary.Models.User?> GetExternalUserAsync(
        string userId);

    Task<List<AnnouncementView>> GetPublishedAnnouncementsAsync();
}

public class UserService(
    ILogger<UserService> logger,
    IHttpClientFactory httpClientFactory) : IUserService
{

    private const string GenericDataIssueMessage = "Error occurred when creating external user";


    public async Task<(Ofgem.LAF.SharedLibrary.Models.User? DasUser, bool Found, string ErrorMessage)> AuthorisedSignatoryExistsAsync(string homeBaseLocalAuthority, string selectedLa)
    {
        string message;

        // check that there is ONLY 1 Dedicated Authorised Signatory
        try
        {
            var targetAuthority = string.Empty;

            if (!string.IsNullOrEmpty(homeBaseLocalAuthority))
            {
                targetAuthority = homeBaseLocalAuthority;
            }

            if (!string.IsNullOrEmpty(selectedLa))
            {
                targetAuthority = selectedLa;
            }

            var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

            var httpResponseMessage =
                await httpClient.GetAsync(
                    $"{UserApi.RouteGetExternalUser}/{targetAuthority}/{UserApi.RouteExistingAuthorisedSignatory}");

            message = string.Empty;

            if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound) return (null, false, message);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.User?>();

                if (result is not null)
                {
                    return (result, true, message);
                }
            }
            else
            {
                var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

                if (problem?.Detail != null)
                {
                    message = problem.Detail;
                    return (null, true, message);
                }
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (null, true, message);
    }

    public async Task<(bool Success, string ErrorMessage)> CreateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority)
    {
        var message = string.Empty;

        try
        {
            externalUser.IsExternal = true;

            externalUser.ExternalUserLocalAuthorities = [];
            externalUser.ExternalUserLocalAuthorities
                .Add(new Ofgem.LAF.SharedLibrary.Models.ExternalUserLocalAuthority()
                {
                    OnsCode = baseLocalAuthority.OnsCode,
                    Name = baseLocalAuthority.Name,
                    IsBaseLocalAuthority = true
                });


            var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

            var httpResponseMessage = await httpClient.PostAsJsonAsync(UserApi.RouteAddExternalUser, externalUser);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, message);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem?.Detail != null)
            {
                message = problem.Detail;
                return (false, message);
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (false, message);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority)
    {
        var message = string.Empty;

        try
        {
            externalUser.ExternalUserLocalAuthorities = [];
            externalUser.ExternalUserLocalAuthorities
                .Add(new Ofgem.LAF.SharedLibrary.Models.ExternalUserLocalAuthority()
                {
                    OnsCode = baseLocalAuthority.OnsCode,
                    Name = baseLocalAuthority.Name,
                    IsBaseLocalAuthority = true
                });


            var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

            var httpResponseMessage = await httpClient.PutAsJsonAsync(UserApi.RouteEditExternalUser, externalUser);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, message);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem?.Detail != null)
            {
                message = problem.Detail;
                return (false, message);
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (false, message);
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.User?> GetExternalUserAsync(string userId)
    {
        var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"{UserApi.Route}/{userId}");

            if (!httpResponseMessage.IsSuccessStatusCode) return null;

            var result =
                httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.User?>();

            return result.Result ?? null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<AnnouncementView>> GetPublishedAnnouncementsAsync()
    {
        List<AnnouncementView> announcements = [];
        try
        {
            var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

            var httpResponseMessage =
                await httpClient.GetAsync(UserApi.RouteGetPublishedAnnouncements);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();

                logger.LogError("Getting Published Announcements Failed. {Message}", errorMessage);

                return announcements;
            }

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<Ofgem.LAF.SharedLibrary.Models.Announcement>>();

            if (result == null) return announcements;

            announcements = [];

            foreach (var item in result)
            {
                if (item is null) continue;

                announcements.Add(AnnouncementView.MapFromDtoDeclaration(item));
            }

            return announcements;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Getting Published Announcements Failed. {Message}", ex.Message);
        }

        return announcements;
    }
}