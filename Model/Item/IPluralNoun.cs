namespace Model.Item;

/// <summary>
/// Marker interface for objects that are plural, (or can be plural) but considered as a single item like "matches", "candles" etc.
/// Another example is "bag of coins" which can be referred to as "bag" (single) or "coins" (plural).
/// We need to know this so that we can properly refer to the item as "them" instead of, or in addition to "it". E.g. "examine them" or
/// "drop them"
/// </summary>
public interface IPluralNoun;