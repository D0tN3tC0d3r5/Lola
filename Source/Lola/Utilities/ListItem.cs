namespace Lola.Utilities;

internal sealed class ListItem<TItem, TKey>(TKey key, string text, TItem? item)
    where TItem : class, IEntity<TKey>
    where TKey : notnull {
    public TKey Key { get; } = key;
    public string Text { get; } = text;
    public TItem? Item { get; } = item;
}
