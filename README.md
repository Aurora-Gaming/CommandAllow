# CommandAllow
A TShock plugin for Terraria servers. Copies the functionality of `/tpallow` and `/wallow` to other user-defined functions

## Usage
Example based to the `/slap <player> <amount>` command: 
- `/commandallow add slap 0` adds a new command to the server: `/slapallow`
- `/commandallow remove slapallow` removes the newly created command

## Permissions
Admin commands permission: `commandallow.admin`

where `cmd` is the name of the command, e.g: `slap`
Function | Permission | 
--- | --- | 
Use `/cmdallow` | `commandallow.cmd` | 
Override a User's `/cmdallow` | `commandallow.cmd.override` | 
Override all instances of the command | `comandallow.overrideall` |

## Configuration
The formatting of the above permissions except `commandallow.admin` are customisable via the generated config file in `tshock/commandallow.json`

Each command generated with `/commandallow` will be populated with default values for enable, disable, warn and help - These values are configurable and changes come into effect after a `/reload`.
