using ElecWasteCollection.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices
{
	public interface IUserAddressService
	{
		bool AddUserAddress(CreateUpdateUserAddress create);
		bool UpdateUserAddress(Guid userId, CreateUpdateUserAddress update);
		bool DeleteUserAddress(Guid userId);
		List<UserAddressResponse>? GetByUserId(Guid userId);

	}
}
