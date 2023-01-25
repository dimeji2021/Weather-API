using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Domain.Dto
{
    public class UserDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty ;
    }
}
