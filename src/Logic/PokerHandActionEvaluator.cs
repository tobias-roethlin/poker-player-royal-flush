using System;
using Nancy.Simple.BusinessObject;
using Card = Nancy.Simple.Logic.Card;
using Rank = Nancy.Simple.Logic.Rank;

namespace Nancy.Simple.Logic
{
    public static class PokerHandActionEvaluator
    {
        public static PokerAction GetActionBasendOnHandCards(Card higherCard, Card lowerCard)
        {
            return higherCard.Color == lowerCard.Color ? GetActionForSuitedCards(higherCard, lowerCard) : GetActionForUnsuitedCards(higherCard, lowerCard);
        }

        public static PokerAction GetActionForSuitedCards(Card higherCard, Card lowerCard)
        {
            switch (higherCard.Rank)
            {
                case Rank.Ace:
                    return PokerAction.Call;
                case Rank.King:
                    return PokerAction.Call;
                case Rank.Queen:
                    return PokerAction.Call;
                case Rank.Jack:
                    return PokerAction.Call;
                case Rank._10:
                    return PokerAction.Call;
                case Rank._9:
                    return (int)lowerCard.Rank >= 3 ? PokerAction.Call : PokerAction.Fold;
                case Rank._8:
                    return (int)lowerCard.Rank >= 4 ? PokerAction.Call : PokerAction.Fold;
                case Rank._7:
                    return (int)lowerCard.Rank >= 3 ? PokerAction.Call : PokerAction.Fold;
                case Rank._6:
                    return (int)lowerCard.Rank >= 3 ? PokerAction.Call : PokerAction.Fold;
                case Rank._5:
                    return PokerAction.Call;
                case Rank._4:
                    return PokerAction.Call;
                case Rank._3:
                    return PokerAction.Fold;
                case Rank._2:
                    return PokerAction.Fold;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static PokerAction GetActionForUnsuitedCards(Card higherCard, Card lowerCard)
        {
            switch (higherCard.Rank)
            {
                case Rank.Ace:
                    return PokerAction.Call;
                case Rank.King:
                    return (int)lowerCard.Rank >= 6 ? PokerAction.Call : PokerAction.Fold;
                case Rank.Queen:
                    return (int)lowerCard.Rank >= 7 ? PokerAction.Call : PokerAction.Fold;
                case Rank.Jack:
                    return (int)lowerCard.Rank >= 8 ? PokerAction.Call : PokerAction.Fold;
                case Rank._10:
                    return (int)lowerCard.Rank >= 7 ? PokerAction.Call : PokerAction.Fold;
                case Rank._9:
                    return (int)lowerCard.Rank >= 7 ? PokerAction.Call : PokerAction.Fold;
                case Rank._8:
                    return (int)lowerCard.Rank >= 7 ? PokerAction.Call : PokerAction.Fold;
                case Rank._7:
                    return (int)lowerCard.Rank >= 7 ? PokerAction.Call : PokerAction.Fold;
                case Rank._6:
                    return (int)lowerCard.Rank >= 6 ? PokerAction.Call : PokerAction.Fold;
                case Rank._5:
                    return (int)lowerCard.Rank >= 5 ? PokerAction.Call : PokerAction.Fold;
                case Rank._4:
                    return (int)lowerCard.Rank >= 4 ? PokerAction.Call : PokerAction.Fold;
                case Rank._3:
                    return (int)lowerCard.Rank >= 3 ? PokerAction.Call : PokerAction.Fold;
                case Rank._2:
                    return (int)lowerCard.Rank >= 2 ? PokerAction.Call : PokerAction.Fold;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}