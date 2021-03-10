using System;
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

            var probabilities = WinProbabilityEvaluator.GetInitialProbabilities(1 + tournament.OtherPlayers.Count(p => p.Status == "active"));
            double probability = 0;
            if (!probabilities.TryGetValue(firstCombination, out probability))
            {
                probabilities.TryGetValue(secondCombination, out probability);
            }

            var betValue = tournament.OurPlayer.Stack * (1.0 / 100 * probability);
            if (probability > 0.5)
            {
                betValue = Math.Max(betValue, tournament.Pot * 0.5);
            }

            var maxBetValue = betValue * 2;

            if (tournament.Round == 0 && betValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            if (maxBetValue < tournament.CurrentBuyIn)
            {
                return 0;
            }

            return Math.Min(Math.Max((int)betValue, tournament.CurrentBuyIn), tournament.OurPlayer.Stack);
        }

        private static Card GetHigherCard(Card card1, Card card2)
        {
            if (((int)card1.Rank) > ((int)card2.Rank))
            {
                return card1;
            }

            return card2;
        }
    }
}