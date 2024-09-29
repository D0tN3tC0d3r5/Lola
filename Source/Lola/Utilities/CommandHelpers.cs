namespace Lola.Utilities;

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

        const string prompt = "Select an item or cancel to return:";
        return await command.Input.BuildSelectionPrompt<TItem, TKey>(prompt, e => e.Id)
                                   .AddChoices(items)
                                   .AddChoice(default!, "[yellow]Cancel[/]", ChoicePosition.AtEnd)
                                   .DisplayAs(mapText)
                                   .ShowAsync(ct);
    }
}
