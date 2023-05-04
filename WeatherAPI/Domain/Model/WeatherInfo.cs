namespace WeatherAPI.Domain.Model
{

    public class WeatherInfo
    {
        public Coord Coord { get; set; }
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public Wind Wind { get; set; }
        public Sys Sys { get; set; }
    }
}
