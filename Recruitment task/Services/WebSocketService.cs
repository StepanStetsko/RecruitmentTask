using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Recruitment_task.Services
{
    public class WebSocketService : WebSocketBehavior
    {
        private readonly AccessTokenService _tokenService;
        private readonly IConfiguration _configuration;

        private static Dictionary<string, InstrumentData> instrumentMap = new Dictionary<string, InstrumentData>();

        public WebSocketService(AccessTokenService tokenService, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public void StartWebSocketServer()
        {
            Task.Run(() =>
            {
                var server = new WebSocketServer("ws://0.0.0.0:5000");
                server.AddWebSocketService("/ws", () => new WebSocketService(_tokenService, _configuration));
                server.Start();
                Console.WriteLine("WebSocket server started at ws://0.0.0.0:5000");
            });
        }

        protected override async void OnMessage(MessageEventArgs e)
        {
            var instrumentId = Context.QueryString.Get("id");

            if (string.IsNullOrEmpty(instrumentId))
            {
                Console.WriteLine("No instrument ID provided.");
                Context.WebSocket.Close();
                return;
            }

            if (!instrumentMap.ContainsKey(instrumentId))
            {
                var instrumentData = new InstrumentData();

                instrumentMap[instrumentId] = instrumentData;
                instrumentMap[instrumentId].Listeners.Add(this);

                await ConnectToDataProviderAsync(instrumentId, instrumentData);
            }

            Console.WriteLine($"Client subscribed to {instrumentId}");
        }

        private async Task ConnectToDataProviderAsync(string Id, InstrumentData instrumentData)
        {
            var token = await _tokenService.GetTokenAsync();
            var providerUrl = _configuration.GetValue<string>("Settings:ws");
            var dataProviderSocket = new ClientWebSocket();
            instrumentData.DataProviderSocket = dataProviderSocket;

            var uri = new Uri(providerUrl + $"?token={token}");
            await dataProviderSocket.ConnectAsync(uri, CancellationToken.None);

            var subscriptionMessage = new
            {
                type = "l1-subscription",
                id = "1",
                instrumentId = Id,
                provider = "simulation",
                subscribe = true,
                kinds = new[] { "ask", "bid", "last" }
            };
            var message = JsonConvert.SerializeObject(subscriptionMessage);
            var bytes = Encoding.UTF8.GetBytes(message);
            await dataProviderSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine($"Connected to data provider for {Id}");

            _ = ReceiveMessagesFromDataProvider(dataProviderSocket, instrumentData);
        }

        private async Task ReceiveMessagesFromDataProvider(ClientWebSocket dataProviderSocket, InstrumentData instrumentData)
        {
            var buffer = new byte[1024 * 4];
            while (dataProviderSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await dataProviderSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await dataProviderSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }

                var jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received JSON from provider: {jsonString}");

                using JsonDocument document = JsonDocument.Parse(jsonString);

                if (document.RootElement.TryGetProperty("ask", out JsonElement askElement) &&
                    askElement.TryGetProperty("price", out JsonElement priceElement))
                {
                    var price = priceElement.ToString();
                    instrumentData.LastPrice = price;

                    foreach (var listener in instrumentData.Listeners)
                    {
                        listener.Send(price);
                    }
                    Console.WriteLine($"Received price from provider: {price}");
                }

            }
        }

        protected override async void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            var instrumentId = Context.QueryString.Get("id");
            if (!string.IsNullOrEmpty(instrumentId) && instrumentMap.ContainsKey(instrumentId))
            {
                await UnsubscribeFromDataProviderAsync(instrumentId);
            }
        }

        private async Task UnsubscribeFromDataProviderAsync(string instrumentId)
        {
            if (!instrumentMap.TryGetValue(instrumentId, out var instrumentData)) return;

            instrumentData.Listeners.Remove(this);

            if (instrumentData.Listeners.Count > 0) return;

            if (instrumentData.DataProviderSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var unsubscriptionMessage = new
                {
                    type = "l1-subscription",
                    id = "1",
                    instrumentId = instrumentId,
                    provider = "simulation",
                    subscribe = false,
                    kinds = new[] { "ask", "bid", "last" }
                };

                var message = JsonConvert.SerializeObject(unsubscriptionMessage);
                var bytes = Encoding.UTF8.GetBytes(message);

                await instrumentData.DataProviderSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);               
            }

            instrumentData.DataProviderSocket.Dispose();
            instrumentMap.Remove(instrumentId);
        }

        public class InstrumentData
        {
            public string LastPrice { get; set; }
            public List<WebSocketService> Listeners { get; set; } = new List<WebSocketService>();
            public ClientWebSocket DataProviderSocket { get; set; }
        }
    }
}