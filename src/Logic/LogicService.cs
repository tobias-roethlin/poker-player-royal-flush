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
            else if (IsStraight(tournament) || IsFlush(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.9);
            }
            else if (IsThreeOfAKind(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.7);
            }
            else if (IsTwoPair(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.5);
            }
            else if (IsPair(tournament))
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.3);
            }

            var maxBetValue = betValue * aggressionlevel;

            if (tournament.Round == 0 && betValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            if (!considerAllIn && maxBetValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            return (int)Math.Min(Math.Max((int)betValue, tournament.CurrentBuyIn), considerAllIn ? tournament.OurPlayer.Stack : tournament.OurPlayer.Stack * 0.7);
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
    }
}