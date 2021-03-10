using System;
using System.Linq;
using Nancy.Simple.BusinessObject;

namespace Nancy.Simple.Logic
{
    public static class Mapper
    {
        

        public static Tournament CreateTournament(Game game)
        {
            var tournament = new Tournament();
            
            tournament.Round = game.round;
            tournament.CommunityCards = game.community_cards.Select(c => new Card {Color = c.suit, Rank = StringToRankMapper(c.rank)}).ToList();
            tournament.OtherPlayers = game.players.Where(p => p.name != "Royal Flush").Select(PlayerMapper).ToList();
            tournament.CurrentBuyIn = game.current_buy_in;
            tournament.Pot = game.pot;

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
    }
}