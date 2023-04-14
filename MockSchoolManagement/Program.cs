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

            #region Identityע��
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
            #region Microsoft��������֤
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
            // ���Խ��������Ȩ
            builder.Services.AddAuthorization(options =>
            {
                //options.AddPolicy("DeleteRolePolicy",
                //   policy => policy.RequireClaim("Delete Role"));

                //options.AddPolicy("AdminRolePolicy",
                //   policy => policy.RequireRole("Admin"));

                ////���Խ�϶����ɫ������Ȩ
                //options.AddPolicy("SuperAdminPolicy", policy =>
                //  policy.RequireRole("Admin", "User", "SuperManager"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));
                options.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
                //���÷���ʧ��ʱ�����������������
                options.InvokeHandlersAfterFailure = false;
                //options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role","true"));

                ////���Խ�Ͻ�ɫ��Ȩ
                //options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));

                ////���Խ�϶����ɫ������Ȩ
                //options.AddPolicy("SuperAdminPolicy", policy => policy.RequireRole("Admin", "User", "SuperManager"));

                //��ʽд��
                //options.AddPolicy("EditRolePolicy", policy => policy
                //    .RequireRole("Admin")
                //    .RequireClaim("Edit Role","true")
                //    .RequireRole("Super Admin")
                //    );

                ////ʹ��ί���Զ������
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")));

                //��װ
                //    options.AddPolicy("EditRolePolicy",policy => 
                //        policy.RequireAssertion(context => AuthorizeAccess(context)))

                //});

                //bool AuthorizeAccess(AuthorizationHandlerContext context)
                //{
                //    return context.User.IsInRole("Admin") &&
                //            context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //            context.User.IsInRole("Super Admin");
                //}
                //�Զ��帴����Ȩ
                //And��ϵ
                //options.AddPolicy("EditRolePolicy",policy =>policy
                //    .RequireClaim("Edit Role","true","yes")
                //    .RequireRole("Admin"));

                //OR��ϵ
                //options.AddPolicy("EditRolePolicy",
                //    policy => policy.RequireAssertion(context =>
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")));

            });
            //ע���Զ�����Ȩ�������
            builder.Services.AddSingleton<IAuthorizationHandler,CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler,SuperAdminHandler>();
            #endregion

            #region �޸�MVC��Ĭ��Cookie������Ϣ
            builder.Services.ConfigureApplicationCookie(options =>
            {
                //�޸ľܾ����ʵ�·�ɵ�ַ
                options.AccessDeniedPath = new PathString("/Admin/AccessDenied");
                //�޸ĵ�¼��ַ��·��
                //   options.LoginPath = new PathString("/Admin/Login");  
                //�޸�ע����ַ��·��
                //   options.LogoutPath = new PathString("/Admin/LogOut");
                //ͳһϵͳȫ�ֵ�Cookie����
                options.Cookie.Name = "MockSchoolCookieName";
                // ��¼�û�Cookie����Ч�� 
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                //�Ƿ��Cookie���û�������ʱ�䡣
                options.SlidingExpiration = true;
            });
            #endregion

            #region NLog log4net��־

            //builder.Logging.ClearProviders();
            //builder.Logging.AddNLog("CfgFile/NLog.config");

            //Nuget���룺
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
            //app.UseHsts(); //Https�ϸ��䰲ȫЭ��
            #region ���þ�̬�ļ�
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

            #region װ���м��
            //app.UseMiddleware<>();
            #endregion

            #region ʹ��Useע���м��
            {
                //app.Use(async (context, next) =>  //ʹ��Useע���м��
                //{
                //    await context.Response.WriteAsync("<h1>Hello World!</h1>");
                //    await next();
                //});

                //app.Map("/Api/GetData", async context =>  //ʹ��Useע���м��
                //{
                //    await context.Response.WriteAsync("Hello World!");

                //});

                //app.MapGet("", async context =>  //ʹ��Useע���м��
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