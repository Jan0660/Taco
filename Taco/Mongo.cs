using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Revolt.Channels;

namespace Taco
{
    public static class Mongo
    {
        public static MongoClient Client;
        public static IMongoDatabase Database;
        public static IMongoCollection<BsonDocument> UserCollection;
        public static IMongoCollection<BsonDocument> ServerCollection;
        public static IMongoCollection<BsonDocument> GroupCollection;

        public static async Task Connect()
        {
            Client = new MongoClient(Program.Config.MongoUrl);
            Database = Client.GetDatabase(Program.Config.DatabaseName);
            UserCollection = Database.GetCollection<BsonDocument>("users");
            ServerCollection = Database.GetCollection<BsonDocument>("servers");
            GroupCollection = Database.GetCollection<BsonDocument>("groups");
        }

        public static UserData GetUserData(string userId)
        {
            var findRes = UserCollection
                .Find(new BsonDocument("UserId", userId)).FirstOrDefault();
            return findRes == null ? null : BsonSerializer.Deserialize<UserData>(findRes);
        }

        public static ServerData GetServerData(string userId)
        {
            var findRes = ServerCollection
                .Find(new BsonDocument("ServerId", userId)).FirstOrDefault();
            return findRes == null ? null : BsonSerializer.Deserialize<ServerData>(findRes);
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

        public static GroupData GetGroupData(string groupId)
        {
            var findRes = ServerCollection
                .Find(new BsonDocument("GroupId", groupId)).FirstOrDefault();
            return findRes == null ? null : BsonSerializer.Deserialize<GroupData>(findRes);
        }

        public static GroupData GetOrCreateGroupData(string groupId)
        {
            var data = GetGroupData(groupId);
            if (data != null)
                return data;
            data = new GroupData(groupId);
            UserCollection.InsertOne(data.ToBsonDocument());
            return data;
        }

        public static ServerData GetOrCreateServerData(string serverId)
        {
            var data = GetServerData(serverId);
            if (data != null)
                return data;
            data = new ServerData(serverId);
            ServerCollection.InsertOne(data.ToBsonDocument());
            return data;
        }

        public static async Task<long> Ping()
        {
            var stopwatch = Stopwatch.StartNew();
            await Database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            return stopwatch.ElapsedMilliseconds;
        }

        public static CommunityData GetOrCreateCommunityData(string id, CommunityType type)
        {
            var data = GetCommunityData(id, type);
            if (data != null)
                return data;
            data = type switch
            {
                CommunityType.Group => GetOrCreateGroupData(id),
                CommunityType.Server => GetOrCreateServerData(id)
            };
            return data;
        }

        public static CommunityData GetCommunityData(string id, CommunityType type)
        {
            BsonDocument bson = type switch
            {
                CommunityType.Group => GroupCollection
                    .Find(new BsonDocument("GroupId", id)).FirstOrDefault(),
                CommunityType.Server => ServerCollection
                    .Find(new BsonDocument("ServerId", id)).FirstOrDefault(),
                _ => throw new Exception("Code oopsie...")
            };
            return bson == null ? null : BsonSerializer.Deserialize<CommunityData>(bson);
        }
    }

    public enum PermissionLevel : sbyte
    {
        BlacklistSilent = -2,
        Blacklist = -1,
        None = 0,
        Developer = 100
    }

    [BsonIgnoreExtraElements]
    public class UserData
    {
        public string UserId;
        public PermissionLevel PermissionLevel;
        public string BlacklistedMessage;
        public Dictionary<string, string> SavedAttachments = new();

        [BsonConstructor]
        private UserData()
        {
        }

        public UserData(string userId)
        {
            UserId = userId;
        }

        public Task UpdateAsync()
            => Mongo.UserCollection.FindOneAndReplaceAsync(new BsonDocument("UserId", UserId),
                this.ToBsonDocument());
    }

    [BsonIgnoreExtraElements]
    public class ServerData : CommunityData
    {
        public string ServerId;
        [BsonIgnore] public TextChannel LogChannel => (TextChannel)Program.Client.Channels.Get(LogChannelId);
        public string LogChannelId;
        public List<string> ModRoles = new();

        [BsonConstructor]
        private ServerData()
        {
        }

        public ServerData(string serverId) => (ServerId) = (serverId);

        public Task UpdateAsync()
            => Mongo.ServerCollection.FindOneAndReplaceAsync(new BsonDocument("ServerId", ServerId),
                this.ToBsonDocument());
    }

    public enum CommunityType : byte
    {
        Group,
        Server
    }

    /// <summary>
    /// Settings storage for groups or servers.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class CommunityData
    {
        public bool AllowSnipe { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public string CustomPrefix { get; set; }

        public CommunityData()
        {
            Tags = new();
        }
    }

    [BsonIgnoreExtraElements]
    public class GroupData : CommunityData
    {
        public string GroupId;

        [BsonConstructor]
        private GroupData()
        {
        }

        public GroupData(string id) => GroupId = id;

        public Task UpdateAsync()
            => Mongo.ServerCollection.FindOneAndReplaceAsync(new BsonDocument("GroupId", GroupId),
                this.ToBsonDocument());
    }
}