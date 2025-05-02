using System.Diagnostics.CodeAnalysis;

namespace Ofgem_Web_LAF_InternalPortal.Services
{
    [ExcludeFromCodeCoverage]
    public static class FeatureService
    {
        public static bool FeatureSwitchShowAdmin { get; set; }
        
        public static void ConfigureFeatureService(WebApplicationBuilder builder)
        {
            var abc = builder.Configuration["FeatureSwitchShowAdmin"]
                      ?? throw new ArgumentNullException(nameof(builder), @"FeatureSwitchShowAdmin is not configured");

            if (bool.TryParse(abc, out bool result)) 
            {
                FeatureSwitchShowAdmin = result;
            }
            else
            {
                FeatureSwitchShowAdmin = false;
            }
        }
    }
}
