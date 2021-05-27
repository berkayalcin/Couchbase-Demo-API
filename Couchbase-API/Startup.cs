using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase_API.BucketProviders;
using Couchbase_API.Repositories;
using Couchbase_API.Services;
using Couchbase.Configuration.Client;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Couchbase_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ITravelRepository, TravelRepository>();
            services.AddScoped<ISolutionPartnerRepository, SolutionPartnerRepository>();
            services.AddScoped<IAirlineService, AirlineService>();
            services.AddScoped<ISolutionPartnerService, SolutionPartnerService>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Couchbase_Demo_API", Version = "v1"});
            });

            services.AddCouchbase(client =>
                {
                    var ipList = new List<string> {"http://localhost"}.Select(ip => new Uri(ip)).ToList();
                    client.Servers = ipList;
                    client.UseSsl = false;
                    client.Username = "Administrator";
                    client.Password = "verystrongpassword";
                    client.UseConnectionPooling = true;
                    client.ConnectionPool = new ConnectionPoolDefinition
                    {
                        SendTimeout = 120000,
                        MaxSize = 20,
                        MinSize = 20,
                        ConnectTimeout = 600000,
                        WaitTimeout = 600000,
                        ShutdownTimeout = 600000
                    };
                    client.OperationLifespan = 90000;
                })
                .AddCouchbaseBucket<ITravelBucketProvider>("travel-sample")
                .AddCouchbaseBucket<ISolutionPartnerBucketProvider>("solution-partners");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Couchbase_Demo_API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}