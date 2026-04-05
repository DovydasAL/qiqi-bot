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
| Command | Arguments | Description | Default Permission |
| --- | --- | --- | --- |
| `/clan-register` | `clan_name` (string, required) – RuneScape clan name to associate with the Discord server (max 20 characters). | Registers the clan for the current guild so that other commands know which clan to manage. | Administrator |
| `/clan-activity` | None. | Generates a CSV attachment listing each clan member and the latest known activity date (based on clan experience or RuneMetrics events). | Everyone |
| `/clan-capped` | None. | Generates a CSV attachment listing members who have capped at the citadel since the most recent reset. Requires `/clan-citadel-reset` to be configured. | Everyone |
| `/clan-citadel-reset` | `day` (integer choice, required) – Day of week 0–6 for Sunday–Saturday.<br>`time` (string, required) – Game time of reset in 24-hour `HH:mm` format. | Stores the weekly reset schedule used by `/clan-capped` to determine which caps belong to the current week. | Administrator |
| `/clan-leave-join-channel` | `channel` (channel, required) – Discord channel that should receive leave/join notifications. | Persists the channel ID that `ClanEventService` uses when posting future clan leave/join events. Re-run to change the target channel. | Administrator |
| `/clan-welcome-channel` | `channel` (channel, optional) – Discord channel that should receive clan welcome events. | Persists the channel ID used for clan welcome event notifications. Omit `channel` to clear the configured channel. | Administrator |
| `/clan-achievement-channel` | `channel` (channel, required) – Discord channel that should receive achievements. | Persists the channel ID that `AchievementService` uses when posting clan member achievements. Re-run to change the target channel. | Administrator |
| `/clan-debug` | `enabled` (boolean, required) – `true` to enable, `false` to disable. | Toggles verbose debug mode for the guild, which influences what diagnostic messages are emitted. | Administrator |
| `/clan-rsn-audit` | None. | Generates a TXT report with two sections: Discord users in the guild who have not set an RSN, and users who have set an RSN that is not currently in the configured clan for this guild. | Administrator |
| `/rsn` | `name` (string, required) – RuneScape display name (1–12 characters). | Links the caller’s Discord user to a RuneScape name within this guild and updates their nickname accordingly. | Everyone |

_All commands must be executed inside a Discord server; invoking them in direct messages will be rejected._


