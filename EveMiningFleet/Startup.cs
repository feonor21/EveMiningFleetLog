using System;
using System.IO;
using AspNetCore.SEOHelper;
using AspNetCore.SEOHelper.Sitemap;
using EveMiningFleet.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EveMiningFleet
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
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("sessiondataprotection/"));

            services.AddDistributedMySqlCache(options =>
            {
                options.ConnectionString = System.Environment.GetEnvironmentVariable("DB_SESSION_connectionstring");
                options.TableName = "webusersessions";
                options.SchemaName = "eveminingfleetsession";
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(48);
                options.IOTimeout = TimeSpan.FromHours(48);
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.MaxAge = TimeSpan.FromDays(5);
            });


            services.AddDbContext<EveMiningFleetContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(System.Environment.GetEnvironmentVariable("DB_DATA_connectionstring"),
                    mySqlOptions =>
                    {
                        mySqlOptions.ServerVersion(new System.Version(5, 7, 31), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql)
                        .EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: System.TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    }
                    )
            );


            services.AddControllersWithViews();
            services.AddRazorPages();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (System.Environment.GetEnvironmentVariable("ENVIRONMENT") == "DevelopmentMonolith")
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Messages/errordefault");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();



            var list = new System.Collections.Generic.List<SitemapNode>();
            list.Add(new SitemapNode { LastModified = DateTime.UtcNow, Priority = 1.0, Url = "https://EveMiningFleet.ovh/" });
            list.Add(new SitemapNode { LastModified = DateTime.UtcNow, Priority = 0.9, Url = "https://EveMiningFleet.ovh/Tools/Reprocess" });
            list.Add(new SitemapNode { LastModified = DateTime.UtcNow, Priority = 0.9, Url = "https://EveMiningFleet.ovh/Tools/MoonReport" });
            list.Add(new SitemapNode { LastModified = DateTime.UtcNow, Priority = 0.5, Url = "https://EveMiningFleet.ovh/Home/WhatIShouldMine" });
            list.Add(new SitemapNode { LastModified = DateTime.UtcNow, Priority = 0.5, Url = "https://EveMiningFleet.ovh/Home/CCPCopyright" });
            new SitemapDocument().CreateSitemapXML(list, env.ContentRootPath + "/wwwroot");

            app.UseXMLSitemap(env.ContentRootPath + "/wwwroot");
            //app.UseRobotsTxt(env.ContentRootPath);

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "Default",
                            pattern: "",
                            defaults: new { controller = "Home", action = "Index" });
                endpoints.MapControllerRoute(name: "Default",
                            pattern: "Logout",
                            defaults: new { controller = "Home", action = "Logout" });
                endpoints.MapControllerRoute(name: "CCPCOPYRIGHT",
                            pattern: "Home/CCPCopyright",
                            defaults: new { controller = "Home", action = "CCPCopyright" });
                endpoints.MapControllerRoute(name: "RoadMap",
                            pattern: "Home/RoadMap",
                            defaults: new { controller = "Home", action = "RoadMap" });
                endpoints.MapControllerRoute(name: "Profil",
                            pattern: "Home/Profil",
                            defaults: new { controller = "Home", action = "Profil" });
                endpoints.MapControllerRoute(name: "ProfilHistory",
                            pattern: "Home/ProfilHistory",
                            defaults: new { controller = "Home", action = "ProfilHistory" });



                endpoints.MapControllerRoute(name: "Reprocess",
                            pattern: "Tools/Reprocess",
                            defaults: new { controller = "Tools", action = "Reprocess" });
                endpoints.MapControllerRoute(name: "ReprocessDownload",
                            pattern: "Tools/ReprocessDownload",
                            defaults: new { controller = "Tools", action = "ReprocessDownload" });
                endpoints.MapControllerRoute(name: "WhatIShouldMine",
                            pattern: "Tools/WhatIShouldMine",
                            defaults: new { controller = "Tools", action = "WhatIShouldMine" });


                endpoints.MapControllerRoute(name: "RapportMoon",
                            pattern: "Tools/MoonReport",
                            defaults: new { controller = "Tools", action = "MoonReport" });

                endpoints.MapControllerRoute(name: "FleetsDetails",
                            pattern: "Fleets/Details",
                            defaults: new { controller = "Fleets", action = "Details" });
                endpoints.MapControllerRoute(name: "FleetsDetailsPartial",
                            pattern: "Fleets/DetailsPartial",
                            defaults: new { controller = "Fleets", action = "DetailsPartial" });
                endpoints.MapControllerRoute(name: "FleetsCreate",
                            pattern: "Fleets/Create",
                            defaults: new { controller = "Fleets", action = "Create" });
                endpoints.MapControllerRoute(name: "FleetsClose",
                            pattern: "Fleets/Close",
                            defaults: new { controller = "Fleets", action = "Close" });
                endpoints.MapControllerRoute(name: "FleetsJoin",
                            pattern: "Fleets/Join",
                            defaults: new { controller = "Fleets", action = "Join" });
                endpoints.MapControllerRoute(name: "FleetsJoinAll",
                            pattern: "Fleets/JoinAll",
                            defaults: new { controller = "Fleets", action = "JoinAll" });
                endpoints.MapControllerRoute(name: "FleetsQuit",
                            pattern: "Fleets/Quit",
                            defaults: new { controller = "Fleets", action = "Quit" });
                endpoints.MapControllerRoute(name: "FleetsEditOption",
                            pattern: "Fleets/EditOption",
                            defaults: new { controller = "Fleets", action = "EditOption" });
                endpoints.MapControllerRoute(name: "FleetsTaxeAdd",
                            pattern: "Fleets/TaxeAdd",
                            defaults: new { controller = "Fleets", action = "TaxeAdd" });
                endpoints.MapControllerRoute(name: "FleetsTaxeEdit",
                            pattern: "Fleets/TaxeEdit",
                            defaults: new { controller = "Fleets", action = "TaxeEdit" });
                endpoints.MapControllerRoute(name: "FleetsTaxeDelete",
                            pattern: "Fleets/TaxeDelete",
                            defaults: new { controller = "Fleets", action = "TaxeDelete" });




                endpoints.MapControllerRoute(name: "LoginLogin",
                            pattern: "LoginCCP/Login",
                            defaults: new { controller = "Login", action = "LoginCCP" });
                endpoints.MapControllerRoute(name: "LoginJoinFleetByLink",
                            pattern: "Login/JoinFleetByLink",
                            defaults: new { controller = "Login", action = "JoinFleetByLink" });
                endpoints.MapControllerRoute(name: "LoginCallbackCCP",
                            pattern: "Login/CallbackCCP",
                            defaults: new { controller = "Login", action = "CallbackCCP" });
                endpoints.MapControllerRoute(name: "LoginSetTimeZone",
                            pattern: "Login/SetTimeZone",
                            defaults: new { controller = "Login", action = "SetTimeZone" });
                endpoints.MapControllerRoute(name: "Logintoogledarkmode",
                            pattern: "Login/toogledarkmode",
                            defaults: new { controller = "Login", action = "toogledarkmode" });






                endpoints.MapControllerRoute(name: "Messageserrordefault",
                            pattern: "Messages/errordefault",
                            defaults: new { controller = "Messages", action = "errordefault" });
                endpoints.MapControllerRoute(name: "Messageserror403",
                            pattern: "Messages/error403",
                            defaults: new { controller = "Messages", action = "error403" });
                endpoints.MapControllerRoute(name: "Messageserror404",
                            pattern: "Messages/error404",
                            defaults: new { controller = "Messages", action = "error404" });
                endpoints.MapControllerRoute(name: "Messageserror400",
                            pattern: "Messages/error400",
                            defaults: new { controller = "Messages", action = "error400" });

            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var DB = scope.ServiceProvider.GetService<EveMiningFleetContext>();
                DB.Database.EnsureCreated();
                DB.Database.Migrate();
            }

        }
    }
}
