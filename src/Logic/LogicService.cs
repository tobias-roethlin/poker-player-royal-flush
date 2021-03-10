using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Simple.BusinessObject;

namespace Nancy.Simple.Logic
{
    public static class LogicService
    {
        public static int Bet(Game game)
        {
            var tournament = CreateTournament(game);

            var higherCard = GetHigherCard(tournament.OurPlayer.Card1, tournament.OurPlayer.Card2);
            var lowerCard = higherCard == tournament.OurPlayer.Card1 ? tournament.OurPlayer.Card2 : tournament.OurPlayer.Card1;
            var card1String = higherCard.GetString();
            var card2String = lowerCard.GetString();
            var firstCombination = card1String + card2String;
            var secondCombination =  card2String + card1String;

            if (higherCard.Color == lowerCard.Color)
            {
                firstCombination += "s";
                secondCombination += "s";
            }

            var action = PokerHandActionEvaluator.GetActionBasendOnHandCards(higherCard, lowerCard);
            if (action == PokerAction.Fold)
            {
                return 0;
            }

            var probabilities = GetInitialProbabilities(1 + tournament.OtherPlayers.Count(p => p.Status != "out"));
            double probability = 0;
            if (!probabilities.TryGetValue(firstCombination, out probability))
            {
                probabilities.TryGetValue(secondCombination, out probability);
            }

            var betValue = (int)(tournament.OurPlayer.Stack * (1.0 / 100 * probability));
            var maxBetValue = betValue * 2;

            if (tournament.Round == 0 && betValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            if (maxBetValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            return Math.Min(Math.Max(betValue, tournament.CurrentBuyIn), tournament.OurPlayer.Stack);
        }

        private static Card GetHigherCard(Card card1, Card card2)
        {
            if (((int)card1.Rank) > ((int)card2.Rank))
            {
                return card1;
            }

            return card2;
        }    

        private static Tournament CreateTournament(Game game)
        {
            var tournament = new Tournament();
            
            tournament.Round = game.round;
            tournament.CommunityCards = game.community_cards.Select(c => new Card {Color = c.suit, Rank = StringToRankMapper(c.rank)}).ToList();
            tournament.OtherPlayers = game.players.Where(p => p.name != "Royal Flush").Select(PlayerMapper).ToList();
            tournament.CurrentBuyIn = game.current_buy_in;

            var ourPlayer = game.players.SingleOrDefault(p => p.name == "Royal Flush");
            if (ourPlayer != null)
            {
                tournament.OurPlayer = PlayerMapper( ourPlayer);
            }

            return tournament;
        }

        public static Player PlayerMapper(BusinessObject.Player playerJson)
        {
            var player = new Player();

            player.Bet = playerJson.bet;

            if (playerJson.hole_cards.Count >= 2)
            {
                player.Card1 = CardMapper(playerJson.hole_cards[0]);
                player.Card2 = CardMapper(playerJson.hole_cards[1]);
            }

            player.Name = playerJson.name;
            player.Stack = int.Parse(playerJson.stack);
            player.Status = playerJson.status;
            
            return player;
        }

        public static Card CardMapper( BusinessObject.Card cardJson)
        {
            var card = new Card();
            card.Color = cardJson.suit;
            card.Rank = StringToRankMapper(cardJson.rank);
            return card;
        }
                
        public static Rank StringToRankMapper(string rankString)
        {
            switch (rankString)
            {
                case "A": return Rank.Ace;
                case "K": return Rank.King;
                case "Q": return Rank.Queen;
                case "J": return Rank.Jack;
                case "10": return Rank._10;
                case "9": return Rank._9;
                case "8": return Rank._8;
                case "7": return Rank._7;
                case "6": return Rank._6;
                case "5": return Rank._5;
                case "4": return Rank._4;
                case "3": return Rank._3;
                case "2": return Rank._2;
            }

            throw new NotImplementedException();
        }
        
        public enum Rank
        {
            Ace = 14,
            King = 13,
            Queen = 12,
            Jack = 11,
            _10 = 10,
            _9 = 9,
            _8 = 8,
            _7 = 7,
            _6 = 6,
            _5 = 5,
            _4 = 4,
            _3 = 3,
            _2 = 2
        }

        public class Tournament
        {
            public List<Card> CommunityCards { get; set; }
            public List<Player> OtherPlayers { get; set; }
            public Player OurPlayer { get; set; }
            public int Round { get; set; }
            public int CurrentBuyIn { get; set; }
        }

        public class Player
        {
            public string Name { get; set; }
            public int Stack { get; set; }
            public string Status { get; set; }
            public int Bet { get; set; }
            public Card Card1 { get; set; }
            public Card Card2 { get; set; }

            public bool SameRank()
            {
                return Card1.Rank == Card2.Rank;
            }

            public bool SameColor()
            {
                return Card1.Color == Card2.Color;
            }
        }

        public class Card
        {
            public Rank Rank { get; set; }
            public string Color { get; set; }

            public string GetString()
            {
                if (Rank == Rank.Ace)
                {
                    return "A";
                }

                if (Rank == Rank.King)
                {
                    return "K";
                }

                if (Rank == Rank.Queen)
                {
                    return "Q";
                }

                if (Rank == Rank.Jack)
                {
                    return "J";
                }

                return ((int)Rank).ToString();
            }
        }

        private static Dictionary<string, double> GetInitialProbabilities(int numberOfPlayers)
        {
            if (numberOfPlayers >= 8)
            {
                return GetInitialProbabilities8();
            }

            if (numberOfPlayers >= 7)
            {
                return GetInitialProbabilities7();
            }
            if (numberOfPlayers >= 6)
            {
                return GetInitialProbabilities6();
            }
            if (numberOfPlayers >= 5)
            {
                return GetInitialProbabilities5();
            }
            if (numberOfPlayers >= 4)
            {
                return GetInitialProbabilities4();
            }
            if (numberOfPlayers >= 3)
            {
                return GetInitialProbabilities3();
            }

            return GetInitialProbabilities2();
        }

        private static Dictionary<string, double> GetInitialProbabilities2()
        {
            var dict = new Dictionary<string, double>();
            dict.Add("AA", 85.54);
            dict.Add("KK", 82.68);
            dict.Add("QQ", 80.24);
            dict.Add("JJ", 77.81);
            dict.Add("TT", 75.38);
            dict.Add("99", 72.49);
            dict.Add("88", 69.63);
            dict.Add("KAs", 67.94);
            dict.Add("QAs", 67.19);
            dict.Add("77", 66.76);
            dict.Add("JAs", 66.5);
            dict.Add("KA", 66.24);
            dict.Add("TAs", 65.84);
            dict.Add("QA", 65.44);
            dict.Add("JA", 64.69);
            dict.Add("QKs", 64.55);
            dict.Add("9As", 64.17);
            dict.Add("TA", 63.99);
            dict.Add("JKs", 63.83);
            dict.Add("66", 63.76);
            dict.Add("8As", 63.51);
            dict.Add("TKs", 63.16);
            dict.Add("7As", 62.72);
            dict.Add("QK", 62.65);
            dict.Add("9A", 62.21);
            dict.Add("5As", 61.92);
            dict.Add("JK", 61.89);
            dict.Add("JQs", 61.74);
            dict.Add("6As", 61.62);
            dict.Add("9Ks", 61.53);
            dict.Add("8A", 61.5);
            dict.Add("TK", 61.19);
            dict.Add("TQs", 61.07);
            dict.Add("4As", 61.07);
            dict.Add("55", 61.03);
            dict.Add("7A", 60.66);
            dict.Add("3As", 60.27);
            dict.Add("8Ks", 60.04);
            dict.Add("5A", 59.78);
            dict.Add("JQ", 59.68);
            dict.Add("6A", 59.49);
            dict.Add("2As", 59.47);
            dict.Add("7Ks", 59.43);
            dict.Add("9K", 59.42);
            dict.Add("9Qs", 59.41);
            dict.Add("TJs", 59.31);
            dict.Add("TQ", 58.96);
            dict.Add("4A", 58.87);
            dict.Add("6Ks", 58.56);
            dict.Add("3A", 57.99);
            dict.Add("5Ks", 57.97);
            dict.Add("8Qs", 57.95);
            dict.Add("8K", 57.82);
            dict.Add("44", 57.81);
            dict.Add("9Js", 57.64);
            dict.Add("9Q", 57.2);
            dict.Add("7K", 57.17);
            dict.Add("2A", 57.14);
            dict.Add("TJ", 57.12);
            dict.Add("4Ks", 57.09);
            dict.Add("7Qs", 56.41);
            dict.Add("3Ks", 56.27);
            dict.Add("6K", 56.24);
            dict.Add("9Ts", 56.2);
            dict.Add("8Js", 56.15);
            dict.Add("6Qs", 55.78);
            dict.Add("8Q", 55.62);
            dict.Add("5K", 55.6);
            dict.Add("2Ks", 55.43);
            dict.Add("9J", 55.3);
            dict.Add("5Qs", 55.18);
            dict.Add("8Ts", 54.7);
            dict.Add("4K", 54.66);
            dict.Add("7Js", 54.62);
            dict.Add("33", 54.57);
            dict.Add("4Qs", 54.3);
            dict.Add("7Q", 53.98);
            dict.Add("9T", 53.79);
            dict.Add("3K", 53.74);
            dict.Add("8J", 53.71);
            dict.Add("3Qs", 53.46);
            dict.Add("89s", 53.34);
            dict.Add("6Q", 53.27);
            dict.Add("7Ts", 53.17);
            dict.Add("6Js", 52.97);
            dict.Add("2K", 52.84);
            dict.Add("5Q", 52.64);
            dict.Add("5Js", 52.61);
            dict.Add("2Qs", 52.58);
            dict.Add("8T", 52.17);
            dict.Add("7J", 52.1);
            dict.Add("79s", 51.85);
            dict.Add("4Js", 51.72);
            dict.Add("4Q", 51.69);
            dict.Add("6Ts", 51.51);
            dict.Add("22", 51.32);
            dict.Add("3Js", 50.87);
            dict.Add("78s", 50.86);
            dict.Add("3Q", 50.78);
            dict.Add("89", 50.76);
            dict.Add("7T", 50.54);
            dict.Add("6J", 50.33);
            dict.Add("69s", 50.21);
            dict.Add("5Ts", 50.05);
            dict.Add("2Js", 50.02);
            dict.Add("5J", 49.93);
            dict.Add("2Q", 49.86);
            dict.Add("4Ts", 49.4);
            dict.Add("68s", 49.22);
            dict.Add("79", 49.17);
            dict.Add("4J", 48.98);
            dict.Add("6T", 48.79);
            dict.Add("59s", 48.77);
            dict.Add("3Ts", 48.54);
            dict.Add("67s", 48.45);
            dict.Add("78", 48.12);
            dict.Add("3J", 48.06);
            dict.Add("58s", 47.77);
            dict.Add("2Ts", 47.68);
            dict.Add("69", 47.4);
            dict.Add("5T", 47.21);
            dict.Add("2J", 47.15);
            dict.Add("57s", 47.08);
            dict.Add("49s", 46.95);
            dict.Add("4T", 46.53);
            dict.Add("56s", 46.48);
            dict.Add("68", 46.35);
            dict.Add("39s", 46.34);
            dict.Add("48s", 45.98);
            dict.Add("59", 45.85);
            dict.Add("3T", 45.6);
            dict.Add("67", 45.52);
            dict.Add("29s", 45.48);
            dict.Add("47s", 45.31);
            dict.Add("45s", 45.13);
            dict.Add("58", 44.82);
            dict.Add("46s", 44.76);
            dict.Add("2T", 44.68);
            dict.Add("38s", 44.13);
            dict.Add("57", 44.1);
            dict.Add("49", 43.92);
            dict.Add("28s", 43.55);
            dict.Add("37s", 43.49);
            dict.Add("56", 43.48);
            dict.Add("35s", 43.37);
            dict.Add("39", 43.27);
            dict.Add("36s", 42.96);
            dict.Add("48", 42.9);
            dict.Add("29", 42.36);
            dict.Add("34s", 42.32);
            dict.Add("47", 42.2);
            dict.Add("45", 42.02);
            dict.Add("46", 41.63);
            dict.Add("27s", 41.61);
            dict.Add("25s", 41.54);
            dict.Add("26s", 41.11);
            dict.Add("38", 40.94);
            dict.Add("24s", 40.52);
            dict.Add("28", 40.3);
            dict.Add("37", 40.25);
            dict.Add("35", 40.14);
            dict.Add("36", 39.71);
            dict.Add("23s", 39.66);
            dict.Add("34", 39.03);
            dict.Add("27", 38.24);
            dict.Add("25", 38.18);
            dict.Add("26", 37.71);
            dict.Add("24", 37.09);
            dict.Add("23", 36.2);

            return dict;
        }

        private static Dictionary<string, double> GetInitialProbabilities3()
        {
            var dict = new Dictionary<string, double>();
            dict.Add("AA", 73.89);
            dict.Add("KK", 69.24);
            dict.Add("QQ", 65.33);
            dict.Add("JJ", 61.61);
            dict.Add("TT", 58.06);
            dict.Add("99", 54.12);
            dict.Add("KAs", 51.88);
            dict.Add("QAs", 50.71);
            dict.Add("88", 50.48);
            dict.Add("JAs", 49.66);
            dict.Add("KA", 49.4);
            dict.Add("TAs", 48.7);
            dict.Add("QKs", 48.46);
            dict.Add("QA", 48.15);
            dict.Add("JKs", 47.41);
            dict.Add("77", 47.04);
            dict.Add("JA", 47.01);
            dict.Add("TKs", 46.46);
            dict.Add("9As", 46.34);
            dict.Add("TA", 45.97);
            dict.Add("JQs", 45.89);
            dict.Add("QK", 45.8);
            dict.Add("8As", 45.49);
            dict.Add("TQs", 44.94);
            dict.Add("JK", 44.67);
            dict.Add("7As", 44.54);
            dict.Add("9Ks", 44.12);
            dict.Add("TJs", 43.9);
            dict.Add("5As", 43.83);
            dict.Add("TK", 43.66);
            dict.Add("66", 43.6);
            dict.Add("9A", 43.46);
            dict.Add("6As", 43.16);
            dict.Add("JQ", 43.11);
            dict.Add("4As", 42.91);
            dict.Add("9Qs", 42.6);
            dict.Add("8A", 42.52);
            dict.Add("8Ks", 42.15);
            dict.Add("TQ", 42.1);
            dict.Add("3As", 42.07);
            dict.Add("9Js", 41.57);
            dict.Add("7A", 41.49);
            dict.Add("7Ks", 41.42);
            dict.Add("2As", 41.24);
            dict.Add("9K", 41.13);
            dict.Add("TJ", 41.05);
            dict.Add("9Ts", 40.94);
            dict.Add("55", 40.73);
            dict.Add("5A", 40.69);
            dict.Add("8Qs", 40.65);
            dict.Add("6Ks", 40.37);
            dict.Add("6A", 40);
            dict.Add("5Ks", 39.77);
            dict.Add("4A", 39.69);
            dict.Add("8Js", 39.61);
            dict.Add("9Q", 39.59);
            dict.Add("8K", 39.02);
            dict.Add("8Ts", 38.99);
            dict.Add("4Ks", 38.89);
            dict.Add("3A", 38.77);
            dict.Add("7Qs", 38.72);
            dict.Add("9J", 38.52);
            dict.Add("89s", 38.31);
            dict.Add("7K", 38.21);
            dict.Add("3Ks", 38.05);
            dict.Add("6Qs", 37.92);
            dict.Add("9T", 37.91);
            dict.Add("2A", 37.85);
            dict.Add("7Js", 37.68);
            dict.Add("8Q", 37.48);
            dict.Add("44", 37.47);
            dict.Add("5Qs", 37.34);
            dict.Add("2Ks", 37.23);
            dict.Add("6K", 37.09);
            dict.Add("7Ts", 37.07);
            dict.Add("79s", 36.52);
            dict.Add("4Qs", 36.48);
            dict.Add("5K", 36.44);
            dict.Add("8J", 36.43);
            dict.Add("78s", 36.26);
            dict.Add("8T", 35.82);
            dict.Add("3Qs", 35.65);
            dict.Add("6Js", 35.64);
            dict.Add("4K", 35.48);
            dict.Add("7Q", 35.4);
            dict.Add("5Js", 35.28);
            dict.Add("89", 35.14);
            dict.Add("6Ts", 35.02);
            dict.Add("2Qs", 34.81);
            dict.Add("3K", 34.55);
            dict.Add("6Q", 34.54);
            dict.Add("69s", 34.49);
            dict.Add("4Js", 34.43);
            dict.Add("7J", 34.38);
            dict.Add("33", 34.36);
            dict.Add("68s", 34.29);
            dict.Add("67s", 34.13);
            dict.Add("5Q", 33.9);
            dict.Add("7T", 33.75);
            dict.Add("3Js", 33.64);
            dict.Add("2K", 33.64);
            dict.Add("5Ts", 33.37);
            dict.Add("79", 33.21);
            dict.Add("78", 32.96);
            dict.Add("4Q", 32.93);
            dict.Add("59s", 32.84);
            dict.Add("2Js", 32.82);
            dict.Add("4Ts", 32.78);
            dict.Add("57s", 32.72);
            dict.Add("58s", 32.67);
            dict.Add("56s", 32.58);
            dict.Add("6J", 32.15);
            dict.Add("3Q", 32.04);
            dict.Add("3Ts", 31.98);
            dict.Add("5J", 31.78);
            dict.Add("45s", 31.62);
            dict.Add("6T", 31.54);
            dict.Add("22", 31.46);
            dict.Add("2Ts", 31.17);
            dict.Add("2Q", 31.16);
            dict.Add("69", 31.02);
            dict.Add("49s", 30.95);
            dict.Add("4J", 30.85);
            dict.Add("47s", 30.85);
            dict.Add("68", 30.84);
            dict.Add("46s", 30.78);
            dict.Add("48s", 30.76);
            dict.Add("67", 30.68);
            dict.Add("39s", 30.38);
            dict.Add("3J", 29.96);
            dict.Add("35s", 29.83);
            dict.Add("5T", 29.79);
            dict.Add("29s", 29.6);
            dict.Add("59", 29.25);
            dict.Add("57", 29.2);
            dict.Add("4T", 29.14);
            dict.Add("58", 29.12);
            dict.Add("2J", 29.09);
            dict.Add("56", 29.06);
            dict.Add("37s", 28.95);
            dict.Add("34s", 28.94);
            dict.Add("36s", 28.91);
            dict.Add("38s", 28.9);
            dict.Add("28s", 28.37);
            dict.Add("3T", 28.24);
            dict.Add("45", 28.03);
            dict.Add("25s", 27.97);
            dict.Add("2T", 27.38);
            dict.Add("49", 27.21);
            dict.Add("47", 27.16);
            dict.Add("24s", 27.16);
            dict.Add("46", 27.14);
            dict.Add("27s", 27.09);
            dict.Add("48", 27.06);
            dict.Add("26s", 27.06);
            dict.Add("39", 26.6);
            dict.Add("23s", 26.3);
            dict.Add("35", 26.11);
            dict.Add("29", 25.75);
            dict.Add("34", 25.16);
            dict.Add("36", 25.13);
            dict.Add("37", 25.12);
            dict.Add("38", 25.05);
            dict.Add("28", 24.46);
            dict.Add("25", 24.12);
            dict.Add("24", 23.25);
            dict.Add("27", 23.12);
            dict.Add("26", 23.1);
            dict.Add("23", 22.34);

            return dict;
        }

        private static Dictionary<string, double> GetInitialProbabilities4()
        {
            var dict = new Dictionary<string, double>();
            dict.Add("AA", 64.35);
            dict.Add("KK", 58.63);
            dict.Add("QQ", 53.95);
            dict.Add("JJ", 49.67);
            dict.Add("TT", 45.74);
            dict.Add("KAs", 42.68);
            dict.Add("99", 41.67);
            dict.Add("QAs", 41.27);
            dict.Add("JAs", 40.05);
            dict.Add("KA", 39.83);
            dict.Add("QKs", 39.58);
            dict.Add("TAs", 38.99);
            dict.Add("JKs", 38.37);
            dict.Add("QA", 38.31);
            dict.Add("88", 38.14);
            dict.Add("TKs", 37.34);
            dict.Add("JQs", 37.34);
            dict.Add("JA", 36.98);
            dict.Add("QK", 36.62);
            dict.Add("9As", 36.42);
            dict.Add("TQs", 36.32);
            dict.Add("TA", 35.82);
            dict.Add("TJs", 35.73);
            dict.Add("8As", 35.56);
            dict.Add("JK", 35.29);
            dict.Add("77", 34.93);
            dict.Add("9Ks", 34.76);
            dict.Add("7As", 34.65);
            dict.Add("JQ", 34.28);
            dict.Add("5As", 34.18);
            dict.Add("TK", 34.16);
            dict.Add("9Qs", 33.73);
            dict.Add("4As", 33.38);
            dict.Add("6As", 33.26);
            dict.Add("9Js", 33.19);
            dict.Add("TQ", 33.17);
            dict.Add("9A", 33.05);
            dict.Add("9Ts", 32.98);
            dict.Add("8Ks", 32.72);
            dict.Add("3As", 32.65);
            dict.Add("TJ", 32.64);
            dict.Add("8A", 32.1);
            dict.Add("7Ks", 32.04);
            dict.Add("2As", 31.94);
            dict.Add("66", 31.84);
            dict.Add("8Qs", 31.72);
            dict.Add("9K", 31.37);
            dict.Add("8Js", 31.15);
            dict.Add("7A", 31.09);
            dict.Add("8Ts", 30.98);
            dict.Add("6Ks", 30.98);
            dict.Add("5A", 30.55);
            dict.Add("89s", 30.53);
            dict.Add("5Ks", 30.53);
            dict.Add("9Q", 30.39);
            dict.Add("9J", 29.86);
            dict.Add("7Qs", 29.8);
            dict.Add("4Ks", 29.76);
            dict.Add("9T", 29.72);
            dict.Add("4A", 29.67);
            dict.Add("6A", 29.58);
            dict.Add("55", 29.5);
            dict.Add("7Js", 29.24);
            dict.Add("8K", 29.19);
            dict.Add("3Ks", 29.05);
            dict.Add("7Ts", 29.04);
            dict.Add("6Qs", 28.99);
            dict.Add("3A", 28.86);
            dict.Add("79s", 28.78);
            dict.Add("78s", 28.76);
            dict.Add("5Qs", 28.54);
            dict.Add("7K", 28.4);
            dict.Add("2Ks", 28.38);
            dict.Add("8Q", 28.19);
            dict.Add("2A", 28.08);
            dict.Add("4Qs", 27.8);
            dict.Add("8J", 27.68);
            dict.Add("8T", 27.56);
            dict.Add("6K", 27.27);
            dict.Add("6Js", 27.21);
            dict.Add("89", 27.15);
            dict.Add("3Qs", 27.09);
            dict.Add("6Ts", 26.99);
            dict.Add("5Js", 26.96);
            dict.Add("44", 26.91);
            dict.Add("68s", 26.83);
            dict.Add("67s", 26.78);
            dict.Add("5K", 26.76);
            dict.Add("69s", 26.75);
            dict.Add("2Qs", 26.42);
            dict.Add("4Js", 26.23);
            dict.Add("7Q", 26.13);
            dict.Add("4K", 25.93);
            dict.Add("57s", 25.62);
            dict.Add("7J", 25.61);
            dict.Add("3Js", 25.54);
            dict.Add("5Ts", 25.53);
            dict.Add("56s", 25.5);
            dict.Add("7T", 25.48);
            dict.Add("58s", 25.38);
            dict.Add("78", 25.26);
            dict.Add("59s", 25.25);
            dict.Add("79", 25.24);
            dict.Add("6Q", 25.24);
            dict.Add("3K", 25.12);
            dict.Add("4Ts", 24.99);
            dict.Add("2Js", 24.87);
            dict.Add("45s", 24.84);
            dict.Add("5Q", 24.73);
            dict.Add("33", 24.59);
            dict.Add("2K", 24.37);
            dict.Add("3Ts", 24.32);
            dict.Add("4Q", 23.91);
            dict.Add("47s", 23.89);
            dict.Add("46s", 23.88);
            dict.Add("2Ts", 23.66);
            dict.Add("48s", 23.6);
            dict.Add("49s", 23.54);
            dict.Add("6J", 23.42);
            dict.Add("6T", 23.26);
            dict.Add("35s", 23.25);
            dict.Add("68", 23.16);
            dict.Add("67", 23.14);
            dict.Add("3Q", 23.14);
            dict.Add("5J", 23.12);
            dict.Add("69", 23.06);
            dict.Add("39s", 23.04);
            dict.Add("22", 22.58);
            dict.Add("34s", 22.42);
            dict.Add("29s", 22.41);
            dict.Add("2Q", 22.38);
            dict.Add("4J", 22.33);
            dict.Add("36s", 22.17);
            dict.Add("37s", 22.14);
            dict.Add("57", 21.93);
            dict.Add("38s", 21.9);
            dict.Add("56", 21.81);
            dict.Add("5T", 21.67);
            dict.Add("58", 21.62);
            dict.Add("25s", 21.57);
            dict.Add("3J", 21.56);
            dict.Add("28s", 21.47);
            dict.Add("59", 21.43);
            dict.Add("45", 21.09);
            dict.Add("4T", 21.08);
            dict.Add("24s", 20.88);
            dict.Add("2J", 20.83);
            dict.Add("27s", 20.5);
            dict.Add("26s", 20.48);
            dict.Add("3T", 20.31);
            dict.Add("23s", 20.1);
            dict.Add("46", 20.05);
            dict.Add("47", 20.02);
            dict.Add("48", 19.71);
            dict.Add("2T", 19.61);
            dict.Add("49", 19.58);
            dict.Add("35", 19.38);
            dict.Add("39", 19.02);
            dict.Add("34", 18.48);
            dict.Add("29", 18.31);
            dict.Add("36", 18.21);
            dict.Add("37", 18.14);
            dict.Add("38", 17.88);
            dict.Add("25", 17.56);
            dict.Add("28", 17.36);
            dict.Add("24", 16.81);
            dict.Add("27", 16.37);
            dict.Add("26", 16.37);
            dict.Add("23", 15.99);

            return dict;
        }

        private static Dictionary<string, double> GetInitialProbabilities5()
        {
            var dict = new Dictionary<string, double>();

            dict.Add("AA", 56.43);
            dict.Add("KK", 50.18);
            dict.Add("QQ", 45.18);
            dict.Add("JJ", 40.78);
            dict.Add("TT", 36.9);
            dict.Add("KAs", 36.69);
            dict.Add("QAs", 35.16);
            dict.Add("JAs", 33.89);
            dict.Add("QKs", 33.85);
            dict.Add("KA", 33.65);
            dict.Add("99", 33.12);
            dict.Add("TAs", 32.85);
            dict.Add("JKs", 32.59);
            dict.Add("QA", 31.98);
            dict.Add("JQs", 31.8);
            dict.Add("TKs", 31.56);
            dict.Add("TQs", 30.79);
            dict.Add("QK", 30.69);
            dict.Add("JA", 30.58);
            dict.Add("TJs", 30.43);
            dict.Add("9As", 30.25);
            dict.Add("88", 30.02);
            dict.Add("8As", 29.44);
            dict.Add("TA", 29.41);
            dict.Add("JK", 29.31);
            dict.Add("9Ks", 28.93);
            dict.Add("7As", 28.63);
            dict.Add("JQ", 28.58);
            dict.Add("5As", 28.41);
            dict.Add("TK", 28.18);
            dict.Add("9Qs", 28.18);
            dict.Add("9Js", 27.85);
            dict.Add("9Ts", 27.83);
            dict.Add("4As", 27.74);
            dict.Add("TQ", 27.46);
            dict.Add("6As", 27.33);
            dict.Add("77", 27.31);
            dict.Add("TJ", 27.2);
            dict.Add("3As", 27.12);
            dict.Add("8Ks", 27.02);
            dict.Add("9A", 26.63);
            dict.Add("2As", 26.57);
            dict.Add("7Ks", 26.38);
            dict.Add("8Qs", 26.24);
            dict.Add("8Ts", 25.92);
            dict.Add("8Js", 25.91);
            dict.Add("8A", 25.72);
            dict.Add("89s", 25.53);
            dict.Add("6Ks", 25.41);
            dict.Add("9K", 25.34);
            dict.Add("5Ks", 25.11);
            dict.Add("7A", 24.8);
            dict.Add("66", 24.74);
            dict.Add("9Q", 24.65);
            dict.Add("5A", 24.52);
            dict.Add("7Qs", 24.49);
            dict.Add("9T", 24.46);
            dict.Add("4Ks", 24.46);
            dict.Add("9J", 24.39);
            dict.Add("7Ts", 24.12);
            dict.Add("7Js", 24.12);
            dict.Add("78s", 23.99);
            dict.Add("79s", 23.95);
            dict.Add("3Ks", 23.86);
            dict.Add("4A", 23.78);
            dict.Add("6Qs", 23.69);
            dict.Add("5Qs", 23.4);
            dict.Add("6A", 23.38);
            dict.Add("2Ks", 23.31);
            dict.Add("8K", 23.25);
            dict.Add("3A", 23.09);
            dict.Add("55", 23.01);
            dict.Add("4Qs", 22.79);
            dict.Add("8Q", 22.53);
            dict.Add("7K", 22.52);
            dict.Add("2A", 22.47);
            dict.Add("8T", 22.4);
            dict.Add("8J", 22.3);
            dict.Add("6Js", 22.23);
            dict.Add("3Qs", 22.2);
            dict.Add("6Ts", 22.18);
            dict.Add("68s", 22.18);
            dict.Add("67s", 22.17);
            dict.Add("5Js", 22.09);
            dict.Add("89", 22.03);
            dict.Add("69s", 22.03);
            dict.Add("2Qs", 21.66);
            dict.Add("6K", 21.47);
            dict.Add("4Js", 21.47);
            dict.Add("57s", 21.32);
            dict.Add("56s", 21.17);
            dict.Add("44", 21.14);
            dict.Add("5K", 21.12);
            dict.Add("58s", 20.99);
            dict.Add("5Ts", 20.96);
            dict.Add("3Js", 20.92);
            dict.Add("45s", 20.79);
            dict.Add("59s", 20.75);
            dict.Add("7Q", 20.63);
            dict.Add("4Ts", 20.48);
            dict.Add("7T", 20.44);
            dict.Add("78", 20.41);
            dict.Add("4K", 20.41);
            dict.Add("2Js", 20.38);
            dict.Add("7J", 20.35);
            dict.Add("79", 20.3);
            dict.Add("3Ts", 19.92);
            dict.Add("6Q", 19.76);
            dict.Add("47s", 19.74);
            dict.Add("3K", 19.74);
            dict.Add("46s", 19.73);
            dict.Add("33", 19.56);
            dict.Add("5Q", 19.41);
            dict.Add("35s", 19.41);
            dict.Add("2Ts", 19.39);
            dict.Add("48s", 19.37);
            dict.Add("49s", 19.23);
            dict.Add("2K", 19.14);
            dict.Add("39s", 18.79);
            dict.Add("4Q", 18.73);
            dict.Add("34s", 18.66);
            dict.Add("67", 18.45);
            dict.Add("68", 18.44);
            dict.Add("6T", 18.33);
            dict.Add("6J", 18.29);
            dict.Add("29s", 18.29);
            dict.Add("22", 18.29);
            dict.Add("69", 18.22);
            dict.Add("36s", 18.19);
            dict.Add("37s", 18.18);
            dict.Add("5J", 18.12);
            dict.Add("3Q", 18.08);
            dict.Add("25s", 17.92);
            dict.Add("38s", 17.89);
            dict.Add("57", 17.55);
            dict.Add("28s", 17.52);
            dict.Add("2Q", 17.47);
            dict.Add("4J", 17.45);
            dict.Add("56", 17.4);
            dict.Add("24s", 17.33);
            dict.Add("58", 17.14);
            dict.Add("45", 17.01);
            dict.Add("5T", 17);
            dict.Add("59", 16.83);
            dict.Add("3J", 16.8);
            dict.Add("27s", 16.74);
            dict.Add("26s", 16.7);
            dict.Add("23s", 16.65);
            dict.Add("4T", 16.48);
            dict.Add("2J", 16.22);
            dict.Add("46", 15.86);
            dict.Add("47", 15.84);
            dict.Add("3T", 15.84);
            dict.Add("35", 15.51);
            dict.Add("48", 15.41);
            dict.Add("2T", 15.26);
            dict.Add("49", 15.18);
            dict.Add("34", 14.71);
            dict.Add("39", 14.69);
            dict.Add("36", 14.19);
            dict.Add("37", 14.14);
            dict.Add("29", 14.12);
            dict.Add("25", 13.89);
            dict.Add("38", 13.8);
            dict.Add("28", 13.37);
            dict.Add("24", 13.27);
            dict.Add("27", 12.59);
            dict.Add("26", 12.58);
            dict.Add("23", 12.54);

            return dict;
        }

        private static Dictionary<string, double> GetInitialProbabilities6()
        {
            var dict = new Dictionary<string, double>();
            dict.Add("AA", 49.77);
            dict.Add("KK", 43.36);
            dict.Add("QQ", 38.36);
            dict.Add("JJ", 34.1);
            dict.Add("KAs", 32.37);
            dict.Add("QAs", 30.81);
            dict.Add("TT", 30.51);
            dict.Add("QKs", 29.68);
            dict.Add("JAs", 29.56);
            dict.Add("KA", 29.21);
            dict.Add("TAs", 28.56);
            dict.Add("JKs", 28.45);
            dict.Add("JQs", 27.78);
            dict.Add("TKs", 27.49);
            dict.Add("QA", 27.48);
            dict.Add("99", 27.17);
            dict.Add("TQs", 26.86);
            dict.Add("TJs", 26.62);
            dict.Add("QK", 26.42);
            dict.Add("JA", 26.09);
            dict.Add("9As", 26.06);
            dict.Add("8As", 25.31);
            dict.Add("JK", 25.05);
            dict.Add("TA", 24.97);
            dict.Add("9Ks", 24.93);
            dict.Add("7As", 24.6);
            dict.Add("5As", 24.59);
            dict.Add("88", 24.57);
            dict.Add("JQ", 24.49);
            dict.Add("9Qs", 24.31);
            dict.Add("9Ts", 24.19);
            dict.Add("9Js", 24.1);
            dict.Add("4As", 24.02);
            dict.Add("TK", 23.97);
            dict.Add("3As", 23.51);
            dict.Add("TQ", 23.43);
            dict.Add("6As", 23.4);
            dict.Add("TJ", 23.3);
            dict.Add("8Ks", 23.16);
            dict.Add("2As", 23.06);
            dict.Add("7Ks", 22.59);
            dict.Add("8Qs", 22.53);
            dict.Add("8Ts", 22.43);
            dict.Add("77", 22.39);
            dict.Add("8Js", 22.31);
            dict.Add("9A", 22.25);
            dict.Add("89s", 22.03);
            dict.Add("6Ks", 21.7);
            dict.Add("5Ks", 21.55);
            dict.Add("8A", 21.42);
            dict.Add("9K", 21.19);
            dict.Add("4Ks", 21);
            dict.Add("7Qs", 20.94);
            dict.Add("7Ts", 20.77);
            dict.Add("9T", 20.75);
            dict.Add("78s", 20.73);
            dict.Add("9Q", 20.67);
            dict.Add("7Js", 20.66);
            dict.Add("79s", 20.64);
            dict.Add("7A", 20.62);
            dict.Add("9J", 20.56);
            dict.Add("5A", 20.55);
            dict.Add("3Ks", 20.5);
            dict.Add("66", 20.31);
            dict.Add("6Qs", 20.17);
            dict.Add("2Ks", 20.05);
            dict.Add("5Qs", 20.01);
            dict.Add("4A", 19.93);
            dict.Add("4Qs", 19.5);
            dict.Add("3A", 19.35);
            dict.Add("6A", 19.3);
            dict.Add("8K", 19.27);
            dict.Add("55", 19.1);
            dict.Add("68s", 19.08);
            dict.Add("67s", 19.06);
            dict.Add("3Qs", 19.02);
            dict.Add("6Ts", 18.96);
            dict.Add("6Js", 18.94);
            dict.Add("5Js", 18.88);
            dict.Add("69s", 18.86);
            dict.Add("2A", 18.85);
            dict.Add("8T", 18.83);
            dict.Add("8Q", 18.7);
            dict.Add("8J", 18.61);
            dict.Add("7K", 18.61);
            dict.Add("2Qs", 18.59);
            dict.Add("89", 18.48);
            dict.Add("57s", 18.48);
            dict.Add("4Js", 18.38);
            dict.Add("56s", 18.31);
            dict.Add("45s", 18.19);
            dict.Add("58s", 18.08);
            dict.Add("5Ts", 17.95);
            dict.Add("3Js", 17.91);
            dict.Add("44", 17.79);
            dict.Add("59s", 17.76);
            dict.Add("6K", 17.63);
            dict.Add("4Ts", 17.54);
            dict.Add("2Js", 17.48);
            dict.Add("5K", 17.44);
            dict.Add("78", 17.11);
            dict.Add("3Ts", 17.07);
            dict.Add("47s", 17.06);
            dict.Add("46s", 17.04);
            dict.Add("7T", 17.03);
            dict.Add("7Q", 16.98);
            dict.Add("35s", 16.98);
            dict.Add("79", 16.96);
            dict.Add("4K", 16.84);
            dict.Add("7J", 16.82);
            dict.Add("33", 16.74);
            dict.Add("2Ts", 16.65);
            dict.Add("48s", 16.62);
            dict.Add("49s", 16.41);
            dict.Add("34s", 16.29);
            dict.Add("3K", 16.28);
            dict.Add("6Q", 16.14);
            dict.Add("39s", 16.03);
            dict.Add("22", 15.98);
            dict.Add("5Q", 15.96);
            dict.Add("2K", 15.8);
            dict.Add("36s", 15.64);
            dict.Add("37s", 15.63);
            dict.Add("29s", 15.63);
            dict.Add("25s", 15.63);
            dict.Add("4Q", 15.37);
            dict.Add("67", 15.32);
            dict.Add("68", 15.3);
            dict.Add("38s", 15.3);
            dict.Add("24s", 15.12);
            dict.Add("6T", 15.07);
            dict.Add("69", 15.02);
            dict.Add("28s", 14.99);
            dict.Add("6J", 14.93);
            dict.Add("5J", 14.86);
            dict.Add("3Q", 14.83);
            dict.Add("57", 14.71);
            dict.Add("56", 14.54);
            dict.Add("23s", 14.53);
            dict.Add("45", 14.43);
            dict.Add("27s", 14.37);
            dict.Add("2Q", 14.35);
            dict.Add("26s", 14.31);
            dict.Add("4J", 14.3);
            dict.Add("58", 14.23);
            dict.Add("5T", 13.97);
            dict.Add("59", 13.84);
            dict.Add("3J", 13.76);
            dict.Add("4T", 13.5);
            dict.Add("2J", 13.29);
            dict.Add("46", 13.17);
            dict.Add("47", 13.15);
            dict.Add("35", 13.11);
            dict.Add("3T", 12.97);
            dict.Add("48", 12.65);
            dict.Add("2T", 12.52);
            dict.Add("34", 12.38);
            dict.Add("49", 12.36);
            dict.Add("39", 11.94);
            dict.Add("36", 11.67);
            dict.Add("25", 11.66);
            dict.Add("37", 11.61);
            dict.Add("29", 11.48);
            dict.Add("38", 11.23);
            dict.Add("24", 11.13);
            dict.Add("28", 10.86);
            dict.Add("23", 10.47);
            dict.Add("27", 10.27);
            dict.Add("26", 10.22);

            return dict;
        }

        private static Dictionary<string, double> GetInitialProbabilities7()
        {
            var dict = new Dictionary<string, double>();
            dict.Add("AA", 44.13);
            dict.Add("KK", 37.83);
            dict.Add("QQ", 33.01);
            dict.Add("JJ", 29.05);
            dict.Add("KAs", 29.01);
            dict.Add("QAs", 27.47);
            dict.Add("QKs", 26.44);
            dict.Add("JAs", 26.29);
            dict.Add("TT", 25.83);
            dict.Add("KA", 25.77);
            dict.Add("TAs", 25.38);
            dict.Add("JKs", 25.3);
            dict.Add("JQs", 24.72);
            dict.Add("TKs", 24.43);
            dict.Add("QA", 24.05);
            dict.Add("TQs", 23.88);
            dict.Add("TJs", 23.72);
            dict.Add("QK", 23.11);
            dict.Add("9As", 23);
            dict.Add("99", 22.96);
            dict.Add("JA", 22.71);
            dict.Add("8As", 22.32);
            dict.Add("9Ks", 21.97);
            dict.Add("5As", 21.84);
            dict.Add("JK", 21.8);
            dict.Add("7As", 21.71);
            dict.Add("TA", 21.66);
            dict.Add("9Ts", 21.49);
            dict.Add("9Qs", 21.45);
            dict.Add("4As", 21.37);
            dict.Add("JQ", 21.35);
            dict.Add("9Js", 21.32);
            dict.Add("3As", 20.93);
            dict.Add("88", 20.85);
            dict.Add("TK", 20.81);
            dict.Add("6As", 20.61);
            dict.Add("2As", 20.57);
            dict.Add("TQ", 20.38);
            dict.Add("8Ks", 20.37);
            dict.Add("TJ", 20.36);
            dict.Add("8Ts", 19.87);
            dict.Add("7Ks", 19.86);
            dict.Add("8Qs", 19.81);
            dict.Add("8Js", 19.68);
            dict.Add("89s", 19.5);
            dict.Add("77", 19.13);
            dict.Add("9A", 19.09);
            dict.Add("6Ks", 19.05);
            dict.Add("5Ks", 18.99);
            dict.Add("4Ks", 18.53);
            dict.Add("78s", 18.41);
            dict.Add("7Qs", 18.38);
            dict.Add("7Ts", 18.37);
            dict.Add("8A", 18.31);
            dict.Add("79s", 18.27);
            dict.Add("7Js", 18.19);
            dict.Add("9K", 18.14);
            dict.Add("3Ks", 18.12);
            dict.Add("9T", 18.01);
            dict.Add("2Ks", 17.75);
            dict.Add("9J", 17.74);
            dict.Add("9Q", 17.73);
            dict.Add("5A", 17.73);
            dict.Add("6Qs", 17.67);
            dict.Add("5Qs", 17.63);
            dict.Add("7A", 17.62);
            dict.Add("66", 17.44);
            dict.Add("4Qs", 17.19);
            dict.Add("4A", 17.19);
            dict.Add("68s", 16.87);
            dict.Add("67s", 16.87);
            dict.Add("3Qs", 16.77);
            dict.Add("3A", 16.71);
            dict.Add("6Ts", 16.68);
            dict.Add("55", 16.63);
            dict.Add("5Js", 16.62);
            dict.Add("69s", 16.6);
            dict.Add("6Js", 16.59);
            dict.Add("57s", 16.51);
            dict.Add("6A", 16.42);
            dict.Add("45s", 16.42);
            dict.Add("2Qs", 16.42);
            dict.Add("8K", 16.39);
            dict.Add("56s", 16.32);
            dict.Add("2A", 16.3);
            dict.Add("8T", 16.25);
            dict.Add("4Js", 16.2);
            dict.Add("58s", 16.05);
            dict.Add("8J", 15.93);
            dict.Add("89", 15.93);
            dict.Add("8Q", 15.92);
            dict.Add("5Ts", 15.85);
            dict.Add("7K", 15.81);
            dict.Add("3Js", 15.8);
            dict.Add("44", 15.73);
            dict.Add("59s", 15.67);
            dict.Add("4Ts", 15.47);
            dict.Add("2Js", 15.45);
            dict.Add("35s", 15.31);
            dict.Add("47s", 15.2);
            dict.Add("46s", 15.18);
            dict.Add("3Ts", 15.08);
            dict.Add("33", 15.03);
            dict.Add("6K", 14.9);
            dict.Add("5K", 14.84);
            dict.Add("78", 14.8);
            dict.Add("2Ts", 14.74);
            dict.Add("48s", 14.7);
            dict.Add("34s", 14.67);
            dict.Add("7T", 14.61);
            dict.Add("79", 14.58);
            dict.Add("22", 14.58);
            dict.Add("49s", 14.45);
            dict.Add("7Q", 14.39);
            dict.Add("4K", 14.33);
            dict.Add("7J", 14.3);
            dict.Add("39s", 14.11);
            dict.Add("25s", 14.07);
            dict.Add("37s", 13.88);
            dict.Add("36s", 13.88);
            dict.Add("3K", 13.86);
            dict.Add("29s", 13.79);
            dict.Add("24s", 13.62);
            dict.Add("6Q", 13.58);
            dict.Add("5Q", 13.53);
            dict.Add("38s", 13.52);
            dict.Add("2K", 13.46);
            dict.Add("28s", 13.25);
            dict.Add("67", 13.16);
            dict.Add("68", 13.12);
            dict.Add("23s", 13.07);
            dict.Add("4Q", 13.04);
            dict.Add("57", 12.79);
            dict.Add("6T", 12.78);
            dict.Add("69", 12.78);
            dict.Add("27s", 12.75);
            dict.Add("45", 12.71);
            dict.Add("26s", 12.65);
            dict.Add("5J", 12.6);
            dict.Add("56", 12.59);
            dict.Add("3Q", 12.58);
            dict.Add("6J", 12.57);
            dict.Add("58", 12.23);
            dict.Add("2Q", 12.19);
            dict.Add("4J", 12.11);
            dict.Add("5T", 11.88);
            dict.Add("59", 11.76);
            dict.Add("3J", 11.67);
            dict.Add("35", 11.52);
            dict.Add("4T", 11.45);
            dict.Add("46", 11.37);
            dict.Add("47", 11.35);
            dict.Add("2J", 11.28);
            dict.Add("3T", 11.02);
            dict.Add("34", 10.84);
            dict.Add("48", 10.78);
            dict.Add("2T", 10.65);
            dict.Add("49", 10.45);
            dict.Add("25", 10.19);
            dict.Add("39", 10.06);
            dict.Add("36", 9.98);
            dict.Add("37", 9.93);
            dict.Add("24", 9.72);
            dict.Add("29", 9.71);
            dict.Add("38", 9.51);
            dict.Add("28", 9.19);
            dict.Add("23", 9.14);
            dict.Add("27", 8.73);
            dict.Add("26", 8.66);

            return dict;
        }

        private static Dictionary<string, double> GetInitialProbabilities8()
        {
            var dict = new Dictionary<string, double>();
            dict.Add("AA", 39.32);
            dict.Add("KK", 33.3);
            dict.Add("QQ", 28.77);
            dict.Add("KAs", 26.28);
            dict.Add("JJ", 25.2);
            dict.Add("QAs", 24.81);
            dict.Add("QKs", 23.84);
            dict.Add("JAs", 23.71);
            dict.Add("KA", 22.96);
            dict.Add("TAs", 22.89);
            dict.Add("JKs", 22.79);
            dict.Add("TT", 22.37);
            dict.Add("JQs", 22.28);
            dict.Add("TKs", 22.03);
            dict.Add("TQs", 21.57);
            dict.Add("TJs", 21.48);
            dict.Add("QA", 21.29);
            dict.Add("9As", 20.64);
            dict.Add("QK", 20.43);
            dict.Add("JA", 20.04);
            dict.Add("8As", 20.03);
            dict.Add("99", 19.96);
            dict.Add("5As", 19.76);
            dict.Add("9Ks", 19.69);
            dict.Add("7As", 19.5);
            dict.Add("9Ts", 19.45);
            dict.Add("4As", 19.34);
            dict.Add("9Qs", 19.25);
            dict.Add("JK", 19.24);
            dict.Add("9Js", 19.2);
            dict.Add("TA", 19.09);
            dict.Add("3As", 18.97);
            dict.Add("JQ", 18.86);
            dict.Add("2As", 18.67);
            dict.Add("6As", 18.49);
            dict.Add("TK", 18.35);
            dict.Add("8Ks", 18.25);
            dict.Add("88", 18.25);
            dict.Add("TJ", 18.08);
            dict.Add("TQ", 18.02);
            dict.Add("8Ts", 17.96);
            dict.Add("7Ks", 17.79);
            dict.Add("8Qs", 17.74);
            dict.Add("8Js", 17.69);
            dict.Add("89s", 17.6);
            dict.Add("5Ks", 17.08);
            dict.Add("6Ks", 17.05);
            dict.Add("77", 16.92);
            dict.Add("78s", 16.7);
            dict.Add("4Ks", 16.68);
            dict.Add("9A", 16.66);
            dict.Add("7Ts", 16.58);
            dict.Add("79s", 16.52);
            dict.Add("7Qs", 16.47);
            dict.Add("7Js", 16.33);
            dict.Add("3Ks", 16.32);
            dict.Add("2Ks", 16.01);
            dict.Add("9T", 15.96);
            dict.Add("8A", 15.95);
            dict.Add("5Qs", 15.83);
            dict.Add("9K", 15.8);
            dict.Add("6Qs", 15.78);
            dict.Add("5A", 15.59);
            dict.Add("9J", 15.58);
            dict.Add("66", 15.51);
            dict.Add("9Q", 15.49);
            dict.Add("4Qs", 15.46);
            dict.Add("7A", 15.35);
            dict.Add("68s", 15.26);
            dict.Add("67s", 15.26);
            dict.Add("4A", 15.13);
            dict.Add("45s", 15.11);
            dict.Add("3Qs", 15.1);
            dict.Add("57s", 15.07);
            dict.Add("6Ts", 14.99);
            dict.Add("55", 14.98);
            dict.Add("5Js", 14.95);
            dict.Add("69s", 14.94);
            dict.Add("56s", 14.86);
            dict.Add("6Js", 14.85);
            dict.Add("2Qs", 14.82);
            dict.Add("3A", 14.71);
            dict.Add("4Js", 14.57);
            dict.Add("58s", 14.56);
            dict.Add("2A", 14.38);
            dict.Add("44", 14.36);
            dict.Add("8T", 14.34);
            dict.Add("5Ts", 14.29);
            dict.Add("6A", 14.24);
            dict.Add("3Js", 14.24);
            dict.Add("8K", 14.22);
            dict.Add("59s", 14.14);
            dict.Add("35s", 14.09);
            dict.Add("89", 14.04);
            dict.Add("4Ts", 13.95);
            dict.Add("2Js", 13.95);
            dict.Add("8J", 13.93);
            dict.Add("33", 13.9);
            dict.Add("8Q", 13.84);
            dict.Add("47s", 13.83);
            dict.Add("46s", 13.8);
            dict.Add("7K", 13.7);
            dict.Add("22", 13.62);
            dict.Add("3Ts", 13.61);
            dict.Add("34s", 13.48);
            dict.Add("2Ts", 13.34);
            dict.Add("48s", 13.3);
            dict.Add("78", 13.12);
            dict.Add("49s", 13.02);
            dict.Add("25s", 12.93);
            dict.Add("5K", 12.9);
            dict.Add("6K", 12.87);
            dict.Add("79", 12.85);
            dict.Add("7T", 12.84);
            dict.Add("39s", 12.72);
            dict.Add("37s", 12.61);
            dict.Add("36s", 12.6);
            dict.Add("24s", 12.53);
            dict.Add("7Q", 12.46);
            dict.Add("4K", 12.45);
            dict.Add("7J", 12.44);
            dict.Add("29s", 12.44);
            dict.Add("38s", 12.21);
            dict.Add("3K", 12.05);
            dict.Add("23s", 12.02);
            dict.Add("28s", 11.97);
            dict.Add("5Q", 11.74);
            dict.Add("2K", 11.72);
            dict.Add("6Q", 11.7);
            dict.Add("67", 11.6);
            dict.Add("27s", 11.58);
            dict.Add("68", 11.55);
            dict.Add("45", 11.5);
            dict.Add("26s", 11.45);
            dict.Add("57", 11.41);
            dict.Add("4Q", 11.31);
            dict.Add("56", 11.21);
            dict.Add("69", 11.15);
            dict.Add("6T", 11.11);
            dict.Add("5J", 10.94);
            dict.Add("3Q", 10.93);
            dict.Add("6J", 10.85);
            dict.Add("58", 10.81);
            dict.Add("2Q", 10.6);
            dict.Add("4J", 10.51);
            dict.Add("35", 10.4);
            dict.Add("5T", 10.37);
            dict.Add("59", 10.29);
            dict.Add("3J", 10.14);
            dict.Add("47", 10.08);
            dict.Add("46", 10.08);
            dict.Add("4T", 9.98);
            dict.Add("2J", 9.82);
            dict.Add("34", 9.76);
            dict.Add("3T", 9.6);
            dict.Add("48", 9.46);
            dict.Add("2T", 9.3);
            dict.Add("25", 9.16);
            dict.Add("49", 9.08);
            dict.Add("36", 8.79);
            dict.Add("37", 8.76);
            dict.Add("24", 8.75);
            dict.Add("39", 8.74);
            dict.Add("29", 8.43);
            dict.Add("38", 8.29);
            dict.Add("23", 8.19);
            dict.Add("28", 8.02);
            dict.Add("27", 7.66);
            dict.Add("26", 7.57);

            return dict;
        }

        private static Tournament CreateTournament(Game game)
        {
            var tournament = new Tournament();
            
            tournament.Round = game.round;

            if (game.community_cards != null)
            {
                tournament.CommunityCards = game.community_cards.Select(c => new Card {Color = c.suit, Rank = StringToRankMapper(c.rank)}).ToList();
            }
            else
            {
                tournament.CommunityCards = new List<Card>();
            }
            
            tournament.OtherPlayers = game.players.Where(p => p.name != "Royal Flush").Select(PlayerMapper).ToList();
            tournament.CurrentBuyIn = game.current_buy_in;

            var ourPlayer = game.players.SingleOrDefault(p => p.name == "Royal Flush");
            if (ourPlayer != null)
            {
                tournament.OurPlayer = PlayerMapper( ourPlayer);
            }

            return tournament;
        }

        public static Player PlayerMapper(BusinessObject.Player playerJson)
        {
            var player = new Player();

            player.Bet = playerJson.bet;
            
            if (playerJson.hole_cards != null && playerJson.hole_cards.Count >= 2)
            {
                player.Card1 = CardMapper(playerJson.hole_cards[0]);
                player.Card2 = CardMapper(playerJson.hole_cards[1]);
            }

            player.Name = playerJson.name;
            player.Stack = int.Parse(playerJson.stack);
            player.Status = playerJson.status;
            
            return player;
        }

        public static Card CardMapper( BusinessObject.Card cardJson)
        {
            var card = new Card();
            card.Color = cardJson.suit;
            card.Rank = StringToRankMapper(cardJson.rank);
            return card;
        }
                
        public static Rank StringToRankMapper(string rankString)
        {
            switch (rankString)
            {
                case "A": return Rank.Ace;
                case "K": return Rank.King;
                case "Q": return Rank.Queen;
                case "J": return Rank.Jack;
                case "10": return Rank._10;
                case "9": return Rank._9;
                case "8": return Rank._8;
                case "7": return Rank._7;
                case "6": return Rank._6;
                case "5": return Rank._5;
                case "4": return Rank._4;
                case "3": return Rank._3;
                case "2": return Rank._2;
            }

            throw new NotImplementedException();
        }
        
        public enum Rank
        {
            Ace = 14,
            King = 13,
            Queen = 12,
            Jack = 11,
            _10 = 10,
            _9 = 9,
            _8 = 8,
            _7 = 7,
            _6 = 6,
            _5 = 5,
            _4 = 4,
            _3 = 3,
            _2 = 2
        }

        public class Tournament
        {
            public List<Card> CommunityCards { get; set; }
            public List<Player> OtherPlayers { get; set; }
            public Player OurPlayer { get; set; }
            public int Round { get; set; }
            public int CurrentBuyIn { get; set; }
        }

        public class Player
        {
            public string Name { get; set; }
            public int Stack { get; set; }
            public string Status { get; set; }
            public int Bet { get; set; }
            public Card Card1 { get; set; }
            public Card Card2 { get; set; }

            public bool SameRank()
            {
                return Card1.Rank == Card2.Rank;
            }

            public bool SameColor()
            {
                return Card1.Color == Card2.Color;
            }
        }

        public class Card
        {
            public Rank Rank { get; set; }
            public string Color { get; set; }

            public string GetString()
            {
                if (Rank == Rank.Ace)
                {
                    return "A";
                }

                if (Rank == Rank.King)
                {
                    return "K";
                }

                if (Rank == Rank.Queen)
                {
                    return "Q";
                }

                if (Rank == Rank.Jack)
                {
                    return "J";
                }

                return ((int)Rank).ToString();
            }
        }
    }
}