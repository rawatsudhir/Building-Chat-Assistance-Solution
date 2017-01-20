using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Net.Http;

namespace DataTemplateSelector
{

    public class QnAMakerResult
    {
        /// <summary>
        /// The top answer found in the QnA Service.
        /// </summary>
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        /// <summary>
        /// The score in range [0, 100] corresponding to the top answer found in the QnA    Service.
        /// </summary>
        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }


    public class MainPageViewModel : BaseViewModel
    {

        private ObservableCollection<MessageViewModel> messagesList;

        public ObservableCollection<MessageViewModel> Messages
        {
            get { return messagesList; }
            set { messagesList = value; RaisePropertyChanged(); }
        }

        private string outgoingText;

        public string OutGoingText
        {
            get { return outgoingText; }
            set { outgoingText = value; RaisePropertyChanged(); }
        }

        public ICommand SendCommand { get; set; }


        public MainPageViewModel()
        {
            // Initialize with default values

            Messages = new ObservableCollection<MessageViewModel>
            {
                new MessageViewModel { Text = "Welcome to Support!", IsIncoming = true, MessagDateTime = DateTime.Now.AddMinutes(-25)}
            };
            OutGoingText = null;
            SendCommand = new Command(async () =>
            {
                var goingtext = OutGoingText;
                Messages.Add(new MessageViewModel { Text = OutGoingText, IsIncoming = false, MessagDateTime = DateTime.Now });
                OutGoingText = null;
                //Make requestawait  to the server
                var response = await PostQueryAsync(goingtext);
                Messages.Add(new MessageViewModel { Text = response.Answer.ToString(), IsIncoming = true, MessagDateTime = DateTime.Now });

            });
        }
        //Make Async function 
        //
        public async Task<QnAMakerResult> PostQueryAsync(string customerquery)
        {
            string responseString = string.Empty;
            var query = customerquery; //User Query
            var knowledgebaseId = "XXXXXXXXXXXXXXXXX"; // Use knowledge base id created.
            var qnamakerSubscriptionKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXX"; //Use subscription key assigned to you.
            //Build the URI
            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");
            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";
            //Set the encoding to UTF8
            var content = new StringContent(postBody, Encoding.UTF8, "application/json");
            //Send the POST request
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
            HttpResponseMessage response = await client.PostAsync(builder.ToString(), content);
            if (response.IsSuccessStatusCode)
            {
                responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
            }
            return null;
        }
    }


}
