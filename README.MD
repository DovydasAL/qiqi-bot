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


