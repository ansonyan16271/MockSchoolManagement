using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using MockSchoolManagement.DataRepositories;
using MockSchoolManagement.Infrastructure;

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
            builder.Services.AddControllersWithViews()
                .AddXmlSerializerFormatters();
            //builder.Services.AddScoped<IStudentRepository,MockStudentRepository>();
            builder.Services.AddScoped<IStudentRepository,SQLStudentRepository>();
            
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