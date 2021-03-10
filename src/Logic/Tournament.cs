using System.Collections.Generic;

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