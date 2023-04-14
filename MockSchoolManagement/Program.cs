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
using MockSchoolManagement.Security;
using NLog.Web;

namespace MockSchoolManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration["ConnectionStrings:MockStudentDBConnection"]));

            // Add services to the container.
            builder.Services.AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();
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
                options.SignIn.RequireConfirmedEmail = true;

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
               .AddEntityFrameworkStores<AppDbContext>()
               .AddDefaultTokenProviders();
            #region Microsoft第三方认证
            {
                builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
                })
                .AddGitHub(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
                });
            }
            #endregion
            // 策略结合声明授权
            builder.Services.AddAuthorization(options =>
            {
                //options.AddPolicy("DeleteRolePolicy",
                //   policy => policy.RequireClaim("Delete Role"));

                //options.AddPolicy("AdminRolePolicy",
                //   policy => policy.RequireRole("Admin"));

                ////策略结合多个角色进行授权
                //options.AddPolicy("SuperAdminPolicy", policy =>
                //  policy.RequireRole("Admin", "User", "SuperManager"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));
                options.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
                //设置返回失败时不调用其他处理程序
                options.InvokeHandlersAfterFailure = false;
                //options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role","true"));

                ////策略结合角色授权
                //options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));

                ////策略结合多个角色进行授权
                //options.AddPolicy("SuperAdminPolicy", policy => policy.RequireRole("Admin", "User", "SuperManager"));

                //链式写法
                //options.AddPolicy("EditRolePolicy", policy => policy
                //    .RequireRole("Admin")
                //    .RequireClaim("Edit Role","true")
                //    .RequireRole("Super Admin")
                //    );

                ////使用委托自定义策略
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")));

                //封装
                //    options.AddPolicy("EditRolePolicy",policy => 
                //        policy.RequireAssertion(context => AuthorizeAccess(context)))

                //});

                //bool AuthorizeAccess(AuthorizationHandlerContext context)
                //{
                //    return context.User.IsInRole("Admin") &&
                //            context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //            context.User.IsInRole("Super Admin");
                //}
                //自定义复杂授权
                //And关系
                //options.AddPolicy("EditRolePolicy",policy =>policy
                //    .RequireClaim("Edit Role","true","yes")
                //    .RequireRole("Admin"));

                //OR关系
                //options.AddPolicy("EditRolePolicy",
                //    policy => policy.RequireAssertion(context =>
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")));

            });
            //注册自定义授权处理程序
            builder.Services.AddSingleton<IAuthorizationHandler,CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler,SuperAdminHandler>();
            #endregion

            #region 修改MVC中默认Cookie配置信息
            builder.Services.ConfigureApplicationCookie(options =>
            {
                //修改拒绝访问的路由地址
                options.AccessDeniedPath = new PathString("/Admin/AccessDenied");
                //修改登录地址的路由
                //   options.LoginPath = new PathString("/Admin/Login");  
                //修改注销地址的路由
                //   options.LogoutPath = new PathString("/Admin/LogOut");
                //统一系统全局的Cookie名称
                options.Cookie.Name = "MockSchoolCookieName";
                // 登录用户Cookie的有效期 
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                //是否对Cookie启用滑动过期时间。
                options.SlidingExpiration = true;
            });
            #endregion

            #region NLog log4net日志

            //builder.Logging.ClearProviders();
            //builder.Logging.AddNLog("CfgFile/NLog.config");

            //Nuget引入：
            //1.Log4Net
            //2.Microsoft.Extensions.Logging.Log4Net.AspNetCore
            builder.Logging.AddLog4Net("CfgFile/log4net.Config");
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
            //app.UseHsts(); //Https严格传输安全协议
            #region 配置静态文件
            //DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            //defaultFilesOptions.DefaultFileNames.Clear();
            ////defaultFilesOptions.DefaultFileNames.Add("default.html");

            //app.UseDefaultFiles("/default.html");//new DefaultFilesOptions()
            //{
            //    DefaultFileNames = new[] { "default.html", "52abp.html"}
            //});


            //FileServerOptions fileServerOptions = new FileServerOptions();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            //fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("52abp.html");
            //app.UseFileServer("/52abp.html");
            app.UseStaticFiles();
            #endregion

            #region 装配中间件
            //app.UseMiddleware<>();
            #endregion

            #region 使用Use注册中间件
            {
                //app.Use(async (context, next) =>  //使用Use注册中间件
                //{
                //    await context.Response.WriteAsync("<h1>Hello World!</h1>");
                //    await next();
                //});

                //app.Map("/Api/GetData", async context =>  //使用Use注册中间件
                //{
                //    await context.Response.WriteAsync("Hello World!");

                //});

                //app.MapGet("", async context =>  //使用Use注册中间件
                //{
                //    await context.Response.WriteAsync("Hello World!");

                //});

                //app.MapWhen(context =>
                //{
                //    string queryString = context.Request.QueryString.Value;
                //    return queryString.Contains("Anson");
                //}, appBuilder =>
                //{
                //    appBuilder.Use(async (appcontext, next) =>
                //    {
                //        await appcontext.Response.WriteAsync("Hello World!");
                //        await next();
                //    });
                //});

                //app.UseWhen(context =>
                //{
                //    string queryString = context.Request.QueryString.Value;
                //    return queryString.Contains("Anson");
                //}, appBuilder =>
                //{
                //    appBuilder.Use(async (appcontext, next) =>
                //    {
                //        await appcontext.Response.WriteAsync("Hello World!");
                //        await next();
                //    });
                //});
            }
            #endregion
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");



            app.Run();
        }
    }
}