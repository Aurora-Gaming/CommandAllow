namespace CommandAllow;

/// <summary>
/// Stores all relevant information for each command to be intercepted.
/// </summary>
public struct CommandDetails
{
    public int NameIndex;
    public string CommandEnabled;
    public string CommandDisabled;
    public string CommandCannotUse;
    public string CommandHelpText;

    public CommandDetails(int position, string commandEnabled, string commandDisabled, string commandCannotUse, string commandHelpText)
    {
        NameIndex = position;
        CommandEnabled = commandEnabled;
        CommandDisabled = commandDisabled;
        CommandCannotUse = commandCannotUse;
        CommandHelpText = commandHelpText;
    }
}
