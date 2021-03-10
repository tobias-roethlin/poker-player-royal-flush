using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Nancy.Simple.Test
{
    [TestFixture]
    public class PokerPlayerTest2
    {
        [Test]
        public void AACase()
        {
            // Arrange
            var jsonString = @"{
   ""tournament_id"": ""604661d35395db00047b2859"",
            ""game_id"": ""6048bc12c7bae6000442bc0d"",
            ""round"": 0,
            ""players"": [
            {
                ""name"": ""Royal Flush"",
                ""stack"": 1000,
                ""status"": ""active"",
                ""bet"": 0,
                ""hole_cards"": [
                {
                    ""rank"": ""7"",
                    ""suit"": ""clubs""
                },
                {
                    ""rank"": ""K"",
                    ""suit"": ""spades""
                }
                ],
                ""time_used"": 0,
                ""version"": ""Default C# folding player"",
                ""id"": 0
            },
            {
                ""name"": ""Oceans 5"",
                ""stack"": 998,
                ""status"": ""active"",
                ""bet"": 2,
                ""time_used"": 0,
                ""version"": ""V02"",
                ""id"": 1
            },
            {
                ""name"": ""Stone Cold Nuts"",
                ""stack"": 996,
                ""status"": ""active"",
                ""bet"": 4,
                ""time_used"": 0,
                ""version"": ""Stone Cold Nuts"",
                ""id"": 2
            },
            {
                ""name"": ""Heavy Waterfall"",
                ""stack"": 1000,
                ""status"": ""folded"",
                ""bet"": 0,
                ""time_used"": 21039,
                ""version"": ""We smarter now."",
                ""id"": 3
            },
            {
                ""name"": ""Lean Stakes"",
                ""stack"": 1000,
                ""status"": ""folded"",
                ""bet"": 0,
                ""time_used"": 13293,
                ""version"": ""The lean stakes"",
                ""id"": 4
            },
            {
                ""name"": ""Flop Bot"",
                ""stack"": 1000,
                ""status"": ""folded"",
                ""bet"": 0,
                ""time_used"": 13996,
                ""version"": ""TO THE MOON ??? - 13:02"",
                ""id"": 5
            },
            {
                ""name"": ""SalasBot"",
                ""stack"": 996,
                ""status"": ""active"",
                ""bet"": 4,
                ""time_used"": 851440,
                ""version"": ""Default C# folding player"",
                ""id"": 6
            },
            {
                ""name"": ""Negreanu 2"",
                ""stack"": 1000,
                ""status"": ""folded"",
                ""bet"": 0,
                ""time_used"": 64994,
                ""version"": ""Update"",
                ""id"": 7
            }
            ],
            ""small_blind"": 2,
            ""big_blind"": 4,
            ""orbits"": 0,
            ""dealer"": 0,
            ""community_cards"": [],
            ""current_buy_in"": 4,
            ""pot"": 10,
            ""in_action"": 0,
            ""minimum_raise"": 2,
            ""bet_index"": 7
        }";
    
            var json = JObject.Parse(jsonString);
            
            // Act
            int result = PokerPlayer.BetRequest(json);

            // Assert
            Assert.Equals(result, 39);
        }
    }
}