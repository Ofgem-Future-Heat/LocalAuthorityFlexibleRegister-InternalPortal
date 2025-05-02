using Ofgem.LAF.SharedLibrary.Extensions;
using Upload = Ofgem.LAF.SharedLibrary.Models.Upload;

namespace Ofgem_Web_LAF_InternalPortal.Services;

public interface IDeclarationManagementService
{
    Task<List<Upload>> GetUploadsAwaitingADecision();
    Task<bool> ThereAreUploadsAwaitingADecision();
}


public class DeclarationManagementService(HttpClient httpClient,
    ILogger<DeclarationManagementService> logger)
    : IDeclarationManagementService
{
    public async Task<List<Upload>> GetUploadsAwaitingADecision()
    {
        List<Upload>? result = [];
        try
        {
            var httpResponseMessage = await httpClient.GetAsync(Services.DeclarationApi.RouteUploadsAwaitingDecision);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Upload>>();
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetUploads,
                $"An issue occurred getting uploads awaiting a decision, {ex.Message}");
        }
        return result;
    }

    public async Task<bool> ThereAreUploadsAwaitingADecision()
    {
        if (await GetUploadsAwaitingADecision() is { Count: > 0 })
        {
            return true;
        }

        return false;
    }
}