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
            var jsonString = @"{""id"":""6048b938c7bae6000442bbed"",""tournament_id"":""604661d35395db00047b2859"",""started"":""2021-03-10 12:19:04 UTC"",""status"":""done"",""teams"":[{""name"":""Royal Flush"",""url"":""http://boiling-reef-21262.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""fecd615031bb8db1cc3901b9b79b4b67c0fd65a4"",""ranking"":7,""version"":""Default C# folding player"",""gained"":0,""commit_id"":""fecd615031bb8db1cc3901b9b79b4b67c0fd65a4"",""points"":166,""relative_points"":-60.0,""deployed"":false},{""name"":""Oceans 5"",""url"":""http://pacific-ocean-58773.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""ebb6a00fde5d7c153ebad296446cab3b422f2a72"",""ranking"":2,""version"":""V02"",""gained"":3,""commit_id"":""ebb6a00fde5d7c153ebad296446cab3b422f2a72"",""points"":269,""relative_points"":43.0,""deployed"":false},{""name"":""Stone Cold Nuts"",""url"":""http://enigmatic-sierra-77980.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""2449851fa40a96760174f4f5280d94651f5c1c41"",""ranking"":1,""version"":""Stone Cold Nuts"",""gained"":5,""commit_id"":""2449851fa40a96760174f4f5280d94651f5c1c41"",""points"":288,""relative_points"":62.0,""deployed"":false},{""name"":""Heavy Waterfall"",""url"":""http://powerful-badlands-48097.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""0d4afca5b17917c4fda4ef15574f27f43b28ba82"",""ranking"":4,""version"":""We smarter now."",""gained"":0,""commit_id"":""0d4afca5b17917c4fda4ef15574f27f43b28ba82"",""points"":165,""relative_points"":-61.0,""deployed"":false},{""name"":""Lean Stakes"",""url"":""http://hidden-lowlands-11920.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""808b88768b9caea199b8c3093077530dc25d1a37"",""ranking"":6,""version"":""The lean stakes"",""gained"":0,""commit_id"":""35d1a72b2be89c70a0340322df050f212ddef229"",""points"":232,""relative_points"":6.0,""deployed"":false},{""name"":""Flop Bot"",""url"":""http://gentle-journey-01994.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""dc393e484bda3a08c9f5058e942617ca7ec38367"",""ranking"":3,""version"":""TO THE MOON 🚀🚀🚀 - 13:02"",""gained"":0,""commit_id"":""dc393e484bda3a08c9f5058e942617ca7ec38367"",""points"":220,""relative_points"":-6.0,""deployed"":false},{""name"":""SalasBot"",""url"":""http://sheltered-ravine-70643.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""9f4838e769065688e813f3bbe322aee235f382c3"",""ranking"":8,""version"":""Default C# folding player"",""gained"":0,""commit_id"":""9f4838e769065688e813f3bbe322aee235f382c3"",""points"":208,""relative_points"":-18.0,""deployed"":false},{""name"":""Negreanu 2"",""url"":""http://obscure-scrubland-42781.herokuapp.com/"",""instance_id"":null,""commit_id_at_start"":""0d027fc0e4761c90e48d270e480b3b765810a545"",""ranking"":5,""version"":""Logging"",""gained"":0,""commit_id"":""0d027fc0e4761c90e48d270e480b3b765810a545"",""points"":260,""relative_points"":34.0,""deployed"":false}],""salted_response_token"":""3cbcb69551ec09ecfa2fa9e910ff424bff49bb32f9a294e1814577fd57bf1106"",""fragments"":157,""finished"":""2021-03-10 12:21:28 UTC""}";
    
            var json = JObject.Parse(jsonString);
            
            // Act
            int result = PokerPlayer.BetRequest(json);

            // Assert
            Assert.Equals(result, 39);
        }
    }
}