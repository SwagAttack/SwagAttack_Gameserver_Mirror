using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
	public interface IGamePlayerInfo
	{
		
		string Username { get; }
		int AmountOfgold { get; set; }
		int NumberOfWorkers { get; set; }
		int NumberOfSoldier { get; set; }
		List<string> EnmeyList { get; set; }
		void RemoveEnemy(string enemyName);
	}
}
