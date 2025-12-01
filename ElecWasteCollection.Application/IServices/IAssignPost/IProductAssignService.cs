using ElecWasteCollection.Application.Model.AssignPost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices.IAssignPost
{
    public interface IProductAssignService
    {
        Task<AssignProductResult> AssignProductsAsync(List<Guid> productIds,DateOnly workDate); 
        Task<List<ProductByDateModel>> GetProductsByWorkDateAsync(DateOnly workDate);




    }
}
