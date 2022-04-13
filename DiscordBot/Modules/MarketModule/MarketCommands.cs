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

namespace DiscordBot.Modules.MarketModule
{
    public class MarketCommands : InteractiveBase
    {
        [Command("view-market")]
        [Alias("viewmarket", "market")]
        [Summary("View the market to see the current offers.")]
        public async Task ViewMarket()
        {
            EmbedBuilder embed = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();
            List<MarketItem> marketItems;
            

            using(MarketBotContext context = new MarketBotContext())
            {
                marketItems = await context.MarketItems.AsQueryable().ToListAsync();
            }

            if (marketItems.Count != 0)
            {
                sb.AppendLine("```Offer     Quantity     Item                         Price          User");
                sb.AppendLine("-----------------------------------------------------------------------");
                foreach (MarketItem marketItem in marketItems)
                {
                    sb.AppendFormat("{0, -10}{1, -13}{2, -29}{3, -15}{4}", marketItem.Offer, marketItem.Quantity, marketItem.Name, marketItem.Price,marketItem.User);
                    sb.AppendLine();
                }
                sb.Append("```");
                await ReplyAsync(sb.ToString());
            }
            else
            {
                embed.WithColor(255, 0, 0);
                embed.Description = "No items are currently on the Market.";
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("wtb-item")]
        [Alias("wtbitem", "buyitem", "wtb")]
        [Summary("Places a WTB offer for the specified item, price and quantity onto the market. Used as nin!wtb-item  <price> <quantity> <item name>")]
        [Example("nin!wtb-item 1000 500 scorpion tails")]
        public async Task WtbItem(int price, int quantity, [Remainder] string name)
        {
            EmbedBuilder embed = new EmbedBuilder();

            MarketItem marketItem = new MarketItem();
            marketItem.User = Context.Message.Author.Username;
            marketItem.Offer = "WTB";
            marketItem.Quantity = quantity.ToString();
            marketItem.Price = price.ToString();
            marketItem.Name = name.ToLower();

            using (MarketBotContext context = new MarketBotContext())
            {
                MarketItem itemCheck = await context.MarketItems.AsQueryable().Where(x => x.Name == marketItem.Name && x.User == marketItem.User).FirstOrDefaultAsync();

                if (itemCheck == null)
                {
                    await context.MarketItems.AddAsync(marketItem);
                    await context.SaveChangesAsync();
                    embed.WithColor(0, 255, 0);
                    embed.Description = $"{Context.Message.Author.Mention} Successfully added buy offer to the market.";
                }
                else
                {
                    embed.WithColor(255, 0, 0);
                    embed.Description = $"{Context.Message.Author.Mention} You already have an offer in the market for that item. If you need to change it, use nin!update-item";
                }
            }
    
            await ReplyAsync(null, false, embed.Build());
            
        }

        [Command("wts-item")]
        [Alias("wtsitem", "sellitem", "wts")]
        [Summary("Places a WTS offer for the specified item, price and quantity onto the market. Used as nin!wts-item <price> <quanity> <item name>")]
        [Example("nin!wts-item 1000 500 scorpion tails")]
        public async Task WtsItem(int price, int quantity, [Remainder] string name)
        {
            EmbedBuilder embed = new EmbedBuilder();

            MarketItem marketItem = new MarketItem();
            marketItem.User = Context.Message.Author.Username;
            marketItem.Offer = "WTS";
            marketItem.Quantity = quantity.ToString();
            marketItem.Price = price.ToString();
            marketItem.Name = name.ToLower();

            using (MarketBotContext context = new MarketBotContext())
            {
                MarketItem itemCheck = await context.MarketItems.AsQueryable().Where(x => x.Name == marketItem.Name && x.User == marketItem.User).FirstOrDefaultAsync();

                if (itemCheck == null)
                {
                    await context.MarketItems.AddAsync(marketItem);
                    await context.SaveChangesAsync();
                    embed.WithColor(0, 255, 0);
                    embed.Description = $"{Context.Message.Author.Mention} Successfully added sell offer to the market.";
                }
                else
                {
                    embed.WithColor(255, 0, 0);
                    embed.Description = $"{Context.Message.Author.Mention} You already have an offer in the market for that item. If you need to change it, use nin!update-item";
                }
            }
            
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("request-item")]
        [Alias("requestitem")]
        [Summary("Pings users with a sell offer for a particular item. Used as nin!request-item <name>")]
        [Example("nin!request-item scorpion tail")]
        public async Task RequestItem([Remainder] string name)
        {
            List<MarketItem> marketItems = new List<MarketItem>();
            List<SocketGuildUser> usersWithItem = new List<SocketGuildUser>();
            EmbedBuilder embed = new EmbedBuilder();
            StringBuilder sb = new StringBuilder();

            using (MarketBotContext context = new MarketBotContext())
            {
                marketItems = await context.MarketItems.AsQueryable().Where(x => x.Name == name).ToListAsync();
            }
            
            if (marketItems.Count > 0)
            {
                foreach (MarketItem marketItem in marketItems)
                {
                    SocketGuildUser user = Context.Guild.Users.Where(x => x.Username == marketItem.User).FirstOrDefault();
                    if (user != null)
                    {
                        usersWithItem.Add(user);
                    }
                }

                if (usersWithItem.Count > 0)
                {
                    string separator = "";
                    foreach (SocketGuildUser user in usersWithItem)
                    {
                        sb.Append($"{separator}{user.Mention}");
                        separator = " ,";
                    }
                    sb.AppendLine();
                    sb.AppendLine($"{Context.Message.Author.Username} is looking for {name}!");
                    await ReplyAsync(sb.ToString());
                }
                else
                {
                    embed.Description = "The user(s) that are selling that item are no longer in this channel.";
                    embed.WithColor(255, 0, 0);
                    await ReplyAsync(null, false, embed.Build());
                }
            }
            else
            {
                embed.Description = "Nobody is currently selling that item.";
                embed.WithColor(255, 0, 0);
                await ReplyAsync(null, false, embed.Build());
            }

            
        }

        [Command("update-item")]
        [Alias("updateitem")]
        [Summary("Updates quantity, price or name for a particular item. It will prompt you to choose which field and the new value when you use the command. Used as nin!update-item <name>")]
        [Example("nin!update-item scorpion tail")]
        public async Task UpdateItem([Remainder] string name)
        {
            EmbedBuilder embed = new EmbedBuilder();
            SocketMessage response;
            string field = "";
            string newValue = "";

            using (MarketBotContext context = new MarketBotContext())
            {
                MarketItem itemToUpdate = await context.MarketItems.AsQueryable().Where(x => x.User == Context.Message.Author.Username && x.Name == name).FirstOrDefaultAsync();
                if (itemToUpdate != null)
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

                                    MarketItem itemCheck = await context.MarketItems.AsQueryable().Where(x => x.User == Context.Message.Author.Username && x.Name == newValue).FirstOrDefaultAsync();
                                    if (itemCheck == null)
                                    {
                                        itemToUpdate.Name = newValue;
                                        context.MarketItems.Update(itemToUpdate);
                                        await context.SaveChangesAsync();
                                        embed.WithColor(0, 255, 0);
                                        embed.Description = "Your update was successful.";
                                    }
                                    else
                                    {
                                        embed.WithColor(255, 0, 0);
                                        embed.Description = $"{Context.Message.Author.Mention} You already have an item with that name.";
                                    }
                                }
                                else
                                {
                                    embed.WithColor(255, 0, 0);
                                    embed.Description = "You waited too long and the command has timed out.";
                                }
                                break;
                            case "price":
                                await ReplyAsync($"{Context.Message.Author.Mention} New price: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    itemToUpdate.Price = newValue;
                                    context.Update(itemToUpdate);
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
                            case "quantity":
                                await ReplyAsync($"{Context.Message.Author.Mention} New quantity: ");
                                response = await NextMessageAsync();
                                if (response != null)
                                {
                                    newValue = response.Content;
                                    itemToUpdate.Quantity = newValue;
                                    context.Update(itemToUpdate.Quantity);
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
                    embed.Description = "Could not find an item with that name.";
                }
            }

            await ReplyAsync(null, false, embed.Build());
        }

        [Command("delete-item")]
        [Alias("deleteitem")]
        [Summary("Deletes an item that you have added to the market. Used as nin!delete-item <name>")]
        [Example("nin!delete-item scorpion tail")]
        public async Task DeleteItem([Remainder] string name)
        {
            MarketItem marketItem = null;
            EmbedBuilder embed = new EmbedBuilder();
            using (MarketBotContext context = new MarketBotContext())
            {
                marketItem = await context.MarketItems.AsQueryable().Where(x => x.Name == name.ToLower() && x.User == Context.Message.Author.Username).FirstOrDefaultAsync();

                if (marketItem != null)
                {
                    context.MarketItems.Remove(marketItem);
                    await context.SaveChangesAsync();
                    embed.WithColor(0, 255, 0);
                    embed.Description = "Item successfully deleted.";

                }
                else
                {
                    embed.WithColor(255, 0, 0);
                    embed.Description = $"{name} was not found in the market for user: {Context.Message.Author.Username}";
                }
            }

            await ReplyAsync(null, false, embed.Build());
            
        }
    }
}
