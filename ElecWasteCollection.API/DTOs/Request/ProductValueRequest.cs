namespace ElecWasteCollection.API.DTOs.Request
{
	public class ProductValueRequest
	{
		public Guid AttributeId { get; set; }

		public string Value { get; set; }
	}
}
