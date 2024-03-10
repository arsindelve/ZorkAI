﻿using Game;
using ZorkOne;

Console.ForegroundColor = ConsoleColor.DarkCyan;

var engine = CreateEngine();
Utilities.Utilities.WriteLineWordWrap(engine.IntroText);

var result = string.Empty;

while (result != "-1")
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("> ");

    var command = Console.ReadLine();
    result = await engine.GetResponse(command);

    if (result?.Trim().StartsWith("-2") ?? false)
    {
        engine = CreateEngine();
        Utilities.Utilities.WriteLineWordWrap(engine.IntroText);
    }

    else if (result?.Trim().StartsWith("-1") ?? false)
    {
        Environment.Exit(0);
    }

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Utilities.Utilities.WriteLineWordWrap(result);
}

GameEngine<ZorkI> CreateEngine()
{
    return new GameEngine<ZorkI>();
}