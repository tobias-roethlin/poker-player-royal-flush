using System.Collections.Generic;

namespace Nancy.Simple.BusinessObject
{
    public class Player
    {
        public string name { get; set; }
        
        public string stack { get; set; }
        
        public string status { get; set; }
        
        public int bet { get; set; }

        public List<Card> hole_cards { get; set; }
        
        public string version { get; set; }
        
        public int id { get; set; }
    }
    
    /*"players":[
    {
        "name":"Player 1",
        "stack":1000,
        "status":"active",
        "bet":0,
        "hole_cards":[],
        "version":"Version name 1",
        "id":0
    },*/
}