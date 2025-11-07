using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Services
{
	public class UserService : IUserService
	{
		private List<User> users = FakeDataSeeder.users;
		private readonly IFirebaseService _firebaseService;
		private readonly ITokenService _tokenService;

		public UserService(IFirebaseService firebaseService, ITokenService tokenService)
		{
			_firebaseService = firebaseService;
			_tokenService = tokenService;
		}

		public List<User> GetAll()
		{

			return users;
		}

		public void AddRange(IEnumerable<User> newUsers)
		{
			throw new NotImplementedException();
		}

		public void AddUser(User user)
		{
			throw new NotImplementedException();
		}

		public  User GetById(Guid id)
		{
			return  users.FirstOrDefault(u => u.UserId == id);
		}

		public void UpdateUser(int iat, int ing, Guid id)
		{
			var user = users.FirstOrDefault(u => u.UserId == id);
			if (user != null)
			{
				user.Iat = iat;
				user.Ing = ing;
			}
		}

		public async Task<string> LoginWithGoogleAsync(string token)
		{
			var decodedToken = await _firebaseService.VerifyIdTokenAsync(token);
			var email = decodedToken.Claims["email"].ToString();
			if (email == null) throw new Exception("Không lấy được email từ trong token firebase");
			string name = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString() : email;
			string picture = decodedToken.Claims.ContainsKey("picture") ? decodedToken.Claims["picture"].ToString() : null;
			var user = users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				user = new User
				{
					UserId = Guid.NewGuid(),
					Email = email,
					Name = name,
					Avatar = picture,
					//Role = "User",
					Iat =null,
					Ing = null
				};
				users.Add(user);
			}

			var accessToken = await _tokenService.GenerateToken(user);
			return accessToken;
		}

		public async Task<string> Login(string email)
		{
			var user = users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				throw new Exception("User not found");
			}
			var accessToken = await _tokenService.GenerateToken(user);
			return accessToken;
		}

		public User Profile(string email)
		{
			var user = users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{
				throw new Exception("User not found");
			}
			return user;
		}
	}
}
