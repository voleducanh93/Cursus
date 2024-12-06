using Cursus.Common.Helper;
using Cursus.Common.Middleware;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.Repository;
using Cursus.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Demo_PayPal.Service;
using System.Threading.RateLimiting;
using Cursus.Service.Services;
using Cursus.API.Hubs;
using Cursus.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.CodeAnalysis.Scripting;
using System;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Cursus.Service.Hubs;
using Cursus.Common.Middleware.AuthorizeHandler;


namespace Cursus.API
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public class Program
    {
        /// The main method for the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var fullVersion = Assembly.GetExecutingAssembly()
                                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                      .InformationalVersion ?? "1.0.0";

            // Trim the commit hash suffix if it exists
            var version = fullVersion.Split('+')[0];

            //Add SignalR
            builder.Services.AddSignalR();

            // Add logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddRepository().AddService();

            builder.Services.AddExceptionHandler();

            builder.Services.AddProblemDetails();

            builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSettings"));

            // Add services to the container.
            builder.Services.AddDbContext<CursusDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            var paypalSettings = builder.Configuration.GetSection("PayPal");
            builder.Services.Configure<PayPalSetting>(paypalSettings);

            // Register PayPalClient and TransactionMonitoringService
            builder.Services.AddScoped<PayPalClient>().AddHostedService<TransactionMonitoringService>();
            builder.Services.AddScoped<SqlScriptRunner>();
            // Add JWT Configuration
            var jwtSecret = builder.Configuration["JWT:Key"]; ;
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new ArgumentNullException(nameof(jwtSecret), "JWT Secret cannot be null or empty.");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };

            });

            //policy authorize
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));
                options.AddPolicy("FPTAdminOnly", policy => policy.AddRequirements(new IsFPTAdminRequirement("fpt.edu.vn")));
                //options.AddPolicy("FPTAdminOnly", policy => policy.RequireAssertion(context => context.User.HasClaim(claim => claim.Type == "Sub" && claim.Value.EndsWith("@fpt.edu.vn"))));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<CursusDbContext>()
                .AddDefaultTokenProviders();

            // Add Rate Limit
            builder.Services.AddRateLimiter(option =>
            {
                option.AddFixedWindowLimiter("default", c =>
                {
                    c.Window = TimeSpan.FromHours(1);
                    c.PermitLimit = 1000;
                    c.QueueLimit = 1000;
                    c.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                option.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            //SignalIR DashBoard
            builder.Services.AddSignalR();

            // Configure Swagger services
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("swagger",
                    new OpenApiInfo
                    {
                        Title = "Cursus API - "+ version,
                        Version = version
                    }
                 );
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type=ReferenceType.SecurityScheme,
                                        Id="Bearer"
                                    }
                                },
                                new string[]{}
                            }
                    });

                opt.IncludeXmlComments(Assembly.GetExecutingAssembly());
            });
            var app = builder.Build();

            app.MapHub<ChatHub>("/chatHub");

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var dbContext = serviceProvider.GetRequiredService<CursusDbContext>();
                var scriptRunner = serviceProvider.GetRequiredService<SqlScriptRunner>();
                var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                string baseDirectory = AppContext.BaseDirectory;

                string projectRootPath = GetSolutionRootDirectory();

                // Xây dựng đường dẫn đến thư mục SQL Scripts
                string scriptsFolderPath = Path.Combine(projectRootPath, "Cursus.Data", "SqlScripts", "StoredProcedures");

                // Chuẩn hóa đường dẫn
                scriptsFolderPath = Path.GetFullPath(scriptsFolderPath);

               
                if (Directory.Exists(scriptsFolderPath))
                {
                    try
                    {
                        await scriptRunner.ExecuteAllSqlScriptsAsync(scriptsFolderPath);
                        Console.WriteLine("SQL scripts executed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing scripts: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Scriptsssss folder not found: {scriptsFolderPath}");
                }

            }
			app.MapHub<NotificationHub>("/notificationHub");


			// Configure the HTTP request pipeline.
			app.UseSwagger(options =>
            {
                options.RouteTemplate = "/openapi/{documentname}.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/openapi/swagger.json", "Cursus API");
                c.RoutePrefix = string.Empty;
            });
            app.MapHub<StatisticsHub>("/statisticsHub"); // Cấu hình Hub
            app.MapScalarApiReference();

            

            app.UseStaticFiles();

            app.UseRateLimiter();

            app.UseExceptionHandler("/error");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        static string GetSolutionRootDirectory()
        {
            // Bắt đầu từ thư mục thực thi hiện tại
            string directory = AppDomain.CurrentDomain.BaseDirectory;

            // Lặp để tìm file .sln
            while (!string.IsNullOrEmpty(directory))
            {
                if (Directory.GetFiles(directory, "*.sln").Length > 0)
                {
                    return directory; // Trả về thư mục chứa file .sln
                }

                // Đi lên một cấp thư mục
                directory = Directory.GetParent(directory)?.FullName;
            }

            throw new Exception("Không tìm thấy file .sln trong cây thư mục.");
        }
    }


}
