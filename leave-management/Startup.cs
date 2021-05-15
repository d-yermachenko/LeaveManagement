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
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using LeaveManagement.PasswordGenerator;
using Microsoft.AspNetCore.Identity.UI.Services;
using LeaveManagement.EmailSender;
using Microsoft.AspNetCore.Http;
using LeaveManagement.Notifications;

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
            services.AddTransient<ILeaveManagementUnitOfWork, LeaveManagementUnitOfWork>();
            services.AddTransient<IVisualNotificationService, VisualNotificationService>();
            services.AddAutoMapper(typeof(Mappings.LeaveManagementMappings));
            GlobalizationStartup.ConfigureServices(services);

            services.AddDefaultIdentity<IdentityUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews(options=> {
                options.Filters.Add(typeof(Filters.HttpErrorFilter));
            });
            services.AddRazorPages(options => {
                options.Conventions.Add(new CustomLocalization.CultureTemplatePageRouteModelConvention());
            });
            /*services.Configure<PasswordHasherOptions>(options =>
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
            );*/
            services.AddTransient<IPasswordGenerator>(
                (serviceProvider) => new PasswordGenerator.MyPasswordGenerator(
                    () => Configuration.GetSection(nameof(MyPasswordGeneratorOptions)).Get<MyPasswordGeneratorOptions>()
            ));
            services.AddTransient<IEmailSender>(
                (serviceProvider) => new EmailSender.SmtpEmailSender(
                    () => Configuration.GetSection(nameof(SmtpSettings)).Get<EmailSender.SmtpSettings>(),
                    logger: serviceProvider.GetService<ILogger<IEmailSender>>()
                )
            );
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
            //app.UseStatusCodePagesWithRedirects("/Error/{0}");
            app.UseStatusCodePages (context => {
                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;

                if (response.StatusCode == (int)StatusCodes.Status404NotFound) {
                    response.Redirect($"/Error/{response.StatusCode}");
                }
                return Task.CompletedTask;
            });

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
                    name: "LocalizedDefault",
                    pattern: $"{{{GlobalizationStartup.CultureRoutePartName}}}/{{controller}}/{{action}}/{{id?}}",
                    defaults: new { culture = GlobalizationStartup.DefaultCulture.Name, controller = "Home", action = "Index" });
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
