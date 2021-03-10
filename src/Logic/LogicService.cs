﻿using System;
using System.Linq;
using Nancy.Simple.BusinessObject;

namespace Nancy.Simple.Logic
{
    public static class LogicService
    {
        public static int Bet(Game game)
        {
            var tournament = Mapper.CreateTournament(game);

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

            var numberOfPlayers = 1 + tournament.OtherPlayers.Count(p => p.Status == "active");
            var aggressionlevel = GetAggressionLevel(numberOfPlayers);
            var probabilities = WinProbabilityEvaluator.GetInitialProbabilities(numberOfPlayers);
            double probability = 0;
            if (!probabilities.TryGetValue(firstCombination, out probability))
            {
                probabilities.TryGetValue(secondCombination, out probability);
            }

            var considerAllIn = false;
            var betValue = tournament.OurPlayer.Stack * (1.0 / 100 * probability);
            if (probability > 0.5)
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.5);
            }

            if (IsStraightFlush(tournament) || IsFullHouse(tournament) || IsFourOfAKind(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot);
                considerAllIn = true;
            }
            else if (IsThreeOfAKind(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.9);
                considerAllIn = true;
            }
            else if (IsStraight(tournament) || IsFlush(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.8);
                considerAllIn = true;
            }
            else if (IsTwoPair(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.5);
            }
            else if (IsPair(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.3);
            }

            var maxBetValue = betValue * 2;

            if (tournament.Round == 0 && betValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            if (!considerAllIn && maxBetValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            return Math.Min(Math.Max((int)betValue, tournament.CurrentBuyIn), tournament.OurPlayer.Stack);
        }

        private static bool IsFourOfAKind(Tournament tournament)
        {
            return tournament.GetCards().GroupBy(c => c.Rank).Any(g => g.Count() == 4);
        }

        private static bool IsFullHouse(Tournament tournament)
        {
            return tournament.GetCards().GroupBy(c => c.Rank).Where(g => g.Count() == 2).Count() == 1
                && tournament.GetCards().GroupBy(c => c.Rank).Where(g => g.Count() == 3).Count() == 1;
        }

        private static bool IsThreeOfAKind(Tournament tournament)
        {
            return tournament.GetCards().GroupBy(c => c.Rank).Any(g => g.Count() == 3);
        }

        private static bool IsTwoPair(Tournament tournament)
        {
            return tournament.GetCards().GroupBy(c => c.Rank).Where(g => g.Count() == 2).Count() >= 2;
        }

        private static bool IsPair(Tournament tournament)
        {
            return tournament.GetCards().GroupBy(c => c.Rank).Any(g => g.Count() == 2);
        }

        private static bool IsStraightFlush(Tournament tournament)
        {
            var sameColorCards = tournament.GetCards().GroupBy(c => c.Color);
            foreach (var group in sameColorCards)
            {
                if (IsStraight(group) && IsFlush(group))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFlush(IEnumerable<Card> card)
        {
            return card.GroupBy(c => c.Color).Any(g => g.Count() >= 5);
        }

        private static bool IsFlush(Tournament tournament)
        {
            return IsFlush(tournament.GetCards());
        }

        private static bool IsStraight(Tournament tournament)
        {
            var ranks = GetRanks(tournament.GetCards());
            for (int i = 1; i < 14; i++)
            {
                if (ranks.Contains(i)
                    && ranks.Contains(i + 1)
                    && ranks.Contains(i + 2)
                    && ranks.Contains(i + 3)
                    && ranks.Contains(i + 4))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsStraight(IEnumerable<Card> cards)
        {
            var ranks = GetRanks(cards);
            for (int i = 1; i < 14; i++)
            {
                if (ranks.Contains(i)
                    && ranks.Contains(i + 1)
                    && ranks.Contains(i + 2)
                    && ranks.Contains(i + 3)
                    && ranks.Contains(i + 4))
                {
                    return true;
                }
            }

            return false;
        }

        private static HashSet<int> GetRanks(IEnumerable<Card> cards)
        {
            var set = new HashSet<int>();

            foreach (var card in cards)
            {
                set.Add((int)card.Rank);
            }

            if (set.Contains(14))
            {
                set.Add(1);
            }

            return set;
        }

        private static Card GetHigherCard(Card card1, Card card2)
        {
            if (((int)card1.Rank) > ((int)card2.Rank))
            {
                return card1;
            }

            return card2;
        }

        public static double GetAggressionLevel(int playerCount)
        {
            switch (playerCount)
            {
                case 2: return 3;
                case 3: return 2.5;
                case 4: return 2.2;
                default: return 2;
            }
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
            public int Pot { get; set; }
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