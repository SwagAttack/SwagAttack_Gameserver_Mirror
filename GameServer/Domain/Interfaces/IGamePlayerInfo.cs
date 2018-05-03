using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
	public interface IGamePlayerInfo
	{
		/// <summary>
		/// Id = GameId + _ + Username
		/// </summary>
		string id { get;} 
		string GameId { get; }
		string Username { get; }
		int AmountOfgold { get; set; }
		int NumberOfWorkers { get; set; }
		int NumberOfSoldier { get; set; }
	}
}
