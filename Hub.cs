using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using static JwtService;
using System;

namespace TRPO
{
    public class ChatHub : Hub
    {
        private readonly IMongoCollection<Message> _messages;
        private readonly IMongoCollection<Chatroom> _chatRooms;
        private readonly IMongoCollection<User> _users;

        private readonly JwtService _jwtService;

        private readonly IMongoDatabase database;
        Dictionary<string, string> myDictionary = new Dictionary<string, string>();

        public ChatHub()
        {
            _jwtService = new JwtService("63e8781bc55321d2dca81a6fa32c53f0e6038dee9700be2c15c982672cdc69abe7733a2760e3d0a44104d68d4f2add1b1eca308d15e6ca5ed3fbd7f68a439170");

            var client = new MongoClient("mongodb://localhost:27017");

            var database = client.GetDatabase("TRPO_chats");
            _messages = database.GetCollection<Message>("Messages");
            _chatRooms = database.GetCollection<Chatroom>("ChatRooms");
            _users = database.GetCollection<User>("Users");
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task CreateUser(string name){
            var httpContext = Context.GetHttpContext();
            httpContext?.Response.Cookies.Append("IdToken", "cookieValue");

        }

        public async Task CreateChatroom(string name){
            var chat = new Chatroom {
                Name = name,
                UserIds = new[] {
                    new ObjectId()
                }
            };
        }

        public override Task? OnConnectedAsync()
        {
            var token = Context.GetHttpContext().Request.Query["token"]; 
            var principal = _jwtService.ValidateJwtToken(token);

            if (principal == null)
            {

                Context.Abort();
                return null;
            }
            Console.WriteLine(Context.ConnectionId);
            Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {

            return base.OnDisconnectedAsync(exception);
        }
    }
}