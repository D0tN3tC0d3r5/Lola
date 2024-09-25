namespace Lola.Main.Commands;

public class SettingsCommand(IHasChildren parent, IOptions<LolaSettings> settings)
    : LolaCommand<SettingsCommand>(parent, "Settings", n => {
        n.Aliases = ["set"];
        n.Description = "Show settings";
        n.ErrorText = "displaying the settings";
        n.Help = "Display the current configuration of Lola.";
    }) {
    private readonly LolaSettings _settings = settings.Value;

    protected override Result HandleCommand() {
        Logger.LogInformation("Executing Settings command...");
        DrawTable();
        return Result.Success();
    }

    private void DrawTable() {
        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.AddRow("Default AI Provider", _settings.DefaultAIProvider);
        table.AddRow("Available Models", string.Join(", ", _settings.AvailableModels));
        Output.Write(table);
        Output.WriteLine();
    }
}
