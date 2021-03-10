using System.Collections.Generic;
using System.Linq;

namespace Nancy.Simple.Logic
{
    public class Tournament
    {
        public List<Card> CommunityCards { get; set; }
        public List<Player> OtherPlayers { get; set; }
        public Player OurPlayer { get; set; }
        public int Round { get; set; }
        public int CurrentBuyIn { get; set; }
        public int Pot { get; set; }

        public bool IsPreFlop
        {
            get
            {
                return CommunityCards == null || CommunityCards.Count == 0;
            }
        }

        public IEnumerable<IEnumerable<Card>> GetCommunityCards(int numberOfCards)
        {
            var list = new List<IEnumerable<Card>>();
            if (CommunityCards == null)
            {
                return list;
            }
            
            if (CommunityCards.Count <= numberOfCards)
            {
                return new[] { CommunityCards };
            }

            return list;
        }

        public IEnumerable<Card> GetCards()
        {
            yield return OurPlayer.Card1;
            yield return OurPlayer.Card2;
            foreach (var card in CommunityCards)
            {
                yield return card;
            }
        }
    }
}