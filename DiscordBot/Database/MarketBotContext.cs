using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace DiscordBot.Database
{
    public class MarketBotContext : DbContext
    {
        public DbSet<UserBio> UserBios { get; set; }
        public DbSet<UserTimer> UserTimers { get; set; }
        public DbSet<MarketItem> MarketItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();
            string connection = configuration.GetConnectionString("DockerConnection");
            var builder = optionsBuilder.UseSqlServer(connection);
          
        }
    }
}
