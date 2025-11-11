namespace ElecWasteCollection.API.DTOs.Request
{
	public class CreatePackageRequest
	{
		public string PackageId { get; set; }

		public string PackageName { get; set; }

		public int SmallCollectionPointsId { get; set; }

		public List<string> ProductsQrCode { get; set; }
	}
}
