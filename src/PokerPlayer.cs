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
			Game game = gameState.ToObject<Game>();
			
			return LogicService.Bet(game);
			
			
			
			//TODO: Use this method to return the value You want to bet
			//return Int32.MaxValue;
		}

		public static void ShowDown(JObject gameState)
		{
			//TODO: Use this method to showdown
		}
		
		/*curl -d 'action=bet_request&game_state={
			"players":[
			{
				"name":"Player 1",
				"stack":1000,
				"status":"active",
				"bet":0,
				"hole_cards":[],
				"version":"Version name 1",
				"id":0
			},
			{
				"name":"Player 2",
				"stack":1000,
				"status":"active",
				"bet":0,
				"hole_cards":[],
				"version":"Version name 2",
				"id":1
			}
			],
			"tournament_id":"550d1d68cd7bd10003000003",
			"game_id":"550da1cb2d909006e90004b1",
			"round":0,
			"bet_index":0,
			"small_blind":10,
			"orbits":0,
			"dealer":0,
			"community_cards":[],
			"current_buy_in":0,
			"pot":0
		}' http://boiling-reef-21262.herokuapp.com/*/
		
		
	}
}

