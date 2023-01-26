namespace WeatherAPI.Domain.Dto
{
    public class WeatherInfoDto
    {
        public double Temperature { get; set; }
        public string Details { get; set; }
        public string Summary { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }
        public string Icon { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string lon { get; set; }
        public string lat { get; set; }
    }
}
