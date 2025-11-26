using ElecWasteCollection.API.DTOs.Request;
using ElecWasteCollection.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElecWasteCollection.API.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		public AuthController(IUserService userService)
		{
			_userService = userService;
		}
		[HttpPost("login-google")]
		public async Task<IActionResult> LoginWithGoogle([FromBody] LoginGGRequest request)
		{
			var response = await _userService.LoginWithGoogleAsync(request.Token);
			return Ok(new { token = response });
		}
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var response = await _userService.Login(request.Username, request.Password);
			return Ok(new { token = response });
		}


	}
}
