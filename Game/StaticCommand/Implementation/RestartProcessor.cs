namespace Game.StaticCommand.Implementation;

internal class RestartProcessor : QuitProcessor
{
    protected override string ReturnValue => "-2";

    protected override string Verb => "restart";
}