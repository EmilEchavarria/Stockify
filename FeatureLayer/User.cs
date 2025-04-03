using System;

namespace FeatureLayer.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Constructor opcional
        public User() { }

        public User(string username, string password, string clientName, string email, string phone)
        {
            Username = username;
            Password = password;
            ClientName = clientName;
            Email = email;
            Phone = phone;
        }
    }
}
