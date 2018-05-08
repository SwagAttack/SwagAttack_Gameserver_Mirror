using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Game : IGame
	{
	    private List<IGamePlayerInfo> _playerList;
	    public Game(string gameId, List<string> playerList)
	    {
			_playerList = new List<IGamePlayerInfo>();
			foreach (var player in playerList)
			{
				var enemyList = new List<string>();
				foreach (var VARIABLE in playerList)
				{
					if(VARIABLE != player) enemyList.Add(VARIABLE);
				}
				_playerList.Add(new GamePlayerInfo( player){ EnmeyList = enemyList });
			}

		    GameId = gameId;
	    }

	    public string GameId { get; set; }
		//public List<IGamePlayerInfo> PlayerList { get {_playerList}; set; }

	    public IGamePlayerInfo GetPlyerinfo(string username)
	    {
		    return _playerList.Find(x => x.Username == username);

	    }
    }
}