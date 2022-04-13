using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DiscordBot.Database
{
    public class MarketItem
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string Offer { get; set; }
        public string User { get; set; }

    }
}
