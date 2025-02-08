namespace JboxTransfer.Server.Models.Output
{
    public class UserInfoDto
    {
        public string Name { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; }
        public string Jaccount { get; set; }
        public string Preference { get; set; }
        public long TotalTransferredBytes { get; set; }
        public long JboxSpaceUsedBytes { get; set; }
        public bool OnlyFullTransfer { get; set; }
    }
}
