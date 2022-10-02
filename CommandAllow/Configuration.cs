using Newtonsoft.Json;
using TShockAPI;

namespace CommandAllow;

public class Configuration
{
    // TODO: Change the config.json file name to a more specific one
    private static readonly string _uri = Path.Combine(TShock.SavePath, "commandallow.json");

    #region ConfigVariables

    public string CommandAllowPermission = "commandallow.{0}";
    public string CommandAllowOverride = "commandallow.{0}.override";
    public string CommandOverrideAll = "commandallow.overrideall";

    // Has an example in the config for the /slap command.s
    public Dictionary<string, CommandDetails> InterceptCommands = new()
    {
        ["slap"] = new CommandDetails(0,
            "You have enabled slap protection!",
            "You have disabled slap protection!",
            "You cannot slap this player!",
            "Toggles whether other people can slap you"),
    };

    public Dictionary<string, string> Defaults = new()
    {
        ["CommandEnabled"] = "You have enabled {0} protection!",
        ["CommandDisable"] = "You have disabled {0} protection!",
        ["CommandCannotUse"] = "You cannot {0} this player!",
        ["CommandHelpText"] = "Toggles whether other people can {0} you.",
    };

    public Dictionary<string, string> Messages = new()
    {
        ["InvalidParameterCount"] = "Invalid Syntax! Proper syntax: /commandallow add <command> <position> of player name>.",
        ["CommandNotExist"] = "The command {0} does not exist on this server!",
        ["CommandAlreadyIntercepted"] = "{0} has already been added!",
        ["CommandInvalidIndex"] = "Please insert a valid index number, eg: /commandallow add slap 0",
        ["CommandAddSuccess"] = "{0} has successfully been added to the list!",
        ["RemoveNotExist"] = "{0} does not exist. Add it with /commandallow add {0}.",
        ["CommandRemoveSuccess"] = "{0} has successfully been removed from the list.",
        ["ListCommand"] = "Following commands have an extension: {0}",
        ["ListCommandEmpty"] = "There are no commands with an extension. Add one with /commandallow add.",
        ["InvalidParameter"] = "Invalid Syntax! Proper syntax: /commandallow <add|remove> <command name>."
    };

    #endregion ConfigVariables

    public static Configuration Read() =>
        !File.Exists(_uri) ? new Configuration().Write()
        : JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(_uri));

    public Configuration Write()
    {
        if (!File.Exists(_uri))
            (new FileInfo(_uri)).Directory.Create();

        File.WriteAllText(_uri,
            JsonConvert.SerializeObject(this, Formatting.Indented));

        return this;
    }
}
