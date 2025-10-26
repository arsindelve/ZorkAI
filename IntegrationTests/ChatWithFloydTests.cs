using ChatLambda;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class ChatWithFloydTests
{
    [Test]
    public async Task ChatWithFloyd()
    {
        var target = new ChatWithFloyd(null);
        var result = await target.AskFloydAsync("floyd, you're such a good friend! I love you so much");
        Console.WriteLine(result.Message);
        if (result.Metadata != null)
        {
            Console.WriteLine($"Assistant Type: {result.Metadata.AssistantType}");
        }
    }
    
    [Test]
    public async Task FloydGoNorth()
    {
        var target = new ChatWithFloyd(null);
        var result = await target.AskFloydAsync("floyd, go north");
        Console.WriteLine(result.Message);
        if (result.Metadata != null)
        {
            Console.WriteLine($"Assistant Type: {result.Metadata.AssistantType}");
            if (result.Metadata.Parameters != null)
            {
                foreach (var param in result.Metadata.Parameters)
                {
                    Console.WriteLine($"  {param.Key}: {param.Value}");
                }
            }
        }
    }
}