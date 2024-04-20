namespace ZorkOne.Item;

public class Matchbook : ItemBase, ICanBeRead, ICanBeExamined, ICanBeTakenAndDropped, IAmALightSource,
    ICanBeTurnedOnAndOff, ITurnBasedActor
{
    public int TurnsRemainingLit { get; set; }

    public int MatchesUsed { get; set; }

    public bool Lit { get; set; }

    public override string[] NounsForMatching => ["matches", "matchbook", "match"];

    public override string InInventoryDescription => "A matchbook" + (Lit ? " (providing light)" : "");

    public string ExaminationDescription => "The matchbook isn't very interesting, except for what's written on it. ";

    public string ReadDescription => """
                                     (Close cover before striking)

                                     YOU too can make BIG MONEY in the exciting field of PAPER SHUFFLING!

                                     Mr. Anderson of Muddle, Mass. says: "Before I took this course I was a lowly bit twiddler. Now with what I learned at GUE Tech I feel really important and can obfuscate and confuse with the best."

                                     Dr. Blank had this to say: "Ten short days ago all I could look forward to was a dead-end job as a doctor. Now I have a promising future and make really big Zorkmids."

                                     GUE Tech can't promise these fantastic results to everyone. But when you earn your degree from GUE Tech, your future will be brighter.
                                     """;

    public string OnTheGroundDescription => NeverPickedUpDescription;

    public override string NeverPickedUpDescription =>
        "There is a matchbook whose cover says \"Visit Beautiful FCD#3\" here. ";

    public bool IsOn
    {
        get => Lit;
        set => Lit = value;
    }

    public string NowOnText => "One of the matches starts to burn.";

    public string NowOffText { get; }

    public string AlreadyOffText { get; }

    public string AlreadyOnText { get; }

    public string? CannotBeTurnedOnText => MatchesUsed == 5 ? "I'm afraid that you have run out of matches. " : "";

    public void OnBeingTurnedOn(IContext context)
    {
        TurnsRemainingLit = 1;
        MatchesUsed++;
        Lit = true;
        context.RegisterActor(this);
    }

    public void OnBeingTurnedOff(IContext context)
    {
        Lit = false;
    }

    public string? Act(IContext context)
    {
        if (!Lit)
        {
            context.RemoveActor(this);
            return "";
        }

        if (TurnsRemainingLit == 1)
        {
            TurnsRemainingLit = 0;
            return "";
        }

        TurnsRemainingLit = 0;
        Lit = false;
        
        // TODO: Was this our only light source? Tell them it's dark 
        
        return "The match has gone out.";
    }
    
}