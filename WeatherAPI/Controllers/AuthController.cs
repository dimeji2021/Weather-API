using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeatherAPI.Domain.Core.Service;
using WeatherAPI.Domain.Dto;

namespace WeatherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("Register")]
        [SwaggerOperation(Summary = "Register a new User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(UserDto request)
        {
            return Ok(await _authService.Register(request));
        }
        [HttpPost("Register_Admin")]
        [SwaggerOperation(Summary = "Register an Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAdmin(UserDto request)
        {
            return Ok(await _authService.RegisterAdmin(request));
        }
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "To login in to the application")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(UserDto request)
        {
            return Ok(await _authService.Login(request));
        }
        [HttpPost("RefreshToken")]
        [SwaggerOperation(Summary = "Refresh token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            return Ok(await _authService.RefreshToken());
        }
    }
}
