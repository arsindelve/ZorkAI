using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Library;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class SpoolReaderTests : EngineTestsBase
{
    [Test]
    public async Task Library_Look()
    {
        var target = GetTarget();
        StartHere<Library>();
        
        var response = await target.GetResponse("look");
        
        response.Should().Contain("There is a microfilm reader on one of the tables");
        
    }
    
    [Test]
    public async Task Examine_Empty()
    {
        var target = GetTarget();
        StartHere<Library>();
        
        var response = await target.GetResponse("examine reader");
        
        response.Should().Contain("The machine has a small screen, and below that, a small circular opening. The screen is currently blank");
    }
    
    [Test]
    public async Task PutIn_GreenSpool()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        
        var response = await target.GetResponse("put spool in reader");
        
        response.Should().Contain("The spool fits neatly into the opening. Some information appears on the screen");
    }
    
    [Test]
    public async Task PutIn_Pliers()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<Pliers>();
        
        var response = await target.GetResponse("put pliers in reader");
        
        response.Should().Contain("It doesn't fit in the circular opening");
    }
    
    [Test]
    public async Task PutIn_GreenSpool_Look()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        
        await target.GetResponse("put spool in reader");
        var response = await target.GetResponse("look");
        
        response.Should().Contain("The microfilm reader contains");
        response.Should().Contain(" A green spool");
    }
    
    [Test]
    public async Task PutIn_Examine()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        
        await target.GetResponse("put spool in reader");
        var response = await target.GetResponse("examine reader");
        
        response.Should().Contain("The screen is currently displaying some information:");
        response.Should().Contain("The rest is all very technical");
    }
    
    [Test]
    public async Task PutIn_Reader()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        
        await target.GetResponse("put spool in reader");
        var response = await target.GetResponse("read reader");
        
        response.Should().Contain("Oonlee peepul wix propur traaneeng shud piilot");
        response.Should().Contain("The rest is all very technical");
    }

    
    [Test]
    public async Task PutIn_Disambiguation()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        Take<RedSpool>();
        
        var response = await target.GetResponse("put spool in reader");
        response.Should().Contain("Do you mean the green spool or the red spool?");
        
        response = await target.GetResponse("red");
        response.Should().Contain("The spool fits neatly into the opening. Some information appears on the screen");
    }
    
    [Test]
    public async Task PutIn_AlreadyHasOne()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        Take<RedSpool>();
        
        await target.GetResponse("put red spool in reader");
        var response = await target.GetResponse("put green spool in reader");

        response.Should().Contain("There's already a spool in the reader");
    }
    
    [Test]
    public async Task PutIn_TakeOut()
    {
        var target = GetTarget();
        StartHere<Library>();
        Take<GreenSpool>();
        
        await target.GetResponse("put green spool in reader");
        var response = await target.GetResponse("take green spool");

        response.Should().Contain("The screen goes blank as you take the spool");
    }
}