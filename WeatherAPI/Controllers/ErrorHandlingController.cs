using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WeatherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorHandlingController : ControllerBase
    {
        public IActionResult HandleError()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionHandlerFeature!.Error;

            if (exception is NotFoundException notFoundException)
            {
                return Problem(notFoundException.Message, null, 404);
            }
            return Problem("An error occured");
        }
    }
}
