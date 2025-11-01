using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model
{
	public class ImaggaCheckResult
	{
		public bool IsMatch { get; set; }
		public string DetectedTagsJson { get; set; }
	}
}
