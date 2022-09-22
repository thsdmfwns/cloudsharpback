using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cloudsharpback.Models
{
    public class LoginDto 
    {
        public LoginDto(string id, string password)
        {
            Id = id;
            Password = password;
        }

        public string Id { get; set; }
        public string Password { get; set; }
    }
}
