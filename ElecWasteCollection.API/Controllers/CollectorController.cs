using ElecWasteCollection.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElecWasteCollection.API.Controllers
{
	[Route("api/collectors")]
	[ApiController]
	public class CollectorController : ControllerBase
	{
		private readonly ICollectorService _collectorService;
		public CollectorController(ICollectorService collectorService)
		{
			_collectorService = collectorService;
		}
		[HttpGet]
		public IActionResult GetAllCollectors()
		{
			var collectors = _collectorService.GetAll();
			return Ok(collectors);
		}
	}
}
