using WeatherAPI.Domain.Dto;
using WeatherAPI.Domain.Model;

namespace WeatherAPI.Domain.Core.Service
{
    public interface IAuthService
    {
        Task<ResponseDto<string>> Login(UserDto request);
        Task<ResponseDto<User>> Register(UserDto request);
        Task<ResponseDto<string>> RefreshToken();
    }
}