using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Snappy.Models
{
    public class SpotifyAuthorizationCode
    {
        public string Code { get; set; }
    }

    public enum SpotifyAuthType
    {
        ClientCredentials,
        AuthorizationCode
    }
}