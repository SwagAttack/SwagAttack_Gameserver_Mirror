
namespace Domain.Interfaces
{
	public interface IGame
	{
		string GameId { get; set; }

		IGamePlayerInfo GetPlyerinfo(string username);
	}
}