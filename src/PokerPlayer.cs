﻿using System;
using Newtonsoft.Json.Linq;

namespace Nancy.Simple
{
	public class PokerPlayer
	{
		public static readonly string VERSION = "Default C# folding player";

		public static int BetRequest(JObject gameState)
		{
			//TODO: Use this method to return the value You want to bet
			return 0;
		}

		public static int ShowDown(JObject gameState)
		{
			//TODO: Use this method to showdown
			return 0;
		}
	}
}
