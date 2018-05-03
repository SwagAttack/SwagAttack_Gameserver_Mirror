using Domain.Interfaces;
using Domain.Models;
using Persistance.Interfaces;
using Persistance.Repository;

namespace Domain.Interfaces
{
	public class GameRepository : SwagRepository<IGamePlayerInfo, GamePlayerInfo>, IGameRepository
	{
		public GameRepository(IDbContext context, string collectionId) : base(context, collectionId)
		{
		}
	}
}
