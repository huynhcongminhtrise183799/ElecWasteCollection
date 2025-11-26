using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model
{
	public class UserResponse
	{
		public Guid UserId { get; set; }
		public string? Name { get; set; }

		public string? Email { get; set; }
	}
}
