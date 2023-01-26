using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.infrastructure.Repository;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private IWeatherRepository _weatherService;
        //private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(/*ILogger<WeatherForecastController> logger,*/ IWeatherRepository weatherService)
        {
            //_logger = logger;
            _weatherService = weatherService;
        }

        [HttpGet(Name = "GetWeatherForecast"), Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Accessible only by an Admin", Description ="Return the weathernreport of specified location")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(string location)
        {
            return Ok(await _weatherService.GetCurrentWeatherAsync(location));
        }
    }
}