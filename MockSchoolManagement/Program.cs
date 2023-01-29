using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using MockSchoolManagement.CustomerMiddlewares;
using MockSchoolManagement.DataRepositories;
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using NLog.Web;

namespace MockSchoolManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextPool<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration["ConnectionStrings:MockStudentDBConnection"]));

            // Add services to the container.
            builder.Services.AddControllersWithViews(config=>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddXmlSerializerFormatters();
            //builder.Services.AddScoped<IStudentRepository,MockStudentRepository>();
            builder.Services.AddScoped<IStudentRepository, SQLStudentRepository>();

            #region Identity注册
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;

            });
            //builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            //{
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequiredUniqueChars = 3;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireUppercase = false;
            //})
            //    .AddEntityFrameworkStores<AppDbContext>();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
               .AddEntityFrameworkStores<AppDbContext>();
            #endregion

            #region NLog log4net日志

            builder.Logging.ClearProviders();
            builder.Logging.AddNLog("CfgFile/NLog.config");

            //Nuget引入：
            //1.Log4Net
            //2.Microsoft.Extensions.Logging.Log4Net.AspNetCore
            //builder.Logging.AddLog4Net("CfgFile/log4net.Config");
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //if(app.Environment.IsEnvironment("UAT"))
                //{
                //    app.UseExceptionHandler("/Home/Error");
                //}

                //app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions()
                //{
                //    SourceCodeLineCount= 5,
                //});
                app.UseExceptionHandler("/Home/Error");
                //app.UseStatusCodePages();
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            //app.UseDefaultFiles(new DefaultFilesOptions()
            //{
            //    DefaultFileNames = new[] { "default.html","52abp.html" }
            //});

            //app.UseFileServer("/52abp.html");

            app.UseStaticFiles();

            app.UseAuthentication();
            

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //app.Use(async (context, next) =>  //使用Use注册中间件
            //{
            //    context.Response.WriteAsync("Hello World!");
            //    await next();
            //});
            app.Run();
        }
    }
}