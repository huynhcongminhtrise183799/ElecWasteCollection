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
	public class ProductService : IProductService
	{
		private readonly List<Products> _products = FakeDataSeeder.products;
		private readonly List<SizeTier> _sizeTiers = FakeDataSeeder.sizeTiers;
		private readonly List<ProductValues> _productValues = FakeDataSeeder.productValues;
		private readonly List<Attributes> _attributes = FakeDataSeeder.attributes;
		private readonly List<Brand> _brands = FakeDataSeeder.brands;
		public bool AddPackageIdToProductByQrCode(string qrCode, string packageId)
		{
			var product = _products.FirstOrDefault(p => p.QRCode == qrCode);
			if (product == null)
			{
				return false;
			}

			product.PackageId = packageId;
			return true;
		}

		public Products? GetById(Guid productId)
		{
			return _products.FirstOrDefault(p => p.Id == productId);
		}

		public Products? GetByQrCode(string qrcode)
		{
			return _products.FirstOrDefault(p => p.QRCode == qrcode);
		}

		public List<ProductDetailModel> GetProductsByPackageId(string packageId)
		{
			// 1. Lọc ra các sản phẩm thuộc package
			var productsInPackage = _products
				.Where(p => p.PackageId == packageId);

			// 2. Dùng .Select để biến đổi TỪNG sản phẩm
			var productDetails = productsInPackage.Select(p =>
			{
				// 3. "Join" bằng tay với SizeTiers
				var sizeTier = _sizeTiers
					.FirstOrDefault(st => st.SizeTierId == p.SizeTierId);
				var brand = _brands
					.FirstOrDefault(b => b.BrandId == p.BrandId);
				// 4. "Join" bằng tay với ProductValues và Attributes
				var attributesList = _productValues
					.Where(pv => pv.ProductId == p.Id) // Lấy các value của sản phẩm này
					.Select(pv =>
					{
						// Với mỗi value, tìm attribute tương ứng
						var attribute = _attributes
							.FirstOrDefault(a => a.Id == pv.AttributeId);

						return new ProductValueDetailModel
						{
							AttributeName = attribute?.Name ?? "N/A",
							Value = pv.Value.ToString(),
						};
					})
					.ToList();

				// 5. Xây dựng model hoàn chỉnh
				return new ProductDetailModel
				{
					ProductId = p.Id,
					Description = p.Description,
					BrandName = brand?.Name,
					BrandId = brand.BrandId,
					SizeTierName = sizeTier?.Name,
					Attributes = attributesList
				};
			})
			.ToList();

			return productDetails;
		}

		public bool UpdateProductStatusByQrCode(string productQrCode, string status)
		{
			var product = _products.FirstOrDefault(p => p.QRCode == productQrCode);
			if (product == null)
			{
				return false;
			}

			product.Status = status;
			return true;
		}
	}
}
