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
        Console.WriteLine(result);

    }
}