using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordBot.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.HelpModule
{
    public class HelpCommands : InteractiveBase
    {
        private readonly CommandService _commands;

        public HelpCommands(CommandService commands)
        {
            _commands = commands;
        }

        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(125, 35, 117);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Help Menu",
                IconUrl = Context.Guild.IconUrl
            };
            embed.Description = "Use nin!help <command> to get more information about a specific command.";

            foreach(ModuleInfo module in _commands.Modules)
            {
                if (module.Name == "HelpCommands")
                {
                    continue;
                }
                StringBuilder sb = new StringBuilder();


                string seperator = "";
                foreach(CommandInfo command in module.Commands)
                {
                    //TODO: Permissions check
                    sb.Append($"{seperator + command.Name}");
                    seperator = ", ";
                }

                string commandDescription = sb.ToString();

                if(!string.IsNullOrEmpty(commandDescription))
                {
                    embed.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = commandDescription;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync(null, false, embed.Build());
        }

        [Command("help")]
        public async Task Help([Remainder] string commandName)
        {
            EmbedBuilder embed = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();

            SearchResult commandSearch = _commands.Search(Context, commandName);

            if (!commandSearch.IsSuccess)
            {
                embed.Description = "The command you entered could not be located.";
                embed.WithColor(255, 0, 0);
                await ReplyAsync(null, false, embed.Build());
            }
            else
            {
                embed.WithColor(125, 35, 117);
                embed.Author = new EmbedAuthorBuilder();
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = commandName
                };

                foreach (CommandMatch match in commandSearch.Commands)
                {
                    CommandInfo command = match.Command;
                    string[] commandAliases = command.Aliases.ToArray();

                    if (commandAliases.Length > 0)
                    {
                        sb.Append("**Alias(es):** ");
                        string separator = "";
                        foreach (string alias in commandAliases)
                        {
                            sb.Append(separator + alias);
                            separator = ", ";
                        }
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    sb.AppendLine($"**Summary:** {command.Summary}");
                    sb.AppendLine();

                    ExampleAttribute example = command.Attributes.OfType<ExampleAttribute>().FirstOrDefault();


                    if (example != null)
                    {
                        sb.AppendLine("**Example:** " + example.ExampleText);
                        sb.AppendLine();
                    }

                    embed.AddField(x =>
                    {
                        x.Name = "__Information__";
                        x.Value = sb.ToString();
                        x.IsInline = false;
                    });
                }
            }
            await ReplyAsync(null, false, embed.Build());
        }
    }
}
