using ElecWasteCollection.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElecWasteCollection.API.Controllers
{
	[Route("api/categories")]
	[ApiController]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService _categorySerivce;
		private readonly ICategoryAttributeService _categoryAttributeService;
		public CategoryController(ICategoryService categorySerivce, ICategoryAttributeService categoryAttributeService)
		{
			_categorySerivce = categorySerivce;
			_categoryAttributeService = categoryAttributeService;
		}
		[HttpGet("parents")]
		public IActionResult GetParentCategories()
		{
			var parentCategories = _categorySerivce.GetParentCategory();
			return Ok(parentCategories);
		}
		[HttpGet("{parentId}/subcategories")]
		public IActionResult GetSubCategoriesByParentId(Guid parentId)
		{
			var subCategories = _categorySerivce.GetSubCategoryByParentId(parentId);
			return Ok(subCategories);
		}
		[HttpGet("{subCategoryId}/attributes")]
		public IActionResult GetAttributesByCategoryId([FromRoute]Guid subCategoryId)
		{
			var attributes = _categoryAttributeService.GetCategoryAttributesByCategoryId(subCategoryId);
			return Ok(attributes);
		}

	}
}
