using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;

namespace Nancy.Simple.Logic
{
    public static class LogicService
    {
        public static int Bet(Game game)
        {
            return 1000;


            var tournament = new Tournament();
            //tournament.OtherPlayers = new List<Player>();
            //tournament.OurPlayer = new Player
            //{
                
            //};

            if (tournament == null)
            {
                return tournament.OurPlayer.Stack / 10;
            }

            if (tournament.Round == 1)
            {
                if (tournament.OurPlayer.HasPocket())
                {
                    return tournament.OurPlayer.Stack;
                }

                if (tournament.OurPlayer.HasSuited())
                {
                    return tournament.OurPlayer.Stack / 4;
                }
            }

            return tournament.OurPlayer.Stack / 10;
        }

        //private double ProbabilityBeingDealtWithStartingHandPercent(Card card1, Card card2)
        //{
        //    if (card1.Rank == card2.Rank && (card1.Rank == Rank.Ace || card1.Rank == Rank.King))
        //    {
        //        return 0.453;
        //    }

        //    if (card1.Rank == card2.Rank && card1.Rank == Rank.Queen)
        //    {
        //        return 1.36;
        //    }

        //    if (card1.Rank == card2.Rank && card1.Rank == Rank.Jack)
        //    {
        //        return 1.81;
        //    }

        //    if (card1.Rank == card2.Rank)
        //    {
        //        return 5.88;
        //    }

        //    if (card1.Rank == card2.Rank)
        //    {
        //        return 1.36;
        //    }

        //    if (card1.Rank == card2.Rank && card1.Rank == Rank.Queen)
        //    {
        //        return 1.36;
        //    }

        //    return 14.3;
        //}

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
            public double GetProbabilityOfBeingBetFirstRound()
            {
                if (OurPlayer.HasPocket())
                {
                    return this.GetProbabilityForPoketsFirstRound();
                }

                return 0;
            }
        }

        public class Player
        {
            public string Name { get; set; }
            public int Stack { get; set; }
            public string Status { get; set; }
            public int Bet { get; set; }
            public Card Card1 { get; set; }
            public Card Card2 { get; set; }

            public bool HasPocket()
            {
                return Card1.Rank == Card2.Rank;
            }

            public bool HasSuited()
            {
                return Card1.Color == Card2.Color;
            }
        }

        public class Card
        {
            public Rank Rank { get; set; }
            public string Color { get; set; }
        }
    }
}