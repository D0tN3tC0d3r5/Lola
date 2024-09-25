namespace Lola.Utilities;

internal sealed class ListItem<TItem, TKey>(TKey key, string text, TItem? item)
    where TItem : class, IEntity<TKey>
    where TKey : notnull {
    public TKey Key { get; } = key;
    public string Text { get; } = text;
    public TItem? Item { get; } = item;
}

public static class CommandHelpers {
    public static async Task<TItem?> SelectEntityAsync<TItem, TKey>(this ICommand command,
                                                                    IEnumerable<TItem> entities,
                                                                    Func<TItem, string> mapText,
                                                                    CancellationToken ct = default)
        where TItem : class, IEntity<TKey>
        where TKey : notnull {
        var items = IsNotNull(entities).ToArray();
        if (items.Length == 0) {
            command.Output.WriteLine("[yellow]No items found.[/]");
            return null;
        }

        var choices = items.ToList(e => new ListItem<TItem, TKey>(e.Id, IsNotNull(mapText)(e), e));
        var cancelOption = new ListItem<TItem, TKey>(default!, "Cancel", null);
        choices.Add(cancelOption);

        const string prompt = "Select an item or cancel to return:";
        return (await command.Input.BuildSelectionPrompt<ListItem<TItem, TKey>>(prompt)
                                   .AddChoices([.. choices])
                                   .ConvertWith(e => e.Item is null ? $"[yellow bold]{e.Text}[/]" : e.Text)
                                   .ShowAsync(ct)).Item;
    }
}
