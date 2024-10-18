using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;

namespace ZorkOne.Item;

public class Thief : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings
{
    
    // You hear a scream of anguish as you violate the robber's hideaway. Using passages unknown to you, he rushes to its defense.
    // The thief gestures mysteriously, and the treasures in the room suddenly vanish.
    
    // 
    
    /*
      
    No effect: 
      The thief stabs nonchalantly with his stiletto and misses.
      You dodge as the thief comes in low.
      You parry a lightning thrust, and the thief salutes you with a grim nod.
      The thief tries to sneak past your guard, but you twist away.
      The thief stabs nonchalantly with his stiletto and misses.
      
    Small Wound
      The thief slowly approaches, strikes like a snake, and leaves you wounded.
      The stiletto flashes faster than you can follow, and blood wells from your leg.
      A quick thrust pinks your left arm, and blood starts to trickle down.
      The thief draws blood, raking his stiletto across your arm.
      
    Knockout      
      The thief knocks you out. 
      Shifting in the midst of a thrust, the thief knocks you unconscious with the haft of his stiletto. 

    Stun You:
      The thief attacks, and you fall back desperately.
      The thief rams the haft of his blade into your stomach, leaving you out of breath.
      The butt of his stiletto cracks you on the skull, and you stagger back.
      

    Kill You:
  
      Finishing you off, the thief inserts his blade into your heart.
      The thief neatly flips your nasty knife out of your hands, and it drops to the floor.
      The stiletto severs your jugular.  It looks like the end.
      The thief bows formally, raises his stiletto, and with a wry grin, ends the battle and your life.
      The thief comes in from the side, feints, and inserts the blade into your ribs.
      
      After Knockout:
        The thief, forgetting his essentially genteel upbringing, cuts your throat.
        The thief, a pragmatist, dispatches you as a threat to his livelihood.
        The thief amuses himself by searching your pockets.




    No effect: 
      Clang! Crash! The thief parries.
      The thief is struck on the arm; blood begins to trickle down.
      A good stroke, but it's too slow; the thief dodges.
      You charge, but the thief jumps nimbly aside.
      A good slash, but it misses the thief by a mile.
      A quick stroke, but the thief is on guard.
      You parry a lightning thrust, and the thief salutes you with a grim nod.
      Your sword misses the thief by an inch.

    Drop My Sword:
      You parry a low thrust, and your sword slips out of your hand.

    Killed:
      It's curtains for the thief as your nasty knife removes his head.

    He's Stunned: 
      The thief is momentarily disoriented and can't fight back.
      The force of your blow knocks the thief back, stunned.
      The thief is confused and can't fight back. The thief slowly regains his feet.
      The thief is staggered, and drops to his knees. The thief slowly regains his feet.
    
    Disarmed:
      The thief's weapon is knocked to the floor, leaving him unarmed. The robber, somewhat surprised at this turn of events, nimbly retrieves his stiletto.

    I'm Stunned:
      You are still recovering from that last blow, so your attack is ineffective.


    Knocked Out: 
      The thief drops to the floor, unconscious.
      The thief is knocked out!
      The haft of your knife knocks out the thief.
      The thief is battered into unconsciousness.
      A furious exchange, and the thief is knocked out!
      Your sword crashes down, knocking the thief into dreamland.
      


      There is a suspicious-looking individual lying unconscious on the ground.
      The unarmed thief cannot defend himself:  He dies.
      The robber revives, briefly feigning continued unconsciousness, and, when he sees his moment, scrambles away from you.


    >take stiletto
    The stiletto seems white-hot. You can't hold on to it.

    >take stiletto
    The thief swings it out of your reach.

    !!!! Important: The chalice is now safe to take.
        Almost as soon as the thief breathes his last breath, a cloud of sinister black fog envelops him, and when the fog lifts, the carcass has disappeared.
        As the thief dies, the power of his magic decreases, and his treasures reappear:
          A stiletto
          A crystal skull
          A trunk of jewels
          A torch
          A sapphire-encrusted bracelet
  

     */
    
    public override string[] NounsForMatching => ["suspicious-looking individual", "thief", "man", "robber", "gentleman", "guy", "dude", "footpad", "crook", "criminal", "gent", "bandit"];
    
    public override void Init()
    {
        throw new NotImplementedException();
    }

    public string ExaminationDescription => "The thief is a slippery character with beady eyes that flit back and forth. He carries, along with an unmistakable arrogance, a large bag over his shoulder and a vicious stiletto, whose blade is aimed menacingly in your direction. I'd watch out if I were you. ";
    
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        throw new NotImplementedException();
    }

    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        // Your proposed victim suddenly recovers consciousness.
        // The thief is taken aback by your unexpected generosity, but accepts the jewel-encrusted egg and stops to admire its beauty.
        // The thief places the glass bottle in his bag and thanks you politely.

        throw new NotImplementedException();
    }
}