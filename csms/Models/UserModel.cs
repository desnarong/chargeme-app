﻿namespace csms.Models
{
    public class UserModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public bool IsAdmin { get; set; }
    }
}
