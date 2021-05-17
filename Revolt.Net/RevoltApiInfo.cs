using Newtonsoft.Json;

namespace Revolt
{
    public class RevoltApiInfo
    {
        [JsonProperty("revolt")] public string Version;
        [JsonProperty("features")] public RevoltApiInfoFeatures Features;
        [JsonProperty("ws")] public string WebsocketUrl;
        [JsonProperty("vapid")] public string Vapid;
    }

    public class RevoltApiInfoFeatures
    {
        [JsonProperty("registration")] public bool Registration;
        [JsonProperty("captcha")] public RevoltApiCaptchaFeature Captcha;
        [JsonProperty("email")] public bool Email;
        [JsonProperty("invite_only")] public bool InviteOnly;
        [JsonProperty("autumn")] public RevoltApiAutumnFeature Autumn;
        [JsonProperty("vortex")] public RevoltApiVortexFeature Vortex;
    }

    public class RevoltApiCaptchaFeature
    {
        [JsonProperty("enabled")] public bool Enabled;
        [JsonProperty("key")] public string Key;
    }

    public class RevoltApiAutumnFeature
    {
        [JsonProperty("enabled")] public bool Enabled;
        [JsonProperty("url")] public string Url;
    }

    public class RevoltApiVortexFeature
    {
        [JsonProperty("enabled")] public bool Enabled;
        [JsonProperty("url")] public string Url;
        [JsonProperty("ws")] public string WebsocketUrl;
    }

    public class VortexInformation
    {
        [JsonProperty("voso")] public string Version;
        [JsonProperty("ws")] public string WebsocketUrl;
        [JsonProperty("features")] public VortexFeatures Features;
    }

    public class VortexFeatures
    {
        [JsonProperty("rtp")] public bool Rtp;
    }

    public class AutumnInformation
    {
        [JsonProperty("autumn")] public string Version;
        [JsonProperty("tags")] public AutumnInfoTags Tags;
        [JsonProperty("jpeg_quality")] public int JpegQuality;
    }

    public class AutumnInfoTags
    {
        [JsonProperty("avatars")] public AutumnInfoTag Avatars;
        [JsonProperty("icons")] public AutumnInfoTag Icons;
        [JsonProperty("banners")] public AutumnInfoTag Banners;
        [JsonProperty("backgrounds")] public AutumnInfoTag Backgrounds;
        [JsonProperty("attachments")] public AutumnInfoTag Attachments;
    }

    public class AutumnInfoTag
    {
        /// <summary>
        /// Maximum size in bytes.
        /// </summary>
        [JsonProperty("max_size")] public ulong MaxSize;

        [JsonProperty("enabled")] public bool Enabled;

        [JsonProperty("serve_if_field_present")]
        public string ServeIfFieldPresent;

        [JsonProperty("restrict_content_type")]
        public string RestrictContentType;
    }
}