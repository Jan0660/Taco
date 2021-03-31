using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using RevoltApi;

namespace RevoltBot
{
    public static class Mongo
    {
        public static MongoClient Client;
        public static IMongoDatabase Database;
        public static IMongoCollection<BsonDocument> UserCollection;

        public static async Task Connect()
        {
            Client = new MongoClient(Program.Config.MongoUrl);
            Database = Client.GetDatabase(Program.Config.DatabaseName);
            UserCollection = Database.GetCollection<BsonDocument>("users");
        }

        public static UserData GetUserData(string userId)
        {
            var findRes = UserCollection
                .Find(new BsonDocument("UserId", userId)).FirstOrDefault();
            return findRes == null ? null : BsonSerializer.Deserialize<UserData>(findRes);
        }

        public static UserData GetOrCreateUserData(string userId)
        {
            var data = GetUserData(userId);
            if (data != null)
                return data;
            data = new UserData(userId);
            UserCollection.InsertOne(data.ToBsonDocument());
            return data;
        }
    }

    [BsonIgnoreExtraElements]
    public class UserData
    {
        public string UserId;

        public UserData(string userId)
        {
            UserId = userId;
        }
    }
}