using ElecWasteCollection.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElecWasteCollection.Application.IServices
{
	public interface IUserPointService
	{
		UserPointModel GetPointByUserId(Guid userId);

		bool UpdatePointForUser(Guid userId, double point);
	}
}
