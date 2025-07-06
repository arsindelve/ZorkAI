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
        var result = await target.AskFloydAsync("where did all the people go?");
        Console.WriteLine(result);

    }
}