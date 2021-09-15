using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Revolt.Channels;

namespace Revolt
{
    public partial class RevoltClient
    {
        internal Channel _deserializeChannel(string json)
        {
            var obj = JObject.Parse(json);
            return _deserializeChannel(obj);
        }

        internal Channel _deserializeChannel(JObject obj)
        {
            Channel channel;
            switch (obj.Value<string>("channel_type"))
            {
                case "Group":
                    channel = obj.ToObject<GroupChannel>();
                    break;
                case "DirectMessage":
                    channel = obj.ToObject<DirectMessageChannel>();
                    break;
                case "SavedMessages":
                    channel = obj.ToObject<SavedMessagesChannel>();
                    break;
                case "TextChannel":
                    channel = obj.ToObject<TextChannel>(new JsonSerializer()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    break;
                default:
                    channel = obj.ToObject<Channel>();
                    break;
            }

            channel!.Client = this;
            return channel;
        }

        internal T _deserialize<T>(string json) where T : RevoltObject
        {
            T obj = JsonConvert.DeserializeObject<T>(json);
            obj.Client = this;
            return obj;
        }

        internal Task<T> _requestAsync<T>(string url, Method method = Method.GET, string? body = null)
        {
            var req = new RestRequest(url, method);
            if (body != null)
                req.AddJsonBody(body);
            return _requestAsync<T>(req);
        }

        internal Task _requestAsync(string url, Method method = Method.GET, string? body = null)
        {
            var req = new RestRequest(url, method);
            if (body != null)
                req.AddJsonBody(body);
            return _requestAsync(req);
        }

        internal Task _requestAsync(RestRequest request)
            => _restClient.ExecuteAsync(request);

        internal async Task<T> _requestAsync<T>(RestRequest request)
        {
            var res = await _restClient.ExecuteAsync(request);
            T val = default;
            try
            {
                if (val is Channel)
                    val = (T)(object)_deserializeChannel(res.Content);
                else
                    val = JsonConvert.DeserializeObject<T>(res.Content, _jsonSerializerSettings);
            }
            // todo: catch json exception type JsonReaderException
            catch (Exception exception)
            {
                var err = JsonConvert.DeserializeObject<RevoltError>(res.Content);
                if (res.StatusCode == HttpStatusCode.OK)
                    throw new Exception(
                        "Internal exception deserializing JSON response from Revolt, please check your Revolt.Net version.",
                        exception);
                throw new RevoltException(err!, res);
            }

            switch (val)
            {
                case User user:
                    user.AttachClient(this);
                    break;
                case RevoltObject revoltObject:
                    revoltObject.Client = this;
                    break;
                case RevoltObject[] revoltObjects:
                {
                    foreach (var obj in revoltObjects)
                        obj.Client = this;
                    break;
                }
            }

            return val;
        }
    }
}