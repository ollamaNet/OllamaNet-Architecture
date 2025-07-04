﻿using System.Text.Json.Serialization;

namespace AuthenticationService.DTOs
{
    public class AuthModel
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string RootFolderId { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }

        //for refreshtoken
        [JsonIgnore]
        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }
    }
}
