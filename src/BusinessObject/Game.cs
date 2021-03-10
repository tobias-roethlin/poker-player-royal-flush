using System.Collections.Generic;
using Nancy.Simple.Logic;

namespace Nancy.Simple
{
    public class Game
    {
        public List<Player> players { get; set; }

        public string tournament_id { get; set; }
        
        public string game_id { get; set; }
        
        public int round { get; set; }
        
        public int bet_index { get; set; }
        
        public int small_blind { get; set; }
        
        public int orbits { get; set; }
       
        public int dealer { get; set; }
        
        public IList<Card> community_cards { get; set; }
        
        public int current_buy_in { get; set; }
        
        public int pot { get; set; }
    }
    
    /*curl -d 'action=bet_request&game_state={
    "players":[
    {
        "name":"Player 1",
        "stack":1000,
        "status":"active",
        "bet":0,
        "hole_cards":[],
        "version":"Version name 1",
        "id":0
    },
    {
        "name":"Player 2",
        "stack":1000,
        "status":"active",
        "bet":0,
        "hole_cards":[],
        "version":"Version name 2",
        "id":1
    }
    ],
    "tournament_id":"550d1d68cd7bd10003000003",
    "game_id":"550da1cb2d909006e90004b1",
    "round":0,
    "bet_index":0,
    "small_blind":10,
    "orbits":0,
    "dealer":0,
    "community_cards":[],
    "current_buy_in":0,
    "pot":0
}' http://boiling-reef-21262.herokuapp.com/*/
}