using System;
using Nancy.Simple.BusinessObject;
using Nancy.Simple.Logic;
using Newtonsoft.Json.Linq;

namespace Nancy.Simple
{
	public static class PokerPlayer
	{
		public static readonly string VERSION = "Default C# folding player";

		public static int BetRequest(JObject gameState)
		{
			try
			{
				Game game = gameState.ToObject<Game>();
			
				return LogicService.Bet(game);
			}
			catch (Exception e)
			{
				Console.WriteLine(gameState);
				Console.WriteLine(e);
				return Int32.MaxValue;
			}
		}

		public static void ShowDown(JObject gameState)
		{
			//TODO: Use this method to showdown
		}
	}
}

