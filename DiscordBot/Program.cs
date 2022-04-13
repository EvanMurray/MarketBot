using Microsoft.Extensions.Hosting;
using System;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Startup().StartAsync().GetAwaiter().GetResult();
        }
    }
}
