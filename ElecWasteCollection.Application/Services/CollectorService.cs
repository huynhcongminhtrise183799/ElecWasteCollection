using ElecWasteCollection.Application.Data;
using ElecWasteCollection.Application.IServices;
using ElecWasteCollection.Application.Model;
using ElecWasteCollection.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.Services
{
	public class CollectorService : ICollectorService
	{
		private  List<User> collectors = FakeDataSeeder.users;

		public List<CollectorResponse> GetAll()
		{
			var response = collectors.Select(c => new CollectorResponse
			{
				CollectorId = c.UserId,
				Name = c.Name,
				Email = c.Email,
				Phone = c.Phone,
				Avatar = c.Avatar,
				SmallCollectionPointId = c.SmallCollectionPointId
			}).ToList();

			return response;

		}

		public CollectorResponse? GetById(Guid id)
		{
			var collector = collectors.FirstOrDefault(c => c.UserId == id);
			if (collector == null)
			{
				return null;
			}

			var response = new CollectorResponse
			{
				CollectorId = collector.UserId,
				Name = collector.Name,
				Email = collector.Email,
				Phone = collector.Phone,
				Avatar = collector.Avatar,
				SmallCollectionPointId = collector.SmallCollectionPointId
			};

			return response;
		}
	}
}
