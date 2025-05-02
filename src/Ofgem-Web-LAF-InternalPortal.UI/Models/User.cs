using Ofgem.LAF.SharedLibrary.Models;
using Ofgem_Web_LAF_InternalPortal.Services;
using System.Security.Claims;
using System.Text.Json;
using System.Web;

namespace Ofgem_Web_LAF_InternalPortal.Models
{
    public class User
    {
        public Ofgem.LAF.SharedLibrary.Models.User Data { get; }

        public User(ClaimsPrincipal user, IHttpClientFactory httpClientFactory)
        {
            var role = DetermineRole(user);

            var getTask = GetUserDetail(user.Identity!.Name!, httpClientFactory);
            getTask.Wait();

            Data = getTask.Result!;

            // update with the data provided by Azure Active Directory if no record found we will still have the email and role
            Data.EmailAddress = user.Identity.Name;

            if (Data.Role != role)
            {
                Data.Role = role;

                var updateTask = UpdateRoleDetail(Data, httpClientFactory);
                updateTask.Wait();
            }
        }

        /// <summary>
        /// Determines the name of the Claims Principal Role assigned to the user via Azure Active Directory for the application
        /// </summary>
        /// <param name="user">ClaimsPrincipal</param>
        /// <returns>Empty string or name of the service role assigned</returns>
        private static string DetermineRole(ClaimsPrincipal? user)
        {
            if (user == null) return string.Empty;

            if (user.IsInRole(UserRoles.Basic)) return nameof(UserRoles.Basic);
            if (user.IsInRole(UserRoles.Standard)) return nameof(UserRoles.Standard);
            if (user.IsInRole(UserRoles.Advanced)) return nameof(UserRoles.Advanced);
            if (user.IsInRole(UserRoles.Expert)) return nameof(UserRoles.Expert);
            if (user.IsInRole(UserRoles.Admin)) return nameof(UserRoles.Admin);

            return string.Empty;
        }

        private static async Task<Ofgem.LAF.SharedLibrary.Models.User?> GetUserDetail(string emailAddress, IHttpClientFactory httpClientFactory)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

                var httpResponseMessage = await httpClient.GetAsync($"{UserApi.RouteGetByEmail}/{HttpUtility.UrlEncode(emailAddress)}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string stringToAvoidMemoryStreamReadingIssue = await httpResponseMessage.Content.ReadAsStringAsync();

                    Ofgem.LAF.SharedLibrary.Models.User? userData = JsonSerializer.Deserialize<Ofgem.LAF.SharedLibrary.Models.User>(stringToAvoidMemoryStreamReadingIssue);

                    return userData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return new Ofgem.LAF.SharedLibrary.Models.User();
        }

        /// <summary>
        /// Put the alteration of the Azure Active Directory Role back into the DB
        /// </summary>
        /// <param name="data"></param>
        /// <param name="httpClientFactory"></param>
        /// <returns></returns>
        private static async Task<bool> UpdateRoleDetail(Ofgem.LAF.SharedLibrary.Models.User data, IHttpClientFactory httpClientFactory)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(UserApi.ApiName);

                var httpResponseMessage = await httpClient.PutAsJsonAsync(UserApi.RoutePut, data);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return false;
        }
    }
}
