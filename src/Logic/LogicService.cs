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
            if (tournament.IsPreFlop && action == PokerAction.Fold)
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

            if (tournament.IsPreFlop && probability < 0.2)
            {
                return ConsiderFold(tournament) ? 0 : tournament.CurrentBuyIn;
            }

            var considerAllIn = false;
            var betValue = tournament.OurPlayer.Stack * (1.0 / 100 * probability / 2);
            if (!tournament.IsPreFlop)
            {
                betValue = tournament.OurPlayer.Stack * (1.0 / 100 * probability / 10);
            }

            if (tournament.IsPreFlop && probability > 0.5)
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.5);
            }

            var firstCardWithCommunityCards = new[] { tournament.OurPlayer.Card1 }.Union(tournament.CommunityCards);
            var secondCardWithCommunityCards = new[] { tournament.OurPlayer.Card2 }.Union(tournament.CommunityCards);
            var ourCards = tournament.OurPlayer.GetCards();

            if ((IsStraightFlush(firstCardWithCommunityCards) || IsStraightFlush(secondCardWithCommunityCards))
                || IsFullHouse(firstCardWithCommunityCards) || IsFullHouse(secondCardWithCommunityCards)
                || IsFourOfAKind(firstCardWithCommunityCards) || IsFourOfAKind(secondCardWithCommunityCards))
            {
                betValue = Math.Max(betValue, tournament.Pot * 2);
                considerAllIn = true;
            }
            else if (IsFlush(firstCardWithCommunityCards) || IsFlush(secondCardWithCommunityCards))
            {
                betValue = tournament.Pot * 1.5;
            }
            else if (IsStraight(firstCardWithCommunityCards) || IsStraight(secondCardWithCommunityCards))
            {
                betValue = tournament.Pot * 1.5;
            }
            else if (IsThreeOfAKind(tournament.GetCards()) && IsPair(ourCards))
            {
                betValue = tournament.Pot * 1.5;
            }
            else if (IsThreeOfAKind(tournament.GetCards()) && (IsPair(firstCardWithCommunityCards) || IsPair(secondCardWithCommunityCards)))
            {
                betValue = tournament.Pot;
            }
            else if (IsTwoPair(tournament.GetCards()) && IsPair(ourCards))
            {
                betValue = tournament.Pot * 0.8;
            }
            else if ((IsPair(firstCardWithCommunityCards) || IsPair(secondCardWithCommunityCards)) && !IsPair(tournament.CommunityCards))
            {
                betValue = tournament.Pot * 0.5;
            }

            var maxBetValue = betValue * aggressionlevel;

            if (tournament.IsPreFlop && betValue < tournament.CurrentBuyIn)
            {
                return ConsiderFold(tournament) ? 0 : tournament.CurrentBuyIn;
            }

            if (!considerAllIn && maxBetValue < tournament.CurrentBuyIn)
            {
                return ConsiderFold(tournament) ? 0 : tournament.CurrentBuyIn;
            }

            return (int)Math.Min(Math.Max((int)betValue, tournament.CurrentBuyIn), considerAllIn ? tournament.OurPlayer.Stack : tournament.OurPlayer.Stack * 0.7);
        }

        private static bool ConsiderFold(Tournament tournament)
        {
            return tournament.OurPlayer.Bet < tournament.OurPlayer.Stack;
        }

        private static bool IsFourOfAKind(IEnumerable<Card> cards)
        {
            return cards.GroupBy(c => c.Rank).Any(g => g.Count() == 4);
        }

        private static bool IsFullHouse(IEnumerable<Card> cards)
        {
            return cards.GroupBy(c => c.Rank).Where(g => g.Count() == 2).Count() == 1
                && cards.GroupBy(c => c.Rank).Where(g => g.Count() == 3).Count() == 1;
        }

        private static bool IsThreeOfAKind(IEnumerable<Card> cards)
        {
            return cards.GroupBy(c => c.Rank).Any(g => g.Count() == 3);
        }

        private static bool IsTwoPair(IEnumerable<Card> cards)
        {
            return cards.GroupBy(c => c.Rank).Where(g => g.Count() == 2).Count() >= 2;
        }

        private static bool IsPair(IEnumerable<Card> cards)
        {
            return cards.GroupBy(c => c.Rank).Any(g => g.Count() == 2);
        }

        private static bool IsStraightFlush(IEnumerable<Card> cards)
        {
            var sameColorCards = cards.GroupBy(c => c.Color);
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