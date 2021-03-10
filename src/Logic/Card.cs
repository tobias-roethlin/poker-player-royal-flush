namespace Nancy.Simple.Logic
{
    public class Card
    {
        public Rank Rank { get; set; }
        public string Color { get; set; }

        public string GetString()
        {
            if (Rank == Rank.Ace)
            {
                return "A";
            }

            if (Rank == Rank.King)
            {
                return "K";
            }

            if (Rank == Rank.Queen)
            {
                return "Q";
            }

            if (Rank == Rank.Jack)
            {
                return "J";
            }

            if (Rank == Rank._10)
            {
                return "T";
            }

            return ((int)Rank).ToString();
        }
    }
}