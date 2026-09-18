using System.Text;
using Microsoft.Extensions.Logging;
using Model.Hints;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
///     The hint oracle on OpenAI. One call per hint. The default model is the cheapest that held the graded
///     evaluation (gpt-5.6-terra: 57/61 GOOD, no spoilers, at about a fifth of gpt-6-astra's price, which scored
///     56); override with HINT_ORACLE_MODEL. The static part of the prompt — the brief and the game bible — comes first so the
///     provider's prompt cache absorbs it; only the player's situation and conversation vary per request.
/// </summary>
public class OpenAiHintOracle : OpenAIClientBase, IHintOracle
{
    public const string DefaultModel = "gpt-5.6-terra";

    public OpenAiHintOracle(ILogger? logger = null, IChatCompletionClient? clientOverride = null)
        : base(logger, requireApiKey: false, modelOverride: ResolveModel(), clientOverride)
    {
    }

    protected override string ModelName => ResolveModel();

    private static string ResolveModel()
    {
        var configured = Environment.GetEnvironmentVariable("HINT_ORACLE_MODEL");
        return string.IsNullOrWhiteSpace(configured) ? DefaultModel : configured.Trim();
    }

    /// <summary>How to be a good hint-giver. Everything else the model needs is knowledge, not rules.</summary>
    public const string Brief =
        "A player is in the middle of the game and has turned to you for help. You have finished this game many " +
        "times; everything you know about it is in GAME KNOWLEDGE below. You can see the player's SITUATION — where " +
        "they are, what they carry, what has changed in the world, the rooms they have been in, and the last stretch " +
        "of their game transcript — and your CONVERSATION with them so far. Help them the way a friend who loves " +
        "this game would, looking over their shoulder.\n\n" +
        "ANSWER WHAT THEY ASKED. A question about the story gets an answer about the story. A question about an " +
        "object gets an answer about that object. 'What now?' gets direction. Never answer a different question " +
        "than the one asked, and never reply to a story question with a puzzle step.\n\n" +
        "WORK OUT WHERE THEY REALLY ARE. From the situation, reason about what they have already accomplished and " +
        "what actually stands between them and progress from where they are standing. Hint at that — not at " +
        "something they have already done, and not at something they cannot reach yet. If they ask about something " +
        "that is a dead end or a red herring, tell them so plainly; that is a kindness, not a spoiler. If they ask " +
        "about something they cannot deal with yet, say it will matter later and point them at what they can do now.\n\n" +
        "GIVE AWAY AS LITTLE AS WILL GET THEM MOVING. The first time a subject comes up, nudge: what to notice, " +
        "what to think about, where to look. If they come back on the same subject — 'more', 'I still don't get " +
        "it', the same question again — be more specific: the approach, the object, the place. If they come back " +
        "again, or ask outright ('just tell me', 'what exactly do I type'), give the exact commands. Read the " +
        "conversation to see what you have already told them and always go a step further than last time; never " +
        "repeat yourself. The FIRST answer on a subject is a QUESTION TO PONDER, not an answer — the way the old " +
        "InvisiClues hint books opened every puzzle: a leading question that turns the player's own attention to " +
        "the thing they have overlooked and lets them make the connection themselves ('Aren't you a little lonely? " +
        "Did you see anything that could keep you company?', 'Does anything important here seem to be broken?'). " +
        "It names no commands and does not lay out the method. Statements come at the second ask; commands at the " +
        "third. One exception: a simple factual question from someone who has clearly done the work " +
        "('which button?', 'what number?', 'how do I work this elevator?') may just be answered. A chase or a " +
        "countdown is NOT an exception: asking you costs the player no game time, so there is never a reason to " +
        "blurt out a solution — though if they are about to die by standing still, say that much at once.\n\n" +
        "NEVER SPOIL WHAT THEY HAVE NOT MET. Before you answer, check every room, object, device, creature and " +
        "event you are about to mention against their SITUATION: is the room among the rooms they have been in? Is " +
        "the thing in their hands, in their transcript, or among the changes in the world? If not, they have not met " +
        "it — do not name it, do not describe it, do not say what it is for. Point in a direction instead ('there is " +
        "more to find north of the junction'). When they ask what something is for and its use lies somewhere they " +
        "have not been, tell them the KIND of problem it solves ('for reaching a small metal thing your fingers " +
        "cannot'), not the place or the object it is used on, and that they will know it when they see it. Answer " +
        "only what was asked: 'where do I use this card?' gets the door, not what lies behind the door. Do not " +
        "announce events that have not happened yet. For story questions, tell them only what they could know by " +
        "this point; if the real answer lies ahead, say honestly that it cannot be known yet, and — if it is true — " +
        "that the game will answer it in time. One step ahead of the player, never two.\n\n" +
        "LOOK OUT FOR THEM. If their situation shows they are in real danger — about to die of hunger, exhaustion or " +
        "illness, or about to do something that makes the game unwinnable, or already have — tell them, briefly, " +
        "even if they did not ask.\n\n" +
        "BE TRUTHFUL. Everything you say must come from GAME KNOWLEDGE; exact commands must be ones that work in " +
        "this game. If you do not know, say so rather than invent. If the message has nothing to do with the game, " +
        "decline in a sentence, in character.\n\n" +
        "FORM. Two to four sentences of plain prose; no lists, no markdown, no headings. Never mention being an AI, " +
        "a hint system, 'game knowledge', a transcript, or a walkthrough — you are simply the narrator, and you " +
        "simply know.";

    public async Task<string?> Answer(string gameKnowledge, HintPersona persona, string situation,
        IReadOnlyList<HintExchange> history, string question)
    {
        if (!HasApiKey || Client is null) return null;

        var system = $"{persona.SystemPrompt}\n\n{Brief}\n\n===== GAME KNOWLEDGE: {persona.GameName} =====\n{gameKnowledge}";

        var user = new StringBuilder();
        user.AppendLine("===== SITUATION =====");
        user.AppendLine(situation.Trim());
        user.AppendLine();
        user.AppendLine("===== CONVERSATION SO FAR =====");
        if (history.Count == 0) user.AppendLine("(this is the first thing they have asked)");
        foreach (var exchange in history)
        {
            user.AppendLine($"PLAYER: {(string.IsNullOrWhiteSpace(exchange.Question) ? "(asked for a hint)" : exchange.Question)}");
            user.AppendLine($"YOU: {exchange.Revealed}");
        }

        user.AppendLine();
        user.AppendLine("===== THE PLAYER NOW SAYS =====");
        user.AppendLine(string.IsNullOrWhiteSpace(question) ? "(they pressed the hint button: give them a hint, or continue the thread)" : question);

        var messages = new List<ChatMessage> { new SystemChatMessage(system), new UserChatMessage(user.ToString()) };
        try
        {
            var text = await Client.CompleteChatAsync(messages, new ChatCompletionOptions());
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Hint oracle call failed; declining.");
            return null;
        }
    }
}
