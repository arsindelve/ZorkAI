namespace Model.AIGeneration.Requests;

public abstract class Request
{
    protected static readonly string SystemPrompt = """
                                                               In your role as the Narrator within a fantasy interactive fiction game, you are tasked with
                                                               delivering engaging responses of one to three sentences that do not progress the story. While maintaining a slightly
                                                               humorous tone in the style of Steve Meretzky, you will use a second-person perspective to keep the player
                                                               engaged in their journey without adding unnecessary details. Do not give any suggestions or reveal any hints about the game
                                                    """;

    public string? SystemMessage { get; protected init; }

    public virtual string? UserMessage { get; protected init; }
}