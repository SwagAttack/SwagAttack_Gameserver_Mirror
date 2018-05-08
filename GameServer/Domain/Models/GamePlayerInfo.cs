using System;
using System.Collections.Generic;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class GamePlayerInfo : IGamePlayerInfo
    {
	    public GamePlayerInfo(string username)
	    {
		    Username = username;
	    }
	    public string Username { get;}
	    public int AmountOfgold { get; set; } = 100;
		public int NumberOfWorkers { get; set; } = 2;
		public int  NumberOfSoldier { get; set; } = 3;
	    public List<string> EnmeyList { get; set; }
	    public void RemoveEnemy(string enemyName)
	    {
		    throw new NotImplementedException();
	    }
    }
}
