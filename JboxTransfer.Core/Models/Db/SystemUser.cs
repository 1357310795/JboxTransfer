using System.ComponentModel.DataAnnotations;

namespace JboxTransfer.Core.Models.Db
{
    public class SystemUser
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Avatar { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string Jaccount { get; set; }
        public string Cookie { get; set; }
        public string Role { get; set; }

        public int StatId { get; set; }
        public UserStatistics Stat { get; set; }

        public string Preference { get; set; }
    }
}
