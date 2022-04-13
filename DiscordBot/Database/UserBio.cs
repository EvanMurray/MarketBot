using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Database
{
    public class UserBio
    {
        [Key]
        public long ID { get; set; }
        public string User { get; set; }
        public string Name { get; set; }
        public string Village { get; set; }
        public string Rank { get; set; }
        public string Level { get; set; }
        public string PrimaryNature { get; set; }
        public string SecondaryNature { get; set; }
    }
}
