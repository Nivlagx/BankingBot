using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using BankingBot.Models;
using BankingBot.DataModels;
using System.Collections.Generic;

namespace BankingBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var userMessage = activity.Text;

                var userName = "";

                string endOutput = "Hello";

                bool SentGreeting = false;

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                // State Client
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
               
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // LUIS
                HttpClient client = new HttpClient();
                var luisURL = "https://api.projectoxford.ai/luis/v2.0/apps/a118c892-898f-41b2-abf4-904c826613cf?subscription-key=3c04249ae37341989e1d95c41d0cbf24&q=";
                string x = await client.GetStringAsync(new Uri(luisURL + activity.Text + "&verbose=true"));

                luis.RootObject rootObject;
                rootObject = JsonConvert.DeserializeObject<luis.RootObject>(x);

                string intent = rootObject.topScoringIntent.intent;
                
                // return our reply to the user
                if (intent == "GetBalance")
                {
                    List<AccountsTable> accounts = await AzureManager.AzureManagerInstance.GetBalance();
                    List<string> transaction = new List<string>();
                    int i = -1;
                    foreach (AccountsTable t in accounts)
                    {
                        transaction.Add("[" + t.Date + "] Cheque " + t.Cheque + ", Savings " + t.Savings + ", Credit " + t.Credit + "\n\n");
                        i += 1;
                    }
                    Activity reply = activity.CreateReply(transaction[i]);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                else if (intent == "Greeting")
                {
                    // calculate something for us to return
                    if (userData.GetProperty<bool>("SentGreeting"))
                    {
                        endOutput = "Hello again";
                    }
                    else
                    {
                        endOutput = "Hello. What is your name?";
                        userData.SetProperty<bool>("SentGreeting", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        userName = rootObject.entities[0].entity;
                    }

                    // return our reply to the user
                    Activity infoReply = activity.CreateReply(endOutput);

                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                }
                else if (intent == "Transfer")
                {
                    string entityMoney = "";
                    double money = 0;
                    string entityToAccount = "";
                    string entityFromAccount = "";
                    
                    // JSON check
                    if (rootObject.entities[2].type == "number")
                    {
                        // convert string to double
                        entityMoney = rootObject.entities[2].entity;
                        money = Convert.ToDouble(entityMoney);
                    }

                    if (rootObject.entities[0].type == "Account::ToAccount")
                    {
                        entityToAccount = rootObject.entities[0].entity;
                    }
                    
                    if (rootObject.entities[1].type == "Account::FromAccount")
                    {
                        entityFromAccount = rootObject.entities[1].entity;
                    }

                    // From Cheque account
                    if (entityFromAccount.ToLower().Equals("cheque"))
                    {
                        if (entityToAccount.ToLower().Equals("savings"))
                        {
                            AccountsTable account = new AccountsTable()
                            {
                                Cheque = 0 - money,
                                Savings = money,
                                Date = DateTime.Now
                            };
                            await AzureManager.AzureManagerInstance.AddTimeline(account);
                            endOutput = "New transaction made [" + account.Date + "] $" + money + " From Cheque to Savings.";
                        }
                        else if (entityToAccount.ToLower().Equals("credit"))
                        {
                            AccountsTable account = new AccountsTable()
                            {
                                Cheque = 0 - money,
                                Credit = money,
                                Date = DateTime.Now
                            };
                            await AzureManager.AzureManagerInstance.AddTimeline(account);
                            endOutput = "New transaction made [" + account.Date + "] $" + money + " From Cheque to Credit.";
                        }
                        else
                        {
                            endOutput = "Error";
                        }
                    }

                    // From Savings account
                    else if (entityFromAccount.ToLower().Equals("savings"))
                    {
                        if (entityToAccount.ToLower().Equals("cheque"))
                        {
                            AccountsTable account = new AccountsTable()
                            {
                                Savings = 0 - money,
                                Cheque = money,
                                Date = DateTime.Now
                            };
                            await AzureManager.AzureManagerInstance.AddTimeline(account);
                            endOutput = "New transaction made [" + account.Date + "] $" + money + " From Savings to Cheque.";
                        }
                        else if (entityToAccount.ToLower().Equals("credit"))
                        {
                            AccountsTable account = new AccountsTable()
                            {
                                Savings = 0 - money,
                                Credit = money,
                                Date = DateTime.Now
                            };
                            await AzureManager.AzureManagerInstance.AddTimeline(account);
                            endOutput = "New transaction made [" + account.Date + "] $" + money + " From Savings to Credit.";
                        }
                        else
                        {
                            endOutput = "Error";
                        }
                    }

                    // From Credit account
                    else if (entityFromAccount.ToLower().Equals("credit"))
                    {
                        if (entityToAccount.ToLower().Equals("savings"))
                        {
                            AccountsTable account = new AccountsTable()
                            {
                                Credit = 0 - money,
                                Savings = money,
                                Date = DateTime.Now
                            };
                            await AzureManager.AzureManagerInstance.AddTimeline(account);
                            endOutput = "New transaction made [" + account.Date + "] $" + money + " From Credit to Savings.";
                        }
                        else if (entityToAccount.ToLower().Equals("cheque"))
                        {
                            AccountsTable account = new AccountsTable()
                            {
                                Credit = 0 - money,
                                Cheque = money,
                                Date = DateTime.Now
                            };
                            await AzureManager.AzureManagerInstance.AddTimeline(account);
                            endOutput = "New transaction made [" + account.Date + "] $" + money + " From Credit to Cheque.";
                        }
                        else
                        {
                            endOutput = "Error";
                        }
                    }
                    else
                    {
                        endOutput = "Please rephrase";
                    }
                    Activity reply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                else if (intent == "GetExchangeRate")
                {
                    Activity reply = activity.CreateReply($"Hello. You want to get exchange rate.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (intent == "None")
                {
                    endOutput = "User data cleared";
                    Activity reply = activity.CreateReply(endOutput);
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    SentGreeting = false;
                }
                else
                {
                    endOutput = "Please rephrase";
                    Activity reply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}