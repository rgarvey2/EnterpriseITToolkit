using Microsoft.Extensions.Logging;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly ILogger<ActiveDirectoryService> _logger;

        public ActiveDirectoryService(ILogger<ActiveDirectoryService> logger)
        {
            _logger = logger;
        }

        public Task<List<ADUser>> GetUsersAsync()
        {
            var users = new List<ADUser>();

            try
            {
                _logger.LogInformation("Retrieving Active Directory users");

                using var context = new PrincipalContext(ContextType.Machine);
                using var searcher = new UserPrincipal(context);
                using var searchResults = new PrincipalSearcher(searcher);

                foreach (var result in searchResults.FindAll())
                {
                    if (result is UserPrincipal user)
                    {
                        var adUser = new ADUser
                        {
                            Username = user.SamAccountName ?? string.Empty,
                            DisplayName = user.DisplayName ?? string.Empty,
                            Email = user.EmailAddress ?? string.Empty,
                            IsEnabled = user.Enabled ?? false,
                            LastLogin = user.LastLogon ?? DateTime.MinValue
                        };

                        users.Add(adUser);
                    }
                }

                _logger.LogInformation("Retrieved {Count} Active Directory users", users.Count);
                AuditLogger.LogSystemAccess(_logger, "ADUserList", "Retrieved", true);

                return Task.FromResult(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Active Directory users");
                AuditLogger.LogSystemAccess(_logger, "ADUserList", "Failed", false);
                return Task.FromResult(users);
            }
        }

        public Task<List<ADGroup>> GetGroupsAsync()
        {
            var groups = new List<ADGroup>();

            try
            {
                _logger.LogInformation("Retrieving Active Directory groups");

                using var context = new PrincipalContext(ContextType.Machine);
                using var searcher = new GroupPrincipal(context);
                using var searchResults = new PrincipalSearcher(searcher);

                foreach (var result in searchResults.FindAll())
                {
                    if (result is GroupPrincipal group)
                    {
                        var adGroup = new ADGroup
                        {
                            Name = group.SamAccountName ?? string.Empty,
                            Description = group.Description ?? string.Empty,
                            MemberCount = group.Members?.Count ?? 0
                        };

                        groups.Add(adGroup);
                    }
                }

                _logger.LogInformation("Retrieved {Count} Active Directory groups", groups.Count);
                AuditLogger.LogSystemAccess(_logger, "ADGroupList", "Retrieved", true);

                return Task.FromResult(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Active Directory groups");
                AuditLogger.LogSystemAccess(_logger, "ADGroupList", "Failed", false);
                return Task.FromResult(groups);
            }
        }

        public Task<ADUser?> GetUserAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Username is null or empty");
                    return Task.FromResult<ADUser?>(null);
                }

                _logger.LogInformation("Retrieving user: {Username}", username);

                using var context = new PrincipalContext(ContextType.Machine);
                using var user = UserPrincipal.FindByIdentity(context, username);

                if (user != null)
                {
                    var adUser = new ADUser
                    {
                        Username = user.SamAccountName ?? string.Empty,
                        DisplayName = user.DisplayName ?? string.Empty,
                        Email = user.EmailAddress ?? string.Empty,
                        IsEnabled = user.Enabled ?? false,
                        LastLogin = user.LastLogon ?? DateTime.MinValue
                    };

                    _logger.LogInformation("User retrieved: {Username}", username);
                    AuditLogger.LogSystemAccess(_logger, "ADUserGet", username, true);

                    return Task.FromResult<ADUser?>(adUser);
                }

                _logger.LogWarning("User not found: {Username}", username);
                return Task.FromResult<ADUser?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user: {Username}", username);
                AuditLogger.LogSystemAccess(_logger, "ADUserGet", username, false);
                return Task.FromResult<ADUser?>(null);
            }
        }

        public Task<List<ADUser>> SearchUsersAsync(string searchTerm)
        {
            var users = new List<ADUser>();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogWarning("Search term is null or empty");
                    return Task.FromResult(users);
                }

                _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);

                using var context = new PrincipalContext(ContextType.Machine);
                using var searcher = new UserPrincipal(context);
                searcher.SamAccountName = $"*{searchTerm}*";
                
                using var searchResults = new PrincipalSearcher(searcher);

                foreach (var result in searchResults.FindAll())
                {
                    if (result is UserPrincipal user)
                    {
                        var adUser = new ADUser
                        {
                            Username = user.SamAccountName ?? string.Empty,
                            DisplayName = user.DisplayName ?? string.Empty,
                            Email = user.EmailAddress ?? string.Empty,
                            IsEnabled = user.Enabled ?? false,
                            LastLogin = user.LastLogon ?? DateTime.MinValue
                        };

                        users.Add(adUser);
                    }
                }

                _logger.LogInformation("Found {Count} users matching search term: {SearchTerm}", users.Count, searchTerm);
                AuditLogger.LogSystemAccess(_logger, "ADUserSearch", searchTerm, true);

                return Task.FromResult(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
                AuditLogger.LogSystemAccess(_logger, "ADUserSearch", searchTerm, false);
                return Task.FromResult(users);
            }
        }
    }
}
