namespace Nancy.Simple.Logic
{
    public class Player
    {
        public string Name { get; set; }
        public int Stack { get; set; }
        public string Status { get; set; }
        public int Bet { get; set; }
        public Card Card1 { get; set; }
        public Card Card2 { get; set; }

        public bool SameRank()
        {
            return Card1.Rank == Card2.Rank;
        }

        public bool SameColor()
        {
            return Card1.Color == Card2.Color;
        }
    }
}