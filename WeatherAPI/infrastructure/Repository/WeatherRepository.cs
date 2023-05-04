namespace WeatherAPI.infrastructure.Repository
{
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using WeatherAPI.Domain.Dto;
    using WeatherAPI.Domain.Model;
    using static WeatherAPI.Domain.Model.WeatherInfo;
    // This should be a service and not a repository
    public class WeatherRepository : IWeatherRepository
    {
        private readonly HttpClient _httpClient;
        private IConfiguration _configuration;

        public WeatherRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ResponseDto<WeatherInfoDto>> GetCurrentWeatherAsync(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return ResponseDto<WeatherInfoDto>.Fail("Location field is required", (int)HttpStatusCode.BadRequest);
            }
            var apiKey = _configuration.GetConnectionString("ApiKey");
            var url = _configuration.GetConnectionString("ApiUrl");
            var response = await _httpClient.GetAsync($"{url}{location}{apiKey}");
            if (response.IsSuccessStatusCode)
            {
                var weather = await response.Content.ReadFromJsonAsync<WeatherInfo>(); // Rather than above
                var weatherdto = new WeatherInfoDto()
                {
                    lon = weather.Coord.lon.ToString(),
                    lat = weather.Coord.lat.ToString(),
                    Temperature = weather.Main.temp,
                    Summary = weather.Weather[0].main,
                    Details = weather.Weather[0].description,
                    Pressure = weather.Main.pressure.ToString(),
                    Humidity = weather.Main.humidity.ToString(),
                    Sunrise = ConvertDateTime(weather.Sys.sunrise).ToString(),
                    Sunset = ConvertDateTime(weather.Sys.sunset).ToString(),
                    Icon = $"https://api.openweathermap.org/img/w/{weather.Weather[0].icon}.png",
                };
                return ResponseDto<WeatherInfoDto>.Success("Successful", weatherdto, (int)HttpStatusCode.OK);
            }
            else
            {
                return ResponseDto<WeatherInfoDto>.Fail(response.ReasonPhrase, (int)response.StatusCode);
            }
        }

        //// This is the shorter way of calling the above API or any external API, but you wouldn't be able to capture
        //// the required error resposne eimplicitly
        //public async Task<ResponseDto<WeatherInfoDto>> GetCurrentWeatherAsyncShort(string location)
        //{
        //    var apiKey = _configuration.GetConnectionString("ApiKey");
        //    var url = _configuration.GetConnectionString("ApiUrl");
        //    try
        //    {
        //        var weather = await _httpClient.GetFromJsonAsync<WeatherInfo>($"{url}{location}{apiKey}");// A short way of doing this
        //        var weatherdto = new WeatherInfoDto()
        //        {
        //            lon = weather.Coord.lon.ToString(),
        //            lat = weather.Coord.lat.ToString(),
        //            Temperature = weather.Main.temp,
        //            Summary = weather.Weather[0].main,
        //            Details = weather.Weather[0].description,
        //            Pressure = weather.Main.pressure.ToString(),
        //            Humidity = weather.Main.humidity.ToString(),
        //            Sunrise = ConvertDateTime(weather.Sys.sunrise).ToString(),
        //            Sunset = ConvertDateTime(weather.Sys.sunset).ToString(),
        //            Icon = $"https://api.openweathermap.org/img/w/{weather.Weather[0].icon}.png",
        //        };
        //        return ResponseDto<WeatherInfoDto>.Success("Successful", weatherdto, (int)HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {

        //        return ResponseDto<WeatherInfoDto>.Fail(ex.Message, (int)HttpStatusCode.InternalServerError);
        //    }
        //}
        private DateTime ConvertDateTime(long millisec)
        {
            var day = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(millisec).ToLocalTime();
            return day;
        }
    }

}
