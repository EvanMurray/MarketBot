using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DiscordBot.Database
{
    public class UserTimer
    {
        [Key]
        public int ID { get; set; }
        public string User { get; set; }
        public string Command { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
