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

            if (tournament.OurPlayer.SameColor())
            {
                firstCombination += "s";
                secondCombination += "s";
            }


            var numberOfPlayers = 1 + tournament.OtherPlayers.Count(p => p.Status == "active");
            var aggressionlevel = GetAggressionLevel(numberOfPlayers);
            var probabilities = WinProbabilityEvaluator.GetInitialProbabilities(numberOfPlayers);
            double probability = 0;
            if (!probabilities.TryGetValue(firstCombination, out probability))
            {
                probabilities.TryGetValue(secondCombination, out probability);
            }

            if (tournament.IsPreFlop)
            {
                return HandlePreFlop(tournament, probability);
            }
            
            var considerAllIn = false;
            var betValue = 0.0;
        
            if (tournament.OurPlayer.Bet == 0)
            {
                betValue = tournament.OurPlayer.Stack * (1.0 / 100 * probability / 10);
            }


            var firstCardWithCommunityCards = new[] { tournament.OurPlayer.Card1 }.Union(tournament.CommunityCards);
            var secondCardWithCommunityCards = new[] { tournament.OurPlayer.Card2 }.Union(tournament.CommunityCards);
            var ourCards = tournament.OurPlayer.GetCards();
            var ourCardsWithCommunityCards = ourCards.Union(tournament.CommunityCards);
            var communityCards = tournament.CommunityCards;

            if ((IsStraightFlush(ourCardsWithCommunityCards) && !IsStraightFlush(communityCards))
                || (IsFullHouse(ourCardsWithCommunityCards) && !IsFullHouse(communityCards))
                || (IsFourOfAKind(ourCardsWithCommunityCards) && !IsFourOfAKind(communityCards)))
            {
                betValue = Math.Max(betValue, tournament.Pot * 3);
                considerAllIn = true;
            }
            else if (((IsStraightFlush(firstCardWithCommunityCards) || IsStraightFlush(secondCardWithCommunityCards)) && !IsStraightFlush(communityCards))
                || ((IsFullHouse(firstCardWithCommunityCards) || IsFullHouse(secondCardWithCommunityCards)) && !IsFullHouse(communityCards))
                || ((IsFourOfAKind(firstCardWithCommunityCards) || IsFourOfAKind(secondCardWithCommunityCards)) && !IsFourOfAKind(communityCards)))
            {
                betValue = Math.Max(betValue, tournament.Pot * 2);
                considerAllIn = true;
            }
            else if (IsFlush(ourCardsWithCommunityCards) && !IsFlush(communityCards))
            {
                betValue = tournament.Pot * 2;
                considerAllIn = true;
            }
            else if ((IsFlush(firstCardWithCommunityCards) || IsFlush(secondCardWithCommunityCards)) && !IsFlush(communityCards))
            {
                betValue = tournament.Pot * 1.5;
                considerAllIn = true;
            }
            else if ((IsStraight(firstCardWithCommunityCards) || IsStraight(secondCardWithCommunityCards)) && !IsStraight(communityCards))
            {
                betValue = tournament.Pot * 1.5;
                considerAllIn = true;
            }
            else if (IsThreeOfAKind(tournament.GetCards()) && IsPair(ourCards))
            {
                betValue = tournament.Pot * 1.5;
            }
            else if (IsThreeOfAKind(tournament.GetCards()) && (IsPair(firstCardWithCommunityCards) || IsPair(secondCardWithCommunityCards)))
            {
                betValue = tournament.Pot * 1.5;
            }
            else if (IsTwoPair(ourCardsWithCommunityCards) && !IsPair(communityCards))
            {
                betValue = tournament.Pot * 0.8;
            }
            else if (IsTwoPair(tournament.GetCards()) && IsPair(ourCards))
            {
                betValue = tournament.Pot * 0.8;
            }
            else if (IsTwoPair(firstCardWithCommunityCards) && IsTwoPair(secondCardWithCommunityCards) && !IsTwoPair(communityCards))
            {
                betValue = tournament.Pot * 0.8;
            }
            else if ((IsPair(firstCardWithCommunityCards) || IsPair(secondCardWithCommunityCards)) && !IsPair(communityCards))
            {
                betValue = tournament.Pot * 0.5;
            }
            else if (!WeHaveHighestCard(tournament) && ConsiderFold(tournament))
            {
                betValue = 0;
            }
            
            if (WeHaveHighestCard(tournament))
            {
                betValue = betValue *= 2;
            }

            var maxBetValue = betValue * aggressionlevel;

            if (!considerAllIn && maxBetValue < tournament.CurrentBuyIn)
            {
                return ConsiderFold(tournament) ? 0 : tournament.CurrentBuyIn;
            }

            return (int)Math.Min(Math.Max((int)betValue, tournament.CurrentBuyIn), considerAllIn ? tournament.OurPlayer.Stack : tournament.OurPlayer.Stack * 0.7);
        }

        private static int HandlePreFlop(Tournament tournament, double probability)
        {
            
            var higherCard = GetHigherCard(tournament.OurPlayer.Card1, tournament.OurPlayer.Card2);
            var lowerCard = higherCard == tournament.OurPlayer.Card1 ? tournament.OurPlayer.Card2 : tournament.OurPlayer.Card1;
            var isFirstBet = tournament.OurPlayer.Bet == 0;
            var isPotCommitted = !ConsiderFold(tournament);
            var Fold = 0;
            
            var action = PokerHandActionEvaluator.GetActionBasendOnHandCards(higherCard, lowerCard);
            
            if (action == PokerAction.Fold)
            {
                return Fold;
            }
            
            if (isFirstBet)
            {
                return (int) (tournament.OurPlayer.Stack * (1.0 / 100 * probability / 2));
            }

            if (probability > 0.5)
            {
                return (int) (tournament.Pot * 0.5);
            }
            
            if (probability > 0.2)
            {
                return (int) (tournament.OurPlayer.Stack * (1.0 / 100 * probability / 2));
            }

            if (isPotCommitted)
            {
                return tournament.CurrentBuyIn;
            }

            return Fold;
        }

        private static bool WeHaveHighestCard(Tournament tournament)
        {
            var ourCards = GetRanks(tournament.OurPlayer.GetCards());
            var allCards = GetRanks(tournament.GetCards());

            if (ourCards.Count == 0)
            {
                return true;
            }

            if (allCards.Count == 0)
            {
                return true;
            }

            return ourCards.Max() >= allCards.Max();
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