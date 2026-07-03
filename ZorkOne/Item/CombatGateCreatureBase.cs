using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace ZorkOne.Item;

/// <summary>
/// Issue #374: shared "can be killed via combat or god mode" machinery for Zork I's three
/// ICanBeAttacked creatures (Troll, Thief, Cyclops). Centralizes the IsDead flag, the once-only
/// guard, and the ICanBeAttacked.GodModeKill implementation, so a real combat engine's DeathBlow
/// and "god mode kill" can never drift out of sync - each subclass only implements OnDeath for its
/// own creature-specific removal effects (dropping held items, clearing actor registration,
/// vanishing from the world).
/// </summary>
public abstract class CombatGateCreatureBase : ContainerBase, ICanBeAttacked
{
    public bool IsDead { get; set; }

    protected abstract void OnDeath(IContext context);

    internal void Die(IContext context)
    {
        if (IsDead)
            return;

        IsDead = true;
        OnDeath(context);
    }

    public bool GodModeKill(IContext context)
    {
        Die(context);
        return true;
    }
}
