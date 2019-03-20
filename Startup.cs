using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyRazorComponents.Components;
using MyRazorComponents.Services;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;

namespace MyRazorComponents {
  public class Startup {
    private Container container = new Container();

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {
      services.AddMvc()
          .AddNewtonsoftJson();

      services.AddRazorComponents();

      IntegrateSimpleInjector(services);
    }

    private void IntegrateSimpleInjector(IServiceCollection services) {
      container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      services.AddSingleton<IControllerActivator>(
        new SimpleInjectorControllerActivator(container));
      services.AddSingleton<IViewComponentActivator>(
        new SimpleInjectorViewComponentActivator(container));

      services.EnableSimpleInjectorCrossWiring(container);
      services.UseSimpleInjectorAspNetRequestScoping(container);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      InitializeContainer(app);

      container.Verify();

      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      } else {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting(routes => {
        routes.MapRazorPages();
        routes.MapComponentHub<App>("app");
      });
    }

    private void InitializeContainer(IApplicationBuilder app) {
      // Add application presentation components:
      container.RegisterMvcControllers(app);
      container.RegisterMvcViewComponents(app);

      container.RegisterSingleton<WeatherForecastService>();

      // Allow Simple Injector to resolve services from ASP.NET Core.
      container.AutoCrossWireAspNetComponents(app);
    }

  }
}
