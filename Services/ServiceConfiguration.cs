using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;
using EnterpriseITToolkit.Services;
using EnterpriseITToolkit.Security;
using EnterpriseITToolkit.Data;

namespace EnterpriseITToolkit
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Serilog with fallback
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
            }
            catch (Exception ex)
            {
                // Fallback to basic console logging if configuration fails
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/EnterpriseITToolkit-.txt", rollingInterval: Serilog.RollingInterval.Day)
                    .CreateLogger();
                
                Log.Warning(ex, "Failed to load Serilog configuration, using fallback configuration");
            }

            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });

            // Add configuration
            services.AddSingleton(configuration);

            // Add services
            services.AddSingleton<INetworkService, NetworkService>();
            services.AddSingleton<ISystemHealthService, SystemHealthService>();
            services.AddSingleton<ISecurityService, SecurityService>();
            services.AddSingleton<IActiveDirectoryService, ActiveDirectoryService>();
            services.AddSingleton<IWindows11Service, Windows11Service>();
            services.AddSingleton<IAutomationService, AutomationService>();
            services.AddSingleton<ITroubleshootingService, TroubleshootingService>();
            services.AddSingleton<IReportingService, ReportingService>();
            services.AddSingleton<IWorkstationService, WorkstationService>();
            
            // Add new enhanced services
            services.AddSingleton<IConfigurationValidationService, ConfigurationValidationService>();
            services.AddSingleton<ICorrelationService, CorrelationService>();
            services.AddSingleton<IPerformanceDashboardService, PerformanceDashboardService>();
            services.AddSingleton<IPluginService, PluginService>();
            
            // Add enhanced security services
            services.AddScoped<IEnhancedAuthenticationService, EnhancedAuthenticationService>();
            services.AddScoped<ITotpService, TotpService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            
            // Add advanced security services
            services.AddScoped<ISiemService, SiemService>();
            services.AddScoped<IThreatDetectionService, ThreatDetectionService>();
            services.AddHttpClient<SiemService>();
            
            // Add automation services
            services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            
            // Add machine learning services
            services.AddSingleton<IMachineLearningService, MachineLearningService>();
            
            // Add database context
            services.AddDbContext<EnterpriseDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            // Add caching services
            services.AddMemoryCache();
            services.AddSingleton<ICachingService, CachingService>();
            
            // Add Redis if configured
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                    ConnectionMultiplexer.Connect(redisConnectionString));
            }
            
            // Add health checks
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database")
                .AddCheck<NetworkHealthCheck>("network")
                .AddCheck<DiskSpaceHealthCheck>("disk_space")
                .AddCheck<MemoryHealthCheck>("memory")
                .AddCheck<ServiceHealthCheck>("services");
            
            // Add health check services
            services.AddScoped<DatabaseHealthCheck>();
            services.AddScoped<NetworkHealthCheck>();
            services.AddScoped<DiskSpaceHealthCheck>();
            services.AddScoped<MemoryHealthCheck>();
            services.AddScoped<ServiceHealthCheck>();
            
            // Add web API services
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            
            // Add SignalR for real-time updates
            services.AddSignalR();
            
            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            
            // Add forms
            services.AddTransient<Forms.LoginForm>();

            // Add feature manager
            services.AddSingleton<FeatureManager>(provider =>
            {
                var tabControl = new TabControl(); // This will be set by MainForm
                return new FeatureManager(tabControl, 
                    provider.GetRequiredService<ILogger<FeatureManager>>(), 
                    provider);
            });

            return services;
        }
    }
}
