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
    }
}