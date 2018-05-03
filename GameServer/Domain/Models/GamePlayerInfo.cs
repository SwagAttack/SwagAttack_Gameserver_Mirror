using System;
using System.Collections.Generic;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class GamePlayerInfo : IGamePlayerInfo
    {
	    public GamePlayerInfo(string GameId, string username)
	    {
		    id = GameId + "_" + Username;

	    }
		public string id { get; }
		public string GameId { get;}
	    public string Username { get;}
	    public int AmountOfgold { get; set; } = 100;
		public int NumberOfWorkers { get; set; } = 2;
		public int  NumberOfSoldier { get; set; } = 3;
	}
}
