using FluentAssertions;
using Planetfall.Item.Lawanda.Library.Computer;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class ComputerTerminalTests : EngineTestsBase
{
    
    [Test]
    public async Task Look()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        
        var response = await target.GetResponse("look");
        response.Should().Contain("On the table is a computer terminal");
    }
    
    [Test]
    public async Task TurnOn()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        
        var response = await target.GetResponse("activate terminal");
        response.Should().Contain("The screen gives off a green flash, and then some writing appears on the screen:");
        response.Should().Contain("    1. Histooree\n    2. Kulcur\n    3. Teknolojee\n    4. Jeeografee\n    5. Xe Prajekt\n    6. Inturlajik Gaamz");
    }
    
    [Test]
    public async Task AlreadyOff()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        
        var response = await target.GetResponse("deactivate terminal");
        response.Should().Contain("isn't on");
    }
    
    [Test]
    public async Task Off_Read()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        
        var response = await target.GetResponse("read terminal");
        response.Should().Contain("The screen is dark");
    }
    
    [Test]
    public async Task Off_Examine()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        
        var response = await target.GetResponse("examine terminal");
        response.Should().Contain("The computer terminal consists of a video display screen, a keyboard with ten keys numbered from zero through nine, and an on-off switch. The screen is dark");
    }
    
    [Test]
    public async Task On_Examine()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;
        
        var response = await target.GetResponse("examine terminal");
        response.Should().Contain("The computer terminal consists of a video display screen, a keyboard with ten keys numbered from zero through nine, and an on-off switch. The screen displays some writing:");
        response.Should().Contain("    1. Histooree\n    2. Kulcur\n    3. Teknolojee\n    4. Jeeografee\n    5. Xe Prajekt\n    6. Inturlajik Gaamz");
    }
    
    [Test]
    public async Task On_Read()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;
        
        var response = await target.GetResponse("read terminal");
        response.Should().Contain("    1. Histooree\n    2. Kulcur\n    3. Teknolojee\n    4. Jeeografee\n    5. Xe Prajekt\n    6. Inturlajik Gaamz");
    }
    
    [Test]
    public async Task AlreadyOn()
    {
        var target= GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;
        
        var response = await target.GetResponse("activate terminal");
        response.Should().Contain("already on");
    }
}