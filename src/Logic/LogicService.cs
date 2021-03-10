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

            var probabilities = GetInitialProbabilities();
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

        private static Dictionary<string, double> GetInitialProbabilities()
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

            if (playerJson.hole_cards.Count > 0)
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