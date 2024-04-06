using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using MongoDB.Bson;
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
        Dictionary<string, string> usersDict = new Dictionary<string, string>();

        private (string, string) CreateUserDB(string name){
            var newUser = new User
                {
                    Name = name
                };
            _users.InsertOneAsync(newUser);
            return (newUser.Id ,_jwtService.GenerateJwtToken(newUser.Id));
        }

        public ChatHub()
        {
            _jwtService = new JwtService("63e8781bc55321d2dca81a6fa32c53f0e6038dee9700be2c15c982672cdc69abe7733a2760e3d0a44104d68d4f2add1b1eca308d15e6ca5ed3fbd7f68a439170");

            var client = new MongoClient("mongodb://localhost:27017");

            var database = client.GetDatabase("TRPO_chats");
            _messages = database.GetCollection<Message>("Messages");
            _chatRooms = database.GetCollection<Chatroom>("ChatRooms");
            _users = database.GetCollection<User>("Users");
        }

        public async Task GetMessages(string token, string chatroomId){
            var filter = Builders<Message>.Filter.Eq("chatId", ObjectId.Parse(chatroomId));
            var messages = await _messages.Find(filter).ToListAsync();
            await Clients.Client(Context.ConnectionId).SendAsync("GetMessages", messages);
        }

        public async Task SendMessage(string token, string chatroomId, string message)
        {
            var userId = _jwtService.ValidateJwtToken(token);
            if(userId != null){
                var messageModel = new Message{
                    UserId = ObjectId.Parse(userId),
                    ChatId = ObjectId.Parse(chatroomId),
                    Content = message
                };
                await _messages.InsertOneAsync(messageModel);
                await Clients.Group(chatroomId).SendAsync("ReceiveMessage", userId, message);
            }
        }

        public async Task CreateUser(string name){
            var httpContext = Context.GetHttpContext();

            var userData = CreateUserDB(name);
            var userId = userData.Item1;
            var userToken = userData.Item2;

            usersDict[userId] = Context.ConnectionId;
            await Clients.Client(Context.ConnectionId).SendAsync("CreateUser", name, userId, userToken);
        }

        public async Task ConnectUser(string token, string chatroom){
            var userId = _jwtService.ValidateJwtToken(token);
            if(userId != null){
                usersDict[userId] = Context.ConnectionId;
                if(chatroom != null) await Groups.AddToGroupAsync(Context.ConnectionId, chatroom);
            }
        }

        public async Task CreateChatroom(string token, string name){
            var userId = _jwtService.ValidateJwtToken(token);
            Console.WriteLine(token);
            if(userId != null){
                Console.WriteLine(userId);
                var chat = new Chatroom {
                    Name = name,
                    UserIds = new[] {
                        ObjectId.Parse(userId)
                    }
                };
                await _chatRooms.InsertOneAsync(chat);
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id);
                await Clients.Client(Context.ConnectionId).SendAsync("JoinChatroom", name, chat.Id);
            }
        }

        public async Task JoinChatroom(string token, string charoomId)
        {
            var userId = _jwtService.ValidateJwtToken(token);
            if (userId != null)
            {
                var filter = Builders<Chatroom>.Filter.Eq("_id", ObjectId.Parse(charoomId));
                var chatroom = await _chatRooms.Find(filter).FirstOrDefaultAsync();
                if (chatroom == null)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("Error", "Chat not found");
                }
                else
                {
                    if (!chatroom.UserIds.Any(id => id == ObjectId.Parse(userId)))
                    {
                        var update = Builders<Chatroom>.Update.Push("userIds", userId);
                        await _chatRooms.UpdateOneAsync(filter, update);
                    }

                    await Groups.AddToGroupAsync(Context.ConnectionId, charoomId);
                    await Clients.Client(Context.ConnectionId).SendAsync("JoinChatroom", chatroom.Name, charoomId);
                }
            }
        }

        public override Task OnConnectedAsync()
        {
            /*var token = Context.GetHttpContext().Request.Query["token"]; 
            var principal = _jwtService.ValidateJwtToken(token);

            if (principal == null)
            {

                Context.Abort();
                return Task.CompletedTask;
            }*/
            Console.WriteLine(Context.ConnectionId);
            //Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var keyToRemove = usersDict.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (keyToRemove != null) usersDict.Remove(keyToRemove);

            return base.OnDisconnectedAsync(exception);
        }
    }
}