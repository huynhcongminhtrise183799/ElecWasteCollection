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
	public class BrandService : IBrandService
	{
		private readonly List<Brand> _brands = FakeDataSeeder.brands;
		public List<BrandModel> GetBrandsByCategoryIdAsync(Guid categoryId)
		{
			var brands = _brands
				.Where(b => b.CategoryId == categoryId)
				.Select(b => new BrandModel
				{
					BrandId = b.BrandId,
					Name = b.Name,
					CategoryId = b.CategoryId
				})
				.ToList();
			return brands;
		}
	}
}
