using Model.Interaction;
using Model.Item;

namespace Model.Interface;

public interface ICombatEngine
{
    InteractionResult? Attack(IContext context, IWeapon? weapon);
    PositiveInteractionResult DeathBlow(IContext context, string attackText);
    PositiveInteractionResult Knockout(string text);
}