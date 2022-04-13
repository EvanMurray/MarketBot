using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.UserBioModule
{
    public class UserBioCommands : InteractiveBase
    {
        [Command("set-bio", RunMode = RunMode.Async)]
        [Alias("setbio")]
        [Summary("Sets up a bio for the user. It will prompt you for each answer. There is a 15 second timeout per question. Used as nin!setbio")]
        public async Task SetBio()
        {
            SocketMessage response = null;
            List<string> userBioFields = new List<string> { "name", "village", "rank", "level", "primary nature", "secondary nature" };
            Dictionary<string, string> userBioAnswers = new Dictionary<string, string>();
            userBioAnswers.Add("name", "");
            userBioAnswers.Add("village", "");
            userBioAnswers.Add("rank", "");
            userBioAnswers.Add("level", "");
            userBioAnswers.Add("primary nature", "");
            userBioAnswers.Add("secondary nature", "");
            bool isSuccessful = true;
            
            for(int i = 0; i < userBioFields.Count; i++)
            {
                if (userBioFields[i] == "secondary nature")
                {
                    //secondary nature check
                    await ReplyAsync($"{Context.Message.Author.Mention} Do you have a secondary nature? (y/n): ");
                    response = await NextMessageAsync();

                    if (response.Content.ToLower() != "y")
                    {
                        continue;
                    }
                }

                await ReplyAsync($"{Context.Message.Author.Mention} Enter your {userBioFields[i]}: ");
                response = await NextMessageAsync();

                if (response != null)
                {
                    userBioAnswers[userBioFields[i]] = response.Content;
                }
                else
                {
                    isSuccessful = false;
                    break;
                }      
            }

            if (isSuccessful)
            {
                using (MarketBotContext context = new MarketBotContext())
                {
                    UserBio nameCheck = await context.UserBios.AsQueryable().Where(x => x.User == Context.Message.Author.Username && x.Name.ToLower() == userBioAnswers["name"].ToLower()).FirstOrDefaultAsync();

                    if (nameCheck == null)
                    {
                        await context.UserBios.AddAsync(new UserBio
                        {
                            User = Context.Message.Author.Username,
                            Name = userBioAnswers["name"],
                            Village = userBioAnswers["village"],
                            Rank = userBioAnswers["rank"],
                            Level = userBioAnswers["level"],
                            PrimaryNature = userBioAnswers["primary nature"],
                            SecondaryNature = userBioAnswers["secondary nature"]
                        });

                        await context.SaveChangesAsync();
                        await ReplyAsync($"{Context.Message.Author.Mention} You have added your profile!");
                    }
                    else
                    {
                        await ReplyAsync($"{Context.Message.Author.Mention} You already have a bio with this name.");
                    }
                }   
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithColor(255, 0, 0);
                embed.Description = "You waited too long and the command has timed out.";
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("get-bio", RunMode = RunMode.Async)]
        [Alias("getbio")]
        [Summary("Retrieves all of the bios under your username. Used as nin!getbio")]
        public async Task GetBio()
        {
            var embed = new EmbedBuilder();

            List<UserBio> userBios = new List<UserBio>();

            using (MarketBotContext context = new MarketBotContext())
            {
                userBios = await context.UserBios.AsQueryable().Where(x => x.User == Context.Message.Author.Username).ToListAsync();
            }

            if (userBios.Count != 0)
            {
                foreach(UserBio userBio in userBios)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine($"Name: {userBio.Name}");
                    sb.AppendLine($"Village: {userBio.Village}");
                    sb.AppendLine($"Rank: {userBio.Rank}");
                    sb.AppendLine($"Level: {userBio.Level}");
                    sb.AppendLine($"Primary Nature: {userBio.PrimaryNature}");

                    if (!string.IsNullOrEmpty(userBio.SecondaryNature))
                    {
                        sb.AppendLine($"Secondary Nature: {userBio.SecondaryNature}");
                    }
                    embed.WithColor(125, 35, 117);
                    embed.Description = sb.ToString();
                    await ReplyAsync(null, false, embed.Build());
                }
            }
            else
            {
                embed.WithColor(255, 0, 0);
                embed.Description = $"No bios were found for user: {Context.Message.Author.Mention}";
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("delete-bio", RunMode = RunMode.Async)]
        [Alias("deletebio")]
        [Summary("Deletes the bio with the name given. Used as nin!delete-bio <name>")]
        [Example("nin!delete-bio Dizzy")]
        public async Task DeleteBio([Remainder] string name)
        {
            EmbedBuilder embed = new EmbedBuilder();
            using (MarketBotContext context = new MarketBotContext())
            {
                UserBio userBio = await context.UserBios.AsQueryable().Where(x => x.User == Context.Message.Author.Username && x.Name == name).FirstOrDefaultAsync();

                if (userBio != null)
                {
                    context.UserBios.Remove(userBio);
                    await context.SaveChangesAsync();
                    embed.WithColor(0, 255, 0);
                    embed.Description = $"{Context.Message.Author.Mention} You have successfully removed {name} from your bios.";
                }
                else
                {
                    embed.WithColor(255, 0, 0);
                    embed.Description = $"{Context.Message.Author.Mention} Could not find any bios named {name}.";
                }
            }
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("update-bio", RunMode = RunMode.Async)]
        [Alias("updatebio")]
        [Summary("Updates the bio. You will be prompted to choose what you wish to update. Used as nin!update-bio <name>\nThe options for the prompt are: name, village, rank, level, primary nature, secondary nature")]
        [Example("nin!update-bio Dizzy")]
        public async Task UpdateBio([Remainder] string name)
        {
      
            EmbedBuilder embed = new EmbedBuilder();
            SocketMessage response;
            string field = "";
            string newValue = "";

            using (MarketBotContext context = new MarketBotContext())
            {
                UserBio userBioToUpdate = await context.UserBios.AsQueryable().Where(x => x.User == Context.Message.Author.Username && x.Name == name).FirstOrDefaultAsync();
                if (userBioToUpdate != null)
                {
                    await ReplyAsync($"{Context.Message.Author.Mention} Enter in the field that you wish to edit: ");
                    response = await NextMessageAsync();
                    if (response != null)
                    {
                        field = response.Content;
                        switch (field.ToLower())
                        {
                            case "name":                    
                                await ReplyAsync($"{Context.Message.Author.Mention} New name: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;

                                    UserBio nameCheck = await context.UserBios.AsQueryable().Where(x => x.User == Context.Message.Author.Username && x.Name == newValue).FirstOrDefaultAsync();
                                    if (nameCheck == null)
                                    {
                                        userBioToUpdate.Name = newValue;
                                        context.UserBios.Update(userBioToUpdate);
                                        await context.SaveChangesAsync();
                                        embed.WithColor(0, 255, 0);
                                        embed.Description = "Your update was successful.";
                                    }
                                    else
                                    {
                                        embed.WithColor(255, 0, 0);
                                        embed.Description = $"{Context.Message.Author.Mention} You already have a bio with that name.";
                                    }
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            case "village":
                                await ReplyAsync($"{Context.Message.Author.Mention} New village: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    userBioToUpdate.Village = newValue;
                                    context.Update(userBioToUpdate);
                                    await context.SaveChangesAsync();
                                    embed.WithColor(0, 255, 0);
                                    embed.Description = "Your update was successful.";
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            case "rank":
                                await ReplyAsync($"{Context.Message.Author.Mention} New rank: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    userBioToUpdate.Rank = newValue;
                                    context.Update(userBioToUpdate);
                                    await context.SaveChangesAsync();
                                    embed.WithColor(0, 255, 0);
                                    embed.Description = "Your update was successful.";
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            case "level":
                                await ReplyAsync($"{Context.Message.Author.Mention} New level: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    userBioToUpdate.Level = newValue;
                                    context.Update(userBioToUpdate);
                                    await context.SaveChangesAsync();
                                    embed.WithColor(0, 255, 0);
                                    embed.Description = "Your update was successful.";
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            case "primary nature":
                                await ReplyAsync($"{Context.Message.Author.Mention} New primary nature: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    userBioToUpdate.PrimaryNature = newValue;
                                    context.Update(userBioToUpdate);
                                    await context.SaveChangesAsync();
                                    embed.WithColor(0, 255, 0);
                                    embed.Description = "Your update was successful.";
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            case "secondary nature":
                                await ReplyAsync($"{Context.Message.Author.Mention} New secondary nature: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    userBioToUpdate.SecondaryNature = newValue;
                                    context.Update(userBioToUpdate);
                                    await context.SaveChangesAsync();
                                    embed.WithColor(0, 255, 0);
                                    embed.Description = "Your update was successful.";
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            default:
                                embed.WithColor(255, 0, 0);
                                embed.Description = "The field selection was invalid.";
                                break;
                        }
                    }
                    else
                    {
                        embed.WithColor(255, 0, 0);
                        embed.Description = "You waited too long and the command has timed out.";
                    }
                }
                else
                {
                    embed.WithColor(255, 0, 0);
                    embed.Description = "Could not find a bio with that name.";
                }
            }

            await ReplyAsync(null, false, embed.Build());
        }
    }
}
