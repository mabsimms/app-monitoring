using Microsoft.AspNetCore.Builder;

namespace Microsoft.AzureCAT.Extensions.Logging.Plugins
{
    public static class PluginExtensions
    {
        public static IApplicationBuilder UseHttpLoggingModule(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HttpLoggingModule>();
        }
    }
}
