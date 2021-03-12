using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace CityInfo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public void ConfigureServices(IServiceCollection services) // This serves as middleware for this application
        {
            // This allows application to access MVC related stuff
            services.AddMvc().AddMvcOptions(o =>
            {
                o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            }); // This adds the option for our json data to return in XML format. Default is Json() because it's the first one in the outputFormatter collection
#if DEBUG 
            services.AddTransient<IMailService, LocalMailService>(); // Service is created each time it's requested, work best with lightweight stateless services 
                                                                     // This will be injected in POI Controller

#else
            services.AddTransient<IMailService, CloudMailService>(); // This will only be used during production
#endif

            /*
             services.AddMvc().AddJsonOptions(o =>
            {
                if (o.SerializerSettings.ContractResolver != null)
                {
                    var castedResolver = o.SerializerSettings.ContractResolver as DefaultContractResolver;
                    castedResolver.NamingStrategy = null;
                }
            });
             */ // The purpose of this JsonOption is just to turn the first letter in the result we get back to uppercase....which is alot of work for silly little issue...lol
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
                app.UseExceptionHandler();
            }

            // This is optional
            app.UseStatusCodePages(); // This gives the user text page message when navigate to an un-specified route / invalid route

            app.UseMvc(); // From here on, the MVC middleware will handle all the HTTP requests

      
        }
    }
}
