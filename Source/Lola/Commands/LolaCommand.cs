using Task = System.Threading.Tasks.Task;
using ValidationException = DotNetToolbox.Results.ValidationException;

namespace Lola.Commands;

public abstract class LolaCommand<TCommand>(IHasChildren parent, string command, Action<TCommand>? configure = null)
    : Command<TCommand>(parent, command, configure)
    where TCommand : LolaCommand<TCommand> {
    protected string ErrorText { get; set; } = string.Empty;

    protected virtual Result HandleCommand()
        => throw new NotImplementedException();
    protected sealed override Result Execute() {
        try {
            var result = HandleCommand();
            Output.WriteLine();
            return result;
        }
        catch (Exception ex) {
            return HandleException(ex);
        }
    }

    protected virtual Task<Result> HandleCommandAsync(CancellationToken ct = default)
        => Task.Run(HandleCommand, ct);

    protected sealed override async Task<Result> ExecuteAsync(CancellationToken ct = default) {
        try {
            var result = await HandleCommandAsync(ct);
            Output.WriteLine();
            return result;
        }
        catch (ValidationException ex) {
            return HandleValidationErrors(ex);
        }
        catch (Exception ex) {
            return HandleException(ex);
        }
    }

    private Result HandleException(Exception ex) {
        Logger.LogError(ex, "{Command} Error!", typeof(TCommand).Name);
        Output.WriteError(ex, $"An error has occurred while {ErrorText}.");
        Output.WriteLine();
        return Result.Error(ex);
    }

    private Result HandleValidationErrors(ValidationException ex) {
        var errors = string.Join("\n", ex.Errors.Select(e => $" - {e.Source}: {e.Message}"));
        Logger.LogError(ex, "{Command} Validation Error!\n{Errors}", typeof(TCommand).Name, errors);
        Output.WriteLine($"[red]We found some problems while {ErrorText}.\nPlease correct the following errors and try again.\n{errors}[/]");
        return Result.Invalid(ex.Errors);
    }
}
