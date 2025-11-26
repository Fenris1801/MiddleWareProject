using CoreWCF.Configuration;

namespace RoutingServer
{
    public class WebHttpBindingStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            services.AddServiceModelWebServices()
                    .AddServiceModelMetadata();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors();

            app.UseServiceModel(builder =>
            {
                builder.AddService<ServiceGPS>(serviceOptions => { })
                       .AddServiceWebEndpoint<ServiceGPS, IServiceGPS>("ServiceGPS");

                // WSDL
                var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
            });
        }
    }
}
