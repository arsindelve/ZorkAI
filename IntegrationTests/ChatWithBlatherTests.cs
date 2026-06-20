using ChatLambda;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class ChatWithBlatherTests
{
    private static IEnumerable<string> TestPhrases()
    {
        yield return "blather, I've finished scrubbing the floor, sir";
        yield return "blather, why do you hate me so much?";
        yield return "blather, can I please have a promotion?";
        yield return "blather, bkjff3f3f";
        yield return "blather, the ship is going to explode!";
        yield return "blather, you have a speck of dust on your uniform";
        yield return "blather, where are all the other officers?";
        yield return "blather, I'm sorry sir, it won't happen again";
    }

    [Test]
    [TestCaseSource(nameof(TestPhrases))]
    public async Task ChatWithBlather(string phrase)
    {
        var target = new ChatWithBlather(null);
        var result = await target.AskBlatherAsync(phrase);

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
    public async Task BlatherGivesDemerits()
    {
        var target = new ChatWithBlather(null);
        var result = await target.AskBlatherAsync("blather, I refuse to do any more push-ups");
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
    public async Task BlatherInspectsTheFloor()
    {
        var target = new ChatWithBlather(null);
        var result = await target.AskBlatherAsync("blather, come inspect the floor I just polished");
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