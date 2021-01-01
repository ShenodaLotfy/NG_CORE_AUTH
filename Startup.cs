using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NG_Core_Auth.Data;
using NG_Core_Auth.Helpers;
using System;
using System.Text;

namespace NG_Core_Auth
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
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // Adding CORS Cross-Origin requests, supports many domain requests
            services.AddCors(options =>
            {
                options.AddPolicy("EnableCORS", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials().Build();
                });
            });

            // Connect to Database, using out Data folder => ApplicationDbContext class we just created
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                {
                    // options used to set the ConnectionStrings
                    // use Configuration variable to get the conntection string from appsettings.json
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                }
            });

            // Specifying that we are going to use Identity Framework
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // required when creating new user
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.User.RequireUniqueEmail = true;

                // Lockout settings 
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;


            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();


            // ---------Tokens-----------
            // get AppSettings section in appsettings.json file 
            var appSettingsSection = Configuration.GetSection("AppSettings");
            // then send this section to AppSetting class in Helpers folder
            services.Configure<AppSettings>(appSettingsSection);
            // then we get those value from AppSettings class 
            var appSettings = appSettingsSection.Get<AppSettings>(); // the matched values between AppSettingsSection in appsettings.json file and AppSettings class
            // now we have matched value between both the class AppSettings and AppSettings section in appsettings.json file
            // now we encode the Secret key 
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);


            // Authentication Middleware setup 
            services.AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
           })
                // user JwtBearerDefaults with our appSettings class values 
           .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
             {
                 // TokenValidationParameters because we have to validate the token to make sure it\s valid 
                 options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                 {
                     // Enable site, audience and SiginKey options
                     ValidateIssuerSigningKey = true,
                     ValidateIssuer = true,
                     ValidateAudience = true,

                     // use our custom appSettings section values 
                     ValidIssuer = appSettings.Site,
                     ValidAudience = appSettings.Audience,
                     IssuerSigningKey = new SymmetricSecurityKey(key),
                 };

             });

            // Authorization Middleware to use in controllers like [Authorize(Policy = "LoggedIn")]
            services.AddAuthorization(options =>
           {
               options.AddPolicy("LoggedIn", policy => policy.RequireRole("Admin", "Customer", "Moderator").RequireAuthenticatedUser());
               options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Admin").RequireAuthenticatedUser());

           });
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

            // use CORS 
            app.UseCors("EnableCORS");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // make sure to add authentication pipeline in the pipeline before mvc UseEndPoints 
            app.UseAuthentication();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
