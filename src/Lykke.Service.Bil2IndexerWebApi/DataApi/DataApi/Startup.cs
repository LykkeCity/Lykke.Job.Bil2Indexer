using DataApi.Factories;
using DataApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace DataApi
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Data API", Version = "v1" });
            });

            //ToDo: register all services / factories from assembly, not 1 by 1
            services.AddSingleton<IAddressService, AddressService>();

            services.AddSingleton<IBlockService, BlockService>();

            services.AddSingleton<IAddressService, AddressService>();

            services.AddSingleton<IAssetService, AssetService>();

            services.AddSingleton<ITransactionService, TransactionService>();

            services.AddSingleton<IAssetModelFactory, AssetModelFactory>();

            services.AddSingleton<IBlockModelFactory, BlockModelFactory>();

            services.AddSingleton<ITransactionModelFactory, TransactionModelFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data API");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}
