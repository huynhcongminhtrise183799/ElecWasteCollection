using ElecWasteCollection.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices
{
    public interface ICategoryService
    {
        List<CategoryModel> GetParentCategory();

		List<CategoryModel> GetSubCategoryByParentId(Guid parentId);

		List<CategoryModel> GetSubCategoryByName(string name, Guid parentId);

	}
}
