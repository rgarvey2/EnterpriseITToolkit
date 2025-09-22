namespace EnterpriseITToolkit.Services
{
    public interface IActiveDirectoryService
    {
        Task<List<ADUser>> GetUsersAsync();
        Task<List<ADGroup>> GetGroupsAsync();
        Task<ADUser?> GetUserAsync(string username);
        Task<List<ADUser>> SearchUsersAsync(string searchTerm);
    }

    public class ADUser
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime LastLogin { get; set; }
        public List<string> Groups { get; set; } = new();
    }

    public class ADGroup
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public List<string> Members { get; set; } = new();
    }
}
