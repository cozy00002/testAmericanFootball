using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TestAmericanFootball2.Models;
using AutoMapper;
using TestAmericanFootball2.ViewModels;
using TestAmericanFootball2.Extentions;

namespace TestAmericanFootball2
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
            services.AddMvc();

            services.AddDbContext<TestAmericanFootball2Context>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("TestAmericanFootball2Context")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            _mapperInitilize();
        }

        private void _mapperInitilize()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Game, AmeFootViiewModel>()
                .ForMember(d => d.RemainYards, o => o.MapFrom(s => s.RemainYards.ToString("0")))
                .ForMember(d => d.GainYards, o => o.MapFrom(s => s.GainYards.ToString("0")))
                .ForMember(d => d.RemainTime, o => o.MapFrom(s => s.RemainSeconds.ConvertMinSec()));
                cfg.CreateMap<AmeFootViiewModel, Game>();
            });
        }
    }
}