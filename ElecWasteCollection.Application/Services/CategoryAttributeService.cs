using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Services
{
	public class CategoryAttributeService : ICategoryAttributeService
	{
		private static List<CategoryAttributes> _categoriesAttribute = FakeDataSeeder.categoryAttributes;
		private static List<Attributes> _attributes = FakeDataSeeder.attributes;
		public List<CategoryAttributeModel> GetCategoryAttributesByCategoryId(Guid categoryId)
		{
			var categoryAttributes = _categoriesAttribute
				.Where(ca => ca.CategoryId == categoryId)
				.Select(ca => new CategoryAttributeModel
				{
					Id = ca.Id,
					Name = _attributes.FirstOrDefault(a => a.Id == ca.AttributeId)?.Name
				})
				.ToList();

			return categoryAttributes;
		}
	}
}
