using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Domain.Entities
{
	public class UserPoints
	{
		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public double Points { get; set; }
	}
}
