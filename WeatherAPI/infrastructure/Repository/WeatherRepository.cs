namespace WeatherAPI.infrastructure.Repository
{
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WeatherAPI.Domain.Dto;
    using WeatherAPI.Domain.Model;
    using static WeatherAPI.Domain.Model.WeatherInfo;

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
            var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={_configuration.GetConnectionString("ApiKey")}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var weather = JsonConvert.DeserializeObject<root>(json);
                var weatherdto = new WeatherInfoDto()
                {
                    lon = weather.coord.lon.ToString(), 
                    lat = weather.coord.lat.ToString(),
                    Temperature = weather.main.temp,
                    Summary = weather.weather[0].main,
                    Details = weather.weather[0].description,
                    Pressure = weather.main.pressure.ToString(),
                    Humidity = weather.main.humidity.ToString(),
                    Sunrise = ConvertDateTime(weather.sys.sunrise).ToString(),
                    Sunset = ConvertDateTime(weather.sys.sunset).ToString(),
                    Icon = $"https://api.openweathermap.org/img/w/{weather.weather[0].icon}.png",
                };
                return ResponseDto<WeatherInfoDto>.Success("Successful", weatherdto, (int)HttpStatusCode.OK);
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<User>(json);
                return ResponseDto<WeatherInfoDto>.Fail("Location not found", (int)HttpStatusCode.NotFound);
            }

        }
        private DateTime ConvertDateTime(long millisec)
        {
            var day = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(millisec).ToLocalTime();
            return day;
        }
    }

}
