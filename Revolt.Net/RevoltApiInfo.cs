using Newtonsoft.Json;

namespace Revolt
{
    public class RevoltApiInfo
    {
        [JsonProperty("revolt")] public string Version { get; private set; }
        [JsonProperty("features")] public RevoltApiInfoFeatures Features { get; private set; }
        [JsonProperty("ws")] public string WebsocketUrl { get; private set; }
        [JsonProperty("vapid")] public string Vapid { get; private set; }
    }

    public class RevoltApiInfoFeatures
    {
        [JsonProperty("registration")] public bool Registration { get; private set; }
        [JsonProperty("captcha")] public RevoltApiCaptchaFeature Captcha { get; private set; }
        [JsonProperty("email")] public bool Email { get; private set; }
        [JsonProperty("invite_only")] public bool InviteOnly { get; private set; }
        [JsonProperty("autumn")] public RevoltApiAutumnFeature Autumn { get; private set; }
        [JsonProperty("voso")] public RevoltApiVortexFeature Vortex { get; private set; }
    }

    public class RevoltApiCaptchaFeature
    {
        [JsonProperty("enabled")] public bool Enabled { get; private set; }
        [JsonProperty("key")] public string Key { get; private set; }
    }

    public class RevoltApiAutumnFeature
    {
        [JsonProperty("enabled")] public bool Enabled { get; private set; }
        [JsonProperty("url")] public string Url { get; private set; }
    }

    public class RevoltApiVortexFeature
    {
        [JsonProperty("enabled")] public bool Enabled { get; private set; }
        [JsonProperty("url")] public string Url { get; private set; }
        [JsonProperty("ws")] public string WebsocketUrl { get; private set; }
    }

    public class VortexInformation
    {
        [JsonProperty("voso")] public string Version { get; private set; }
        [JsonProperty("ws")] public string WebsocketUrl { get; private set; }
        [JsonProperty("features")] public VortexFeatures Features { get; private set; }
    }

    public class VortexFeatures
    {
        [JsonProperty("rtp")] public bool Rtp { get; private set; }
    }

    public class AutumnInformation
    {
        [JsonProperty("autumn")] public string Version { get; private set; }
        [JsonProperty("tags")] public AutumnInfoTags Tags { get; private set; }
        [JsonProperty("jpeg_quality")] public int JpegQuality { get; private set; }
    }

    public class AutumnInfoTags
    {
        [JsonProperty("avatars")] public AutumnInfoTag Avatars { get; private set; }
        [JsonProperty("icons")] public AutumnInfoTag Icons { get; private set; }
        [JsonProperty("banners")] public AutumnInfoTag Banners { get; private set; }
        [JsonProperty("backgrounds")] public AutumnInfoTag Backgrounds { get; private set; }
        [JsonProperty("attachments")] public AutumnInfoTag Attachments { get; private set; }
    }

    public class AutumnInfoTag
    {
        /// <summary>
        /// Maximum size in bytes.
        /// </summary>
        [JsonProperty("max_size")]
        public ulong MaxSize { get; private set; }

        [JsonProperty("enabled")] public bool Enabled { get; private set; }

        [JsonProperty("serve_if_field_present")]
        public string ServeIfFieldPresent { get; private set; }

        [JsonProperty("restrict_content_type")]
        public string RestrictContentType { get; private set; }
    }
}