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
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
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
                if (intent == "Greeting")
                {
                    Activity reply = activity.CreateReply($"Hello. What is your name?");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (intent == "Transfer")
                {
                    Activity reply = activity.CreateReply($"Hello. You want to transfer.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (intent == "GetExchangeRate")
                {
                    Activity reply = activity.CreateReply($"Hello. You want to get exchange rate.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    Activity reply = activity.CreateReply($"Hello. You sent {activity.Text} which was {length} characters. Your intent is: {intent}");
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