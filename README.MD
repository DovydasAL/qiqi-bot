# QiQi Bot

## Initial Deployment (EC2 with systemd)
1. Install .NET runtime
    - `sudo dnf install dotnet-runtime-9.0`
2. Create `/etc/qiqibot.env` with contents:
    ```
    DiscordBotToken=<token>
    DBConnectionString=<connection-string>
    ```
3. `sudo mkdir -p /opt/qiqibot && sudo chown ec2-user:ec2-user /opt/qiqibot`
4. Copy the service file to `/etc/systemd/system/qiqibot.service`
5. Pickup the new service file `sudo systemctl daemon-reload`
6. Run `./deploy.sh` to deploy the code and start it
    - Publishes solution
    - Copies with SCP to directory
    - Restarts the service

## Logs
To view the logs, run `./prod_logs.sh`

## Slash Commands
### User Commands
| Command | Arguments | Description |
| --- | --- | --- |
| `/rsn` | `name` (string, required) – RuneScape display name (1–12 characters). | Links the caller’s Discord user to a RuneScape name within this guild and updates their nickname accordingly. |

### Administrator Commands
| Command | Arguments | Description |
| --- | --- | --- |
| `/clan-register` | `clan_name` (string, required) – RuneScape clan name to associate with the Discord server (max 20 characters). | Registers the clan for the current guild so that other commands know which clan to manage. |
| `/clan-activity` | None. | Generates a CSV attachment listing each clan member and the latest known activity date (based on clan experience or RuneMetrics events). |
| `/clan-capped` | None. | Generates a CSV attachment listing members who have capped at the citadel since the most recent reset. Requires `/clan-citadel-reset` to be configured. |
| `/clan-citadel-reset` | `day` (integer choice, required) – Day of week 0–6 for Sunday–Saturday.<br>`time` (string, required) – Game time of reset in 24-hour `HH:mm` format. | Stores the weekly reset schedule used by `/clan-capped` to determine which caps belong to the current week. |
| `/clan-citadel-channel` | `channel` (channel, optional) – Discord channel that should receive citadel visit/cap notifications. | Persists the channel ID used for citadel notifications. Omit `channel` to clear the configured channel. |
| `/clan-leave-join-channel` | `channel` (channel, optional) – Discord channel that should receive leave/join notifications. | Persists the channel ID used for clan leave/join notifications. Omit `channel` to clear the configured channel. |
| `/clan-welcome-channel` | `channel` (channel, optional) – Discord channel that should receive clan welcome events. | Persists the channel ID used for clan welcome event notifications. Omit `channel` to clear the configured channel. |
| `/clan-achievement-channel` | `channel` (channel, optional) – Discord channel that should receive achievements. | Persists the channel ID used for clan member achievement notifications. Omit `channel` to clear the configured channel. |
| `/clan-debug` | `enabled` (boolean, required) – `true` to enable, `false` to disable. | Toggles verbose debug mode for the guild, which influences what diagnostic messages are emitted. |
| `/clan-rsn-audit` | None. | Generates a TXT report with two sections: Discord users in the guild who have not set an RSN, and users who have set an RSN that is not currently in the configured clan for this guild. |
| `/clan-member-set-rsn` | `user` (user, required) – Discord member whose RSN will be set.<br>`name` (string, required) – RuneScape display name (1–12 characters). | Allows administrators to set or update another member’s RSN for this guild and update their nickname. |

_All commands must be executed inside a Discord server; invoking them in direct messages will be rejected._


