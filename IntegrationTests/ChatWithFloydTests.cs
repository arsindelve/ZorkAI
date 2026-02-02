using ChatLambda;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class ChatWithFloydTests
{
    private static IEnumerable<string> TestPhrases()
    {
        yield return "floyd, you're such a good friend! I love you so much";
        yield return "floyd, go find some food";
        yield return "floyd, bkjff3f3f";
        yield return "floyd, pick up the wrench";
        yield return "floyd, take the laser";
        yield return "floyd, press the red button";
        yield return "floyd, where did all the people go?";
        yield return "floyd, why isn't the course control system working?";
    }

    [Test]
    [TestCaseSource(nameof(TestPhrases))]
    public async Task ChatWithFloyd(string phrase)
    {
        var target = new ChatWithFloyd(null);
        var result = await target.AskFloydAsync(phrase);

        Console.WriteLine($"Phrase: \"{phrase}\"");
        Console.WriteLine($"Response: {result.Message}");

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

        Console.WriteLine(new string('-', 50));
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
    
    [Test]
    public async Task FloydGetTheFromitzBoard()
    {
        var target = new ChatWithFloyd(null);
        var result = await target.AskFloydAsync("floyd, go get the fromitz board");
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