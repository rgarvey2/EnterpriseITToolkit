using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using EnterpriseITToolkit.Common;
using EnterpriseITToolkit.Services;

namespace EnterpriseITToolkit
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            try
            {
                // Configure application
                var configuration = BuildConfiguration();
                var serviceProvider = ConfigureServices(configuration);

                // Initialize global exception handling
                var logger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
                GlobalExceptionHandler.Initialize(logger);
                GlobalExceptionHandler.SetupGlobalExceptionHandling();

                logger.LogInformation("Starting Enterprise IT Toolkit v4.0");

                // Validate configuration
                var configValidator = serviceProvider.GetRequiredService<IConfigurationValidationService>();
                var validationResult = await configValidator.ValidateConfigurationAsync();
                
                if (!validationResult.Success)
                {
                    var errorMessage = $"Configuration validation failed:\n\n{string.Join("\n", validationResult.ValidationChecks.Where(vc => !vc.Passed).Select(vc => $"- {vc.Name}: {vc.Message}"))}";
                    MessageBox.Show(errorMessage, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Initialize correlation service
                var correlationService = serviceProvider.GetRequiredService<ICorrelationService>();
                correlationService.SetCorrelationId(correlationService.GenerateCorrelationId());

                // Enable visual styles
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Show login form first
                var loginForm = serviceProvider.GetRequiredService<Forms.LoginForm>();
                var loginResult = loginForm.ShowDialog();
                
                if (loginResult == DialogResult.OK && loginForm.AuthenticationResult?.Success == true)
                {
                    logger.LogInformation("User {Username} authenticated successfully", loginForm.AuthenticationResult.Username);
                    
                    // Run the main form
                    var mainForm = serviceProvider.GetRequiredService<MainForm>();
                    Application.Run(mainForm);
                }
                else
                {
                    logger.LogInformation("Authentication failed or cancelled");
                }

                logger.LogInformation("Enterprise IT Toolkit shutdown completed");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to start application: {ex.Message}\n\n" +
                                 $"Exception Type: {ex.GetType().Name}\n" +
                                 $"Stack Trace:\n{ex.StackTrace}";
                
                MessageBox.Show(errorMessage, 
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Try to log the error if possible
                try
                {
                    Log.Error(ex, "Application startup failed");
                }
                catch
                {
                    // Ignore logging errors during startup failure
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            
            // Configure services
            services.ConfigureServices(configuration);
            
            // Register main form
            services.AddTransient<MainForm>();

            return services.BuildServiceProvider();
        }
    }
}
