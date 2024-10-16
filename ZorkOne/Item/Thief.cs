using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;

namespace ZorkOne.Item;

public class Thief : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings
{
    
    // You hear a scream of anguish as you violate the robber's hideaway. Using passages unknown to you, he rushes to its defense.
    // The thief gestures mysteriously, and the treasures in the room suddenly vanish.
    
    // The thief places the glass bottle in his bag and thanks you politely.
    
    // The thief is a slippery character with beady eyes that flit back and forth. He carries, along with an unmistakable arrogance, a large bag over his shoulder and a vicious stiletto, whose blade is aimed menacingly in your direction. I'd watch out if I were you.
    
    /*
      
      The thief stabs nonchalantly with his stiletto and misses.
      You dodge as the thief comes in low.
      You parry a lightning thrust, and the thief salutes you with a grim nod.
      The thief tries to sneak past your guard, but you twist away.
      
      The stiletto severs your jugular.  It looks like the end.
      The thief comes in from the side, feints, and inserts the blade into your ribs.
      The thief bows formally, raises his stiletto, and with a wry grin, ends the battle and your life.

      A quick thrust pinks your left arm, and blood starts to trickle down.
      The thief draws blood, raking his stiletto across your arm.
      The stiletto flashes faster than you can follow, and blood wells from your leg.

     The butt of his stiletto cracks you on the skull, and you stagger back.
     The thief attacks, and you fall back desperately.
     The thief rams the haft of his blade into your stomach, leaving you out of breath.
     
     
     Carry out attacking the thief with something:
     
    Your sword crashes down, knocking the thief into dreamland.[or]
    The thief is battered into unconsciousness.[or]
    A furious exchange, and the thief is knocked out![at random]";
  
    The haft of your knife knocks out the thief.[or]
    The thief drops to the floor, unconscious.[or]
    The thief is knocked out![at random]";
     
     The thief receives a deep gash in his side.[or]
     A savage blow on the thigh!  The thief is stunned but can still fight!
     Slash!  Your blow lands!  That one hit an artery, it could be serious!
     
  The thief receives a deep gash in his side.[or]
  A savage cut on the leg stuns the thief, but he can still fight![or]
  Slash!  Your stroke connects!  The thief could be in serious trouble!
  
   "A good stroke, but it's too slow, the thief dodges.";
  
  end if;
  change the engrossed of the thief to false.
  
  
<GLOBAL THIEF-MELEE
<TABLE (PURE)
 <LTABLE (PURE)
  <LTABLE (PURE) "The thief stabs nonchalantly with his stiletto and misses.">
  <LTABLE (PURE) "You dodge as the thief comes in low.">
  <LTABLE (PURE) "You parry a lightning thrust, and the thief salutes you with
a grim nod.">
  <LTABLE (PURE) "The thief tries to sneak past your guard, but you twist away.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "Shifting in the midst of a thrust, the thief knocks you unconscious
with the haft of his stiletto.">
  <LTABLE (PURE) "The thief knocks you out.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "Finishing you off, the thief inserts his blade into your heart.">
  <LTABLE (PURE) "The thief comes in from the side, feints, and inserts the blade
into your ribs.">
  <LTABLE (PURE) "The thief bows formally, raises his stiletto, and with a wry grin,
ends the battle and your life.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "A quick thrust pinks your left arm, and blood starts to
trickle down.">
  <LTABLE (PURE) "The thief draws blood, raking his stiletto across your arm.">
  <LTABLE (PURE) "The stiletto flashes faster than you can follow, and blood wells
from your leg.">
  <LTABLE (PURE) "The thief slowly approaches, strikes like a snake, and leaves
you wounded.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "The thief strikes like a snake! The resulting wound is serious.">
  <LTABLE (PURE) "The thief stabs a deep cut in your upper arm.">
  <LTABLE (PURE) "The stiletto touches your forehead, and the blood obscures your
vision.">
  <LTABLE (PURE) "The thief strikes at your wrist, and suddenly your grip is slippery
with blood.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "The butt of his stiletto cracks you on the skull, and you stagger
back.">
  <LTABLE (PURE) "The thief rams the haft of his blade into your stomach, leaving
you out of breath.">
  <LTABLE (PURE) "The thief attacks, and you fall back desperately.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "A long, theatrical slash. You catch it on your " F-WEP ", but the
thief twists his knife, and the " F-WEP " goes flying.">
  <LTABLE (PURE) "The thief neatly flips your " F-WEP " out of your hands, and it drops
to the floor.">
  <LTABLE (PURE) "You parry a low thrust, and your " F-WEP " slips out of your hand.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "The thief, a man of superior breeding, pauses for a moment to consider the propriety of finishing you off.">
  <LTABLE (PURE) "The thief amuses himself by searching your pockets.">
  <LTABLE (PURE) "The thief entertains himself by rifling your pack.">>
 <LTABLE (PURE)
  <LTABLE (PURE) "The thief, forgetting his essentially genteel upbringing, cuts your
throat.">
  <LTABLE (PURE) "The thief, a pragmatist, dispatches you as a threat to his
livelihood.">>>>

     */
    public override string[] NounsForMatching { get; }
    public override void Init()
    {
        throw new NotImplementedException();
    }

    public string ExaminationDescription { get; }
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        throw new NotImplementedException();
    }

    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        throw new NotImplementedException();
    }
}