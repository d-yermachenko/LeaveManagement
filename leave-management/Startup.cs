using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LeaveManagement.Contracts;
using LeaveManagement.Repository.Entity;
using AutoMapper;
using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using LeaveManagement.PasswordGenerator;
using Microsoft.AspNetCore.Identity.UI.Services;
using LeaveManagement.EmailSender;

namespace LeaveManagement {
    public class Startup {
        public Startup(IConfiguration configuration) {

            Configuration = configuration;
        }



        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<ApplicationDbContext>(options => {
                string activeConnectionName = Configuration.GetValue<string>("ActiveConnectionName");
                string connectionString = Configuration.GetConnectionString(activeConnectionName);
                options.UseSqlServer(connectionString);
            });
            services.AddAntiforgery();
            services.AddDataProtection()
                .SetApplicationName("leave-management")
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Configuration.GetValue<string>("KeysFolder")));
            services.AddScoped<ILeaveTypeRepositoryAsync, LeaveTypeRepository>();
            services.AddScoped<ILeaveAllocationRepositoryAsync, LeaveAllocationRepository>();
            services.AddScoped<ILeaveRequestsRepositoryAsync, LeaveRequestsRepository>();
            services.AddAutoMapper(typeof(Mappings.LeaveManagementMappings));
            GlobalizationStartup.ConfigureServices(services);
            services.AddScoped<IEmployeeRepositoryAsync, EmployeeRepository>();
            services.AddDefaultIdentity<IdentityUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IPasswordGenerator>(
                (serviceProvider) => new PasswordGenerator.PasswordGenerator(
                    () => Configuration.GetSection(nameof(PasswordGeneratorOptions)).Get<PasswordGeneratorOptions>()
            ));
            services.AddTransient<IEmailSender>(
                (serviceProvider) => new EmailSender.SmtpEmailSender(
                    () => Configuration.GetSection(nameof(SmtpSettings)).Get<EmailSender.SmtpSettings>()
                )
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManagement,
            ApplicationDbContext applicationDbContext) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var logger = app.ApplicationServices.GetService<ILogger>();
            var seedingTask = applicationDbContext.Database.MigrateAsync().ContinueWith(task => SeedData.Seed(userManager, roleManagement, logger).Wait()
            , TaskContinuationOptions.OnlyOnRanToCompletion);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{culture}/{controller}/{action}/{id?}",
                    defaults: new { culture = CultureInfo.CurrentCulture.Name, controller = "Home", action = "Index" });
                endpoints.MapRazorPages();
            });
            GlobalizationStartup.Configure(app, env);
            try {
                seedingTask.Wait();
            }
            catch (AggregateException ae) {
                var nae = ae.Flatten();
                logger?.LogError(nae, "Aggregate exception when migrating and seeding database", null);
            }
        }
    }
}
