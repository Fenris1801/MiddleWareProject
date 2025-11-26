using CoreWCF;
using CoreWCF.Configuration;

namespace ProxyCacheServer
{
    public class HttpBindingStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddSingleton<ProxyCacheService>();

            services.AddServiceModelServices()
                    .AddServiceModelMetadata();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseServiceModel(builder =>
            {
                builder.AddService<ProxyCacheService>(serviceOptions => { })
                .AddServiceEndpoint<ProxyCacheService, IProxyCacheService>(new BasicHttpBinding(), "/ProxyCacheService");

                // WSDL
                var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
            });
        }
    }
}
