namespace Nancy.Simple
{
    public class Card
    {
        public string rank { get; set; }                // Rank of the card. Possible values are numbers 2-10 and J,Q,K,A
        public string suit { get; set; }  
    }
    
    /*"hole_cards": [                         // The cards of the player. This is only visible for your own player
    //     except after showdown, when cards revealed are also included.
    {
        "rank": "6",                    // Rank of the card. Possible values are numbers 2-10 and J,Q,K,A
        "suit": "hearts"                // Suit of the card. Possible values are: clubs,spades,hearts,diamonds
    },
    {
    "rank": "K",
    "suit": "spades"
}
]*/
}