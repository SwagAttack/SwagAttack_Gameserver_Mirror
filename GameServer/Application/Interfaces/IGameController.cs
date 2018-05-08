using System;
using System.Collections.Generic;
using System.Text;
using Domain.Interfaces;

namespace Application.Interfaces
{
    public interface IGameController
    {
	    /// <summary>
		/// starts a game with a Id and a list of player names
		/// </summary>
		/// <param name="gameId"> </param>
		/// <param name="playesrList"></param>
	    void StartGame(string gameId, List<string> playesrList);

	    IGamePlayerInfo GetPlayersGameInfo(string gameId, string username);

		bool LeaveGame(string gameId, string username);
	}
}
