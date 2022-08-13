using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace CommandAllow
{
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin
    {
        public override string Author => "RussianMushroom";
        public override string Description => "Mimics the functionality of /tpallow and /wallow and extends them to other commands.";
        public override string Name => "CommandAllow";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public Configuration Config;

        public List<Command> AllowCommand = new List<Command>();

        public Main(Terraria.Main game) : base(game)
        {
            Order = 1;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInit);
            ServerApi.Hooks.WorldSave.Register(this, OnSave);

            PlayerHooks.PlayerCommand += OnPlayerCommand;

            GeneralHooks.ReloadEvent += OnReload;

        }

        private void OnPlayerCommand(PlayerCommandEventArgs e)
        {
            if (e.Player == null) return;

            if (e.Player.HasPermission(Config.CommandOverrideAll)) return;

            if (!Config.InterceptCommands.ContainsKey(e.CommandName)) return;

            if (e.Player.HasPermission(Config.CommandAllowOverride.SFormat(e.CommandName))) return;

            if (!Commands.ChatCommands.First(c => c.Name == e.CommandName).CanRun(e.Player)) return;

            CommandDetails interceptCommand = Config.InterceptCommands[e.CommandName];

            if (interceptCommand.NameIndex > e.Parameters.Count - 1) return;

            List<TSPlayer> players = TSPlayer.FindByNameOrID(e.Parameters[interceptCommand.NameIndex]);

            if (players.Count != 1) return;

            if (!players.First().GetData<bool>($"{e.CommandName}allow")) return;

            e.Handled = true;
            e.Player.SendErrorMessage(interceptCommand.CommandCannotUse, players.First().Name);
        }

        private void OnSave(WorldSaveEventArgs args)
        {
            Config.Write();
        }

        private void OnReload(ReloadEventArgs e)
        {
            // Load values from the configuration file
            Config = Configuration.Read();
            e?.Player?.SendSuccessMessage($"[{Name}] Successfully reloaded config.");

            AllowCommand.ForEach(command => Commands.ChatCommands.Remove(command));
            Config.InterceptCommands.ForEach(interceptCommand => AddCommand(interceptCommand));
        }

        private void OnInit(EventArgs args)
        {
            Config = Configuration.Read();

            Commands.ChatCommands.Add(new Command("commandallow.admin", OnCommand, "ca", "commandallow")
            {
                HelpText = "Allows for players to toggle receiving certain commands. Mimics the functionality of /wallow or /tpallow"
                
            });
            Config.InterceptCommands.ForEach(interceptCommand => AddCommand(interceptCommand));
        }

        private void OnCommand(CommandArgs args)
        {
            if (args.Parameters.Count > 3 || args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage(Config.Messages["InvalidParameterCount"]);
                return;
            }

            switch (args.Parameters.FirstOrDefault())
            {
                case "add":
                case "a":
                    {
                        if (Config.InterceptCommands.ContainsKey(args.Parameters[1].ToLower()))
                        {
                            args.Player.SendErrorMessage(Config.Messages["CommandAlreadyIntercepted"], args.Parameters[1]);
                            return;
                        }

                        // Default value for a player name, eg: /teleport RussianMushroom
                        int playerNameIndex = 0;
                        string commandName = args.Parameters[1].ToLower();

                        if (!Commands.ChatCommands.Select(c => c.Name).Contains(commandName))
                        {
                            args.Player.SendErrorMessage(Config.Messages["CommandNotExist"], commandName);
                            return;
                        }

                        if (args.Parameters.Count == 3)
                        {
                            if (!int.TryParse(args.Parameters[2], out playerNameIndex))
                            {
                                args.Player.SendErrorMessage(Config.Messages["CommandInvalidIndex"]);
                                return;
                            }
                        }

                        AddCommand(playerNameIndex, commandName);
                        args.Player.SendSuccessMessage(Config.Messages["CommandAddSuccess"], commandName, $"/{commandName}allow");

                        break;
                    }

                case "remove":
                case "r":
                    {
                        string commandName = args.Parameters[1].ToLower();

                        if (!Config.InterceptCommands.ContainsKey(commandName))
                        {
                            args.Player.SendErrorMessage(Config.Messages["RemoveNotExist"], commandName);
                            return;
                        }

                        RemoveCommand(commandName);
                        args.Player.SendSuccessMessage(Config.Messages["CommandRemoveSuccess"], $"/{commandName}allow");
                        break;
                    }
                case "list":
                    if (Config.InterceptCommands.Count > 0)
                        args.Player.SendInfoMessage(Config.Messages["ListCommand"], string.Join(", ", Config.InterceptCommands.Keys));
                    else
                        args.Player.SendInfoMessage(Config.Messages["ListCommandEmpty"]);
                    break;

                default:
                    args.Player.SendErrorMessage(Config.Messages["InvalidParameter"]);
                    break;
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                ServerApi.Hooks.GameInitialize.Deregister(this, OnInit);
                ServerApi.Hooks.WorldSave.Deregister(this, OnSave);

                PlayerHooks.PlayerCommand -= OnPlayerCommand;

                GeneralHooks.ReloadEvent -= OnReload;

                // Save values to the configuration file
                Config.Write();
            }

            base.Dispose(disposing);
        }

        #region Helper Methods

        /// <summary>
        /// Commands of the format /[command]allow are added to the list of commands.
        /// Enable, disable, warn and help texts are set to default config values and can be edited 
        /// after the fact in the config.
        /// </summary>
        /// <param name="playerNameIndex">The zero position of the player name in the original command</param>
        /// <param name="commandName"></param>
        private void AddCommand(int playerNameIndex, string commandName)
        {
            CommandDetails commandDetails = new CommandDetails(playerNameIndex,
                Config.Defaults["CommandEnabled"].SFormat(commandName),
                Config.Defaults["CommandDisable"].SFormat(commandName),
                Config.Defaults["CommandCannotUse"].SFormat(commandName),
                Config.Defaults["CommandHelpText"].SFormat(commandName));

            Config.InterceptCommands.Add(commandName, commandDetails);
            AddCommand(commandName, commandDetails);
        }

        /// <summary>
        /// <inheritdoc cref="AddCommand(int, string)"/>
        /// Loads in commands from the config
        /// </summary>
        /// <param name="command"></param>
        private void AddCommand(KeyValuePair<string, CommandDetails> command) => AddCommand(command.Key, command.Value);

        /// <summary>
        /// <inheritdoc cref="AddCommand(KeyValuePair{string, CommandDetails})"/>
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="commandDetails"></param>
        private void AddCommand(string commandName, CommandDetails commandDetails)
        {
            Command command = new Command(Config.CommandAllowPermission.SFormat(commandName),
                args =>
                {
                    bool enabled = args.Player.GetData<bool>($"{commandName}allow");

                    args.Player.SetData($"{commandName}allow", !enabled);
                    args.Player.SendInfoMessage(enabled ? commandDetails.CommandDisabled : commandDetails.CommandEnabled);

                }, $"{commandName}allow")
            {
                HelpText = commandDetails.CommandHelpText
                

            };

            Commands.ChatCommands.Add(command);
            AllowCommand.Add(command);
        }

        /// <summary>
        /// Removes a command of the format [command]allow. Also deletes all player data associated with that command.
        /// </summary>
        /// <param name="commandName"></param>
        private void RemoveCommand(string commandName)
        {
            Command command = Commands.ChatCommands.FirstOrDefault(c => c.Name == commandName);

            if (command == null) return;

            AllowCommand.Remove(command);
            Commands.ChatCommands.Remove(command);
            TShock.Players.ForEach(p => p.RemoveData(commandName));
        }

        #endregion Helper Methods
    }
}
