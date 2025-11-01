using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Domain.Entities
{
	public class PostImages
	{
		public Guid PostImageId { get; set; }

		public Guid PostId { get; set; }

		public string ImageUrl { get; set; }

		public string AiDetectedLabelsJson { get; set; }

		public Post Post { get; set; }
	}
}
