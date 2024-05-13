using ZorkOne;

namespace UnitTests.ZorkITests.Walkthrough;

public abstract class WalkthroughTestBase : EngineTestsBase
{
    private GameEngine<ZorkI, ZorkIContext> _target;

    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();
        Repository.Reset();
    }
    
    protected void InvokeGodMode(string setup)
    {
        var method = GetType().GetMethod(setup);
        if (method == null) throw new ArgumentException("Method " + setup + " doesn't exist");

        // Invoke the method on the current instance
        method.Invoke(this, null);
    }
    
    public void KillTroll()
    {
        // We can't have the randomness of trying to kill the troll. Let's God-Mode this dude. 
        Repository.GetItem<Troll>().IsDead = true;
    }

    public void GoToRoundRoom()
    {
        // Entering the loud room when it's draining will cause us to flee the room in a random 
        // direction. For the test we need to remove the randomness and end up in the Round Room
        _target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
    }
    
    protected async Task Do(string input, params string[] outputs)
    {
        var result = await _target.GetResponse(input);
        //Console.WriteLine(result);
        foreach (var output in outputs) result.Should().Contain(output);
    }
}