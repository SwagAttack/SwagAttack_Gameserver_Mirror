using System;
using System.Collections.Generic;
using System.Text;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Controllers
{
	class GameController : IGameController
	{
		private readonly IGameRepository _gameRepository;

		public GameController(IGameRepository gameRepo)
		{
			_gameRepository = gameRepo;
		}

		public void StartGame(string gameId, List<string> playesrList)
		{
			var game = new Game(gameId, playesrList);

			_gameRepository.CreateItemAsync(game);
		}

		public IGamePlayerInfo GetPlayersGameInfo(string gameId, string username)
		{
			var gameinfo = _gameRepository.GetItemAsync(gameId).Result;
			return gameinfo.GetPlyerinfo(username);

		}

		public bool LeaveGame(string gameId, string username)
		{
			return _gameRepository.DeleteItemAsync(gameId + "_" + username).IsCompleted;
		}
	}
}
