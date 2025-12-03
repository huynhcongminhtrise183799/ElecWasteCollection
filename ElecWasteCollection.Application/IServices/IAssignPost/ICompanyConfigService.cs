using ElecWasteCollection.Application.Model.AssignPost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices.IAssignPost
{

    public interface ICompanyConfigService
    {
        CompanyConfigResponse UpdateCompanyConfig(CompanyConfigRequest request);
        CompanyConfigResponse GetCompanyConfig();


    }
}
