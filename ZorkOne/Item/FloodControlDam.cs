using GameEngine;
using GameEngine.Item;
using ZorkOne.Location;

namespace ZorkOne.Item;

/// <summary>
/// An examinable stand-in for Flood Control Dam #3 itself. In the original Zork I source the dam is
/// an object (SYNONYM DAM GATE GATES) the player can examine, but this port models the dam as a
/// <see cref="Dam" /> location. Without an object carrying these nouns the parser — whose vocabulary
/// is reflected from every item's <c>NounsForMatching</c> — could not resolve "examine dam" or
/// "examine gates" at all. This fixed, non-takeable item restores that vocabulary and reports the
/// live sluice-gate state. (Issue #228.)
/// </summary>
public class FloodControlDam : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching =>
        ["dam", "gates", "sluice gate", "sluice gates", "flood control dam", "flood control dam #3", "dam #3"];

    public override string Name => "dam";

    public string ExaminationDescription => Repository.GetLocation<Dam>().GatesDescription();
}
