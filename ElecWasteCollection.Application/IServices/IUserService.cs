using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices
{
	public interface IUserService
	{
		void AddUser(User user);
		void AddRange(IEnumerable<User> newUsers);
		//void UpdateUser(int iat, int ing, Guid id);
		List<User> GetAll();
		User GetById(Guid id);
		Task<string> LoginWithGoogleAsync(string token);

		Task<string> Login(string userName, string password);

		UserProfileResponse Profile(string email);

		User? GetByPhone(string phone);

	}
}
