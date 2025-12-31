using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElecWasteCollection.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecyclingAssignController : ControllerBase
    {
        private readonly IPackageAssignService _service;

        public RecyclingAssignController(IPackageAssignService service)
        {
            _service = service;
        }

        [HttpGet("configs")]
        public async Task<IActionResult> GetConfigs()
        {
            try { return Ok(await _service.GetRecyclerConfigsAsync()); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("configs")]
        public async Task<IActionResult> UpdateConfigs([FromBody] List<UpdateRecyclerRatioDto> configs)
        {
            try
            {
                await _service.UpdateRecyclerRatiosAsync(configs);
                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpGet("packages-by-date")]
        public async Task<IActionResult> GetPackagesByDate([FromQuery] DateTime date)
        {
            try
            {
                var result = await _service.GetPackagesByDateAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignPackages([FromBody] List<string> packageIds)
        {
            if (packageIds == null || !packageIds.Any()) return BadRequest("List ID trống");
            try { return Ok(await _service.AssignPackagesToRecyclersAsync(packageIds)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("assigned-packages")]
        public async Task<IActionResult> GetAssignedPackages([FromQuery] DateTime date, [FromQuery] string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest(new { message = "CompanyId không được để trống" });
            }

            try
            {
                var result = await _service.GetAssignedPackagesByCompanyAsync(date, companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("recycling-companies")]
        public async Task<IActionResult> GetRecyclingCompanies()
        {
            try
            {
                var result = await _service.GetRecyclingCompaniesAsync();

                if (result == null || !result.Any())
                {
                    return Ok(new { message = "Không tìm thấy công ty tái chế nào.", data = result });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}