namespace ElecWasteCollection.API.DTOs.Request
{
    public class CreatePostAtWarehouseRequest
    {
		public Guid SenderId { get; set; }
		//public string Name { get; set; }
		public string Description { get; set; }

		public List<string> Images { get; set; }

		public Guid ParentCategoryId { get; set; }

		public Guid SubCategoryId { get; set; }
		public Guid BrandId { get; set; }

		public string QrCode { get; set; }
	}
}
