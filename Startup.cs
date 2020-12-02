using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YoYoMooc.ExampleApp.Models;

namespace YoYoMooc.ExampleApp
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
      // services.AddTransient<IProductRepository, MockProductRepository>();
      services.AddTransient<IProductRepository, DataProductRepository>();
      services.AddRazorPages();

      var host = Configuration["DBHOST"] ?? "localhost";
      var port = Configuration["DBPORT"] ?? "3306";
      var password = Configuration["DBPASSWORD"] ?? "123456";


      var connectionStr = $"server={host};userid=root;pwd={password};"
      + $"port={port};database=products";

      services.AddDbContextPool<ProductDbContext>(options => options.UseMySql(connectionStr));

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
      });

      app.UseDataInitializer();
    }
  }
}
