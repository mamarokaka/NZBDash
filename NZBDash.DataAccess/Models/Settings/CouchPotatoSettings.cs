﻿namespace NZBDash.DataAccess.Models.Settings
{
    public class CouchPotatoSettings : SettingsEntity
    {
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
