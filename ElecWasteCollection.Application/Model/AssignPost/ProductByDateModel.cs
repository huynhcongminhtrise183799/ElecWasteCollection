using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Model.AssignPost
{
    public class ProductByDateModel
    {
        public Guid ProductId { get; set; }
        public Guid PostId { get; set; }   
        public string ProductName { get; set; }
        public string UserName { get; set; }
        public string? Address { get; set; }
    }
}
