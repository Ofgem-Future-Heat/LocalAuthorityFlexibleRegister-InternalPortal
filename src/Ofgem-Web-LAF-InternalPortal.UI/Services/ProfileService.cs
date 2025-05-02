using Ofgem.API.LAF.UserManagement.Application.Exceptions;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem_Web_LAF_InternalPortal.Services
{
    public interface IProfileService
    {
        Task<Result<bool>> ProfileExistsAsync(string name);
    }
    public class ProfileService : IProfileService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            IHttpClientFactory httpClientFactory,
            ILogger<ProfileService> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<bool>> ProfileExistsAsync(string name)
        {
            try
            {
                _logger.LogLafInformation(LogEvents.Profiles);
                var httpClient = _httpClientFactory.CreateClient(Services.LocalAuthorityApi.ApiName);

                var httpResponseMessage =
                    await httpClient.GetAsync(Services.LocalAuthorityApi.RouteNameExists + $"{name}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Success(false);
            }
            catch (Exception ex)
            {
                _logger.LogLafError(ex, LogEvents.Profiles);
                
                throw new ProfileRetrievalException();
            }
        }

    }
}
