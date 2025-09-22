using Microsoft.Extensions.Logging;
using System.Windows.Forms;
using System.Security;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Common
{
    public static class GlobalExceptionHandler
    {
        private static ILogger? _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static void HandleException(Exception ex, string? context = null)
        {
            try
            {
                var contextInfo = string.IsNullOrEmpty(context) ? "Unknown" : context;
                
                _logger?.LogError(ex, "Unhandled exception in context: {Context}", contextInfo);
                
                // Log security audit event for critical exceptions
                if (ex is SecurityException || ex is UnauthorizedAccessException)
                {
                    AuditLogger.LogSecurityEvent(_logger!, "CRITICAL_EXCEPTION", 
                        $"Security-related exception in {contextInfo}: {ex.Message}");
                }

                // Show user-friendly error message
                var errorMessage = GetUserFriendlyErrorMessage(ex);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception handlerEx)
            {
                // Fallback error handling
                MessageBox.Show($"A critical error occurred: {handlerEx.Message}", 
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        public static Task HandleAsyncException(Exception ex, string? context = null)
        {
            try
            {
                var contextInfo = string.IsNullOrEmpty(context) ? "Unknown" : context;
                
                _logger?.LogError(ex, "Unhandled async exception in context: {Context}", contextInfo);
                
                // Log security audit event for critical exceptions
                if (ex is SecurityException || ex is UnauthorizedAccessException)
                {
                    AuditLogger.LogSecurityEvent(_logger!, "CRITICAL_ASYNC_EXCEPTION", 
                        $"Security-related async exception in {contextInfo}: {ex.Message}");
                }

                // Show user-friendly error message on UI thread
                if (Application.OpenForms.Count > 0)
                {
                    var mainForm = Application.OpenForms[0];
                    if (mainForm != null && mainForm.InvokeRequired)
                    {
                        mainForm.Invoke(new Action(() => 
                        {
                            var errorMessage = GetUserFriendlyErrorMessage(ex);
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    else
                    {
                        var errorMessage = GetUserFriendlyErrorMessage(ex);
                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception handlerEx)
            {
                // Fallback error handling
                MessageBox.Show($"A critical error occurred: {handlerEx.Message}", 
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            
            return Task.CompletedTask;
        }

        private static string GetUserFriendlyErrorMessage(Exception ex)
        {
            return ex switch
            {
                SecurityException => "Access denied. You don't have permission to perform this action.",
                UnauthorizedAccessException => "Access denied. Please run the application as administrator.",
                FileNotFoundException => "Required file not found. Please check the installation.",
                DirectoryNotFoundException => "Required directory not found. Please check the installation.",
                TimeoutException => "Operation timed out. Please try again.",
                ArgumentException => "Invalid input provided. Please check your input and try again.",
                InvalidOperationException => "Operation cannot be performed at this time. Please try again later.",
                NotSupportedException => "This operation is not supported on your system.",
                _ => "An unexpected error occurred. Please contact your system administrator."
            };
        }

        public static void SetupGlobalExceptionHandling()
        {
            // Handle UI thread exceptions
            Application.ThreadException += (sender, e) =>
            {
                HandleException(e.Exception, "UI Thread");
            };

            // Handle non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    HandleException(ex, "App Domain");
                }
            };

            // Handle async exceptions
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                _ = HandleAsyncException(e.Exception, "Task Scheduler");
                e.SetObserved(); // Mark as handled
            };
        }
    }
}
