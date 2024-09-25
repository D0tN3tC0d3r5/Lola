namespace Lola.Jobs.Commands;

public class JobsMainMenu(IHasChildren parent)
    : LolaCommand<JobsMainMenu>(parent, "Jobs", n => {
        n.Description = "Manage Jobs";
        n.ErrorText = "displaying the job's main menu";
        n.AddCommand<ListJobs>();
        //n.AddCommand<TaskCreateCommand>();
        //n.AddCommand<TaskUpdateCommand>();
        //n.AddCommand<TaskRemoveCommand>();
        //AddCommand<TaskViewCommand>();
        n.AddCommand<HelpCommand>();
    }) {
    protected override async Task<Result> HandleCommandAsync(CancellationToken ct = default) {
        Logger.LogInformation("Executing Jobs command...");
        var choice = await Input.BuildSelectionPrompt<string>("What would you like to do?")
                                .ConvertWith(MapTo)
                                .AddChoices(Commands.ToArray(c => c.Name))
                                .ShowAsync(ct);

        var command = Commands.FirstOrDefault(i => i.Name == choice);
        return command is null
            ? Result.Success()
            : await command.Execute([], ct);

        string MapTo(string item) => Commands.FirstOrDefault(i => i.Name == item)?.Description ?? string.Empty;
    }
}
