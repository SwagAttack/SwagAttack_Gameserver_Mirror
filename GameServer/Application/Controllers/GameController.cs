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
		    foreach (var player in playesrList)
		    {
			    _gameRepository.CreateItemAsync(new GamePlayerInfo(gameId, player));
		    }
	    }

	    public IGamePlayerInfo GetPlayersGameInfo(string gameId, string username)
	    {
		    return _gameRepository.GetItemAsync(gameId + "_" + username).Result;
	    }

	    public bool LeaveGame(string gameId, string username)
	    {
		   return _gameRepository.DeleteItemAsync(gameId + "_" + username).IsCompleted;
	    }
    }
}
