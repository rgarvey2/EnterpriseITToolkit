using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Forms
{
    public partial class LoginForm : Form
    {
        private readonly ILogger<LoginForm> _logger;
        private readonly IAuthenticationService _authService;
        private TextBox usernameTextBox = null!;
        private TextBox passwordTextBox = null!;
        private Button loginButton = null!;
        private Button cancelButton = null!;
        private Label statusLabel = null!;
        private ProgressBar progressBar = null!;

        public AuthenticationResult? AuthenticationResult { get; private set; }

        public LoginForm(ILogger<LoginForm> logger, IAuthenticationService authService)
        {
            _logger = logger;
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form properties
            Text = "Enterprise IT Toolkit - Technician Login";
            Size = new Size(400, 300);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;

            // Title label
            var titleLabel = new Label
            {
                Text = "Enterprise IT Toolkit v4.0",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(50, 20),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle label
            var subtitleLabel = new Label
            {
                Text = "Technician Authentication Required",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Location = new Point(50, 50),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username label
            var usernameLabel = new Label
            {
                Text = "Username:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(50, 90),
                Size = new Size(80, 20)
            };

            // Username textbox
            usernameTextBox = new TextBox
            {
                Location = new Point(140, 88),
                Size = new Size(200, 23),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password label
            var passwordLabel = new Label
            {
                Text = "Password:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(50, 120),
                Size = new Size(80, 20)
            };

            // Password textbox
            passwordTextBox = new TextBox
            {
                Location = new Point(140, 118),
                Size = new Size(200, 23),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // Login button
            loginButton = new Button
            {
                Text = "Login",
                Location = new Point(140, 160),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            loginButton.Click += LoginButton_Click;

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(230, 160),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            cancelButton.Click += CancelButton_Click;

            // Status label
            statusLabel = new Label
            {
                Text = "Enter your technician credentials",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.LightGray,
                Location = new Point(50, 200),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(50, 220),
                Size = new Size(300, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Add controls to form
            Controls.Add(titleLabel);
            Controls.Add(subtitleLabel);
            Controls.Add(usernameLabel);
            Controls.Add(usernameTextBox);
            Controls.Add(passwordLabel);
            Controls.Add(passwordTextBox);
            Controls.Add(loginButton);
            Controls.Add(cancelButton);
            Controls.Add(statusLabel);
            Controls.Add(progressBar);

            // Set tab order
            usernameTextBox.TabIndex = 0;
            passwordTextBox.TabIndex = 1;
            loginButton.TabIndex = 2;
            cancelButton.TabIndex = 3;

            // Set default button
            AcceptButton = loginButton;
            CancelButton = cancelButton;

            // Add event handlers
            usernameTextBox.TextChanged += (s, e) => ClearStatus();
            passwordTextBox.TextChanged += (s, e) => ClearStatus();
            passwordTextBox.KeyPress += PasswordTextBox_KeyPress;
        }

        private async void LoginButton_Click(object? sender, EventArgs e)
        {
            await PerformLoginAsync();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PasswordTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                _ = PerformLoginAsync();
            }
        }

        private async Task PerformLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                ShowStatus("Please enter both username and password", Color.Red);
                return;
            }

            try
            {
                // Show progress
                SetLoadingState(true);
                ShowStatus("Authenticating...", Color.Yellow);

                // Perform authentication
                AuthenticationResult = await _authService.AuthenticateAsync(usernameTextBox.Text, passwordTextBox.Text);

                if (AuthenticationResult.Success)
                {
                    ShowStatus("Authentication successful", Color.Green);
                    _logger.LogInformation("User {Username} logged in successfully", AuthenticationResult.Username);
                    
                    // Wait a moment to show success message
                    await Task.Delay(1000);
                    
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ShowStatus(AuthenticationResult.Error ?? "Authentication failed", Color.Red);
                    passwordTextBox.Clear();
                    passwordTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", usernameTextBox.Text);
                ShowStatus("Authentication service error. Please try again.", Color.Red);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            loginButton.Enabled = !isLoading;
            cancelButton.Enabled = !isLoading;
            usernameTextBox.Enabled = !isLoading;
            passwordTextBox.Enabled = !isLoading;
            progressBar.Visible = isLoading;
        }

        private void ShowStatus(string message, Color color)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }

        private void ClearStatus()
        {
            if (statusLabel.ForeColor != Color.LightGray)
            {
                statusLabel.Text = "Enter your technician credentials";
                statusLabel.ForeColor = Color.LightGray;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            usernameTextBox.Focus();
        }
    }
}
