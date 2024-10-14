using GameEngine;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class CyclopsCombatEngine : ICombatEngine
{
    /// <summary>
    /// This is all there is to combat with the cyclops. He never attacks you until the one
    /// time he does, which is fatal. Your attacks do nothing. 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="weapon"></param>
    /// <returns></returns>
    public InteractionResult Attack(IContext context, IWeapon? weapon)
    {
        var cyclops = Repository.GetItem<Cyclops>();
        cyclops.IsAgitated = true;
        cyclops.IsSleeping = false;
        return new PositiveInteractionResult("The cyclops shrugs but otherwise ignores your pitiful attempt. \n");
    }
}