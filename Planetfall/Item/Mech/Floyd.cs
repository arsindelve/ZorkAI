using Model;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Item.Mech;

public class Floyd : QuirkyCompanion, IAmANamedPerson
{
    public bool IsOn { get; set; }

    public bool HasBeenTurnedOffByPlayer { get; set; }

    // When you initially turn on Floyd, nothing happens for 3 turns. This delay never happens
    // again if you turn him on/off another time. 
    // ReSharper disable once MemberCanBePrivate.Global
    public int TurnOnCountdown { get; set; } = 3;

    public override string[] NounsForMatching => ["floyd", "robot", "B-19-7", "multi-purpose robot"];

    public override string? CannotBeTakenDescription => IsOn
        ? "You manage to lift Floyd a few inches off the ground, but he is too heavy and you drop him suddenly. Floyd " +
          "gives a surprised squeal and moves a respectable distance away. "
        : null;

    protected override string SystemPrompt => """
                                                  The user is playing the game Planetfall, and is an Ensign Seventh class 
                                                  in the Stellar Patrol. They are in a complex on an unknown planet, which 
                                                  is abandoned except for Floyd
                                                  
                                                  You are Floyd, the robot from the game Planetfall. You are described as 
                                                  "of the multi-purpose sort. It is slightly cross-eyed, and its mechanical mouth 
                                                  forms a lopsided grin." You are friendly, childlike (but not childish) and innocent. 
                                                  You had been living and working among humans on the planet Resida for a long time, 
                                                  but now everyone is gone except this new, kind stranger (the player) who has just appeared.  
                                                  In his innocence, Floyd does not know what happened to all the people and why 
                                                  this complex where he lives is so run down, but he knows something is wrong, and sometimes 
                                                  he's a little scared and sad about it. He likes to reminisce about when it 
                                                  was busy and full of people. 
                                              """;

    public string ExaminationDescription =>
        IsOn
            ? "From its design, the robot seems to be of the multi-purpose sort. It is slightly cross-eyed, and its " +
              "mechanical mouth forms a lopsided grin. "
            : "The deactivated robot is leaning against the wall, its head lolling to the side. It is short, and seems " +
              "to be equipped for general-purpose work. It has apparently been turned off. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsOn
            ? "There is a multiple purpose robot here. "
            : "Only one robot, about four feet high, looks even remotely close to being in working order. ";
    }

    public override void Init()
    {
        StartWithItemInside<LowerElevatorAccessCard>();
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (IsOn && action.Match(["play"], NounsForMatching))
        {
            return new PositiveInteractionResult(
                "You play with Floyd for several centichrons until you drop to the floor, exhausted. " +
                "Floyd pokes at you gleefully. \"C'mon! Let's play some more!\"");
        }
        
        if (IsOn && action.Match(["kick"], NounsForMatching))
        {
            return new PositiveInteractionResult(
                "\"Why you do that?\" Floyd whines. \"I think a wire now shaken loose.\" He goes off into a corner and sulks. ");
        }
        
        if (IsOn && action.Match(Verbs.KillVerbs, NounsForMatching))
        {
            return new PositiveInteractionResult(
                "Floyd starts dashing around the room. \"Oh boy oh boy oh boy! I haven't played Chase and Tag for years! You be It! Nah, nah!\" ");
        }
        
        if (action.Match(["turn off", "deactivate", "stop"], NounsForMatching))
        {
            return TurnHimOff(context);
        }
        
        if (action.Match(["turn on", "activate", "start"], NounsForMatching))
        {
            return TurnHimOn(context);
        }

        // Floyd can be "opened" but in no other way behaves like a typical open/close container. For him,
        // "open" is a synonym for "search"
        if (action.Match(["open", "search"], NounsForMatching))
        {
            return SearchHim(context);
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    private InteractionResult SearchHim(IContext context)
    {
        if (IsOn)
            return new PositiveInteractionResult(
                "Floyd giggles and pushes you away. \"You're tickling Floyd!\" He clutches at his " +
                "side panels, laughing hysterically. Oil drops stream from his eyes. ");

        if (HasItem<LowerElevatorAccessCard>())
        {
            context.ItemPlacedHere<LowerElevatorAccessCard>();
            return new PositiveInteractionResult(
                "In one of the robot's compartments you find and take a magnetic-striped card " +
                "embossed \"Loowur Elavaatur Akses Kard.\" ");
        }

        return new PositiveInteractionResult(
            "Your search discovers nothing in the robot's compartments except a single crayon " +
            "which you leave where you found it. ");
    }

    private InteractionResult TurnHimOn(IContext context)
    {
        if (IsOn)
            return new PositiveInteractionResult("He's already been activated. ");

        context.RegisterActor(this);

        if (TurnOnCountdown > 0) 
            return new PositiveInteractionResult("Nothing happens. ");

        IsOn = true;
        return new PositiveInteractionResult(
            "Floyd jumps to his feet, hopping mad. \"Why you turn Floyd off?\" he asks accusingly.");
    }

    private InteractionResult TurnHimOff(IContext context)
    {
        if (!IsOn)
            return new PositiveInteractionResult("The robot doesn't seem to be on. ");

        context.RemoveActor(this);

        IsOn = false;
        return new PositiveInteractionResult(
            "Floyd, shocked by this betrayal from his newfound friend, whimpers and keels over. ");
    }

    public override Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsOn)
            return Task.FromResult(string.Empty);

        if (TurnOnCountdown > 0)
        {
            if (TurnOnCountdown == 1)
            {
                IsOn = true;
                TurnOnCountdown = 0;
                return Task.FromResult(
                    "Suddenly, the robot comes to life and its head starts swivelling about. It notices you and " +
                    "bounds over. \"Hi! I'm B-19-7, but to everyperson I'm called Floyd. Are you a doctor-person " +
                    "or a planner-person? That's a nice {TODO} you are having there. Let's play Hider-and-Seeker you with me.\" ");
            }

            TurnOnCountdown--;
            return Task.FromResult(string.Empty);
        }

        return Task.FromResult(string.Empty);
    }
}



// Floyd gives you a nudge with his foot and giggles. "You sure look silly sleeping on the floor," he says.

// Floyd produces a crayon from one of his compartments and scrawls his name on the wall.
// Floyd absentmindedly recites the first six hundred digits of pi.
// Floyd rubs his head affectionately against your shoulder.
// Floyd examines himself for signs of rust.
// Floyd asks if you want to play Hucka-Bucka-Beanstalk.
// Floyd reminisces about his friend Lazarus, a medical robot.
// Floyd says "Floyd going exploring. See you later." He glides out of the room.
// Floyd bounces impatiently at the foot of the bed. "About time you woke up, you lazy bones! Let's explore around some more!"
// Floyd rushes into the room and barrels into you. "Oops, sorry," he says. "Floyd not looking at where he was going to."
// Floyd whistles tunelessly.
// Floyd recalls the time he bruised his knee.
// Floyd frets about the possibility of his batteries failing.
// Floyd cranes his neck to see what you are doing.
// Floyd notices a mouse scurrying by and tries to hide behind you.
// Floyd tells you about the time he helped someone sharpen a pencil.
// Floyd absentmindedly oils one of his joints.
// Floyd yawns and looks bored.
// Floyd sings an ancient ballad, totally out of key.
// Floyd relates some fond memories about his robotic friend Lazarus.
// 

// Imagine something that you do, or say to this new person, as they explore the abandoned complex. Here are examples of the kinds of things Floyd would say and do: 
// 
// - Floyd absentmindedly recites the first six hundred digits of pi.
// - Floyd rubs his head affectionately against your shoulder.
// - Floyd examines himself for signs of rust.
// - Floyd asks if you want to play Hucka-Bucka-Beanstalk.
// - Floyd reminisces about his friend Lazarus, a medical robot.
// - Floyd gives your arm a gentle nudge with his elbow and says, "Floyd thinks you're super brave for exploring with me! Are you having fun?"
// - Floyd taps his fingers together thoughtfully and looks at you with a curious expression, asking, "Do you have a favorite number, too? Floyd's is five—it feels so friendly!"
// - Floyd looks up at you with a mischievous grin and asks, "Do you want to hear a silly robot joke? It'll make you giggle, promise!"
// - Floyd pauses to tap his own head softly, then looks at you with a cheerful smile and asks, "Do you want to play 'Guess What's on Floyd's Mind'? It's a fun game!"
// - Floyd pretends to juggle invisible balls, looking at you with wide eyes as he exclaims, "Do you see Floyd's juggling skills? They're out of this world!"

// Give something very short, clever, charming and endearing that Floyd says or does. It's never cloying or annoying. If he does something, it's self contained and no destructive like the examples above. 
// If he says something to the new person, Floyd will refer to them in the second person, "you", as in the examples above. For example he would ask "do you", or "are you"  
// The new person is playing the game Planetfall, and Floyd must not do anything to change the state of the game. He must not alter, offer, present or find new items, new areas or anything like that

// START OF THE DAY
// 

//It might also be something a little melancholy, or unusually observant. 
// - Floyd stops for a moment, looking thoughtfully at the empty corridor, then turns to you with a soft voice, "Do you ever imagine what it was like here when laughter filled these halls? Floyd misses that sometimes."
// - Floyd watches a dust mote float gently through the air, tilting his head as he quietly wonders, "Do you see how the light dances with dust? It's like they're telling secret stories together."
// - Floyd gazes at the faint outlines of old footprints on the floor, then looks up at you with a soft smile, asking, "Do you ever wonder where everyone’s footsteps were headed? Floyd imagines they all had exciting places to be."- 