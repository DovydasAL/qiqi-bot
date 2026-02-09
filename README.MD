# QiQi Bot

## Initial Deployment (EC2 with systemd)
1. `sudo dnf install dotnet-runtime-9.0`
2. Create `/etc/qiqibot.env` with contents:
    ```
    DiscordBotToken=<token>
    DBConnectionString=<connection-string>
    ```
3. `sudo mkdir -p /opt/qiqibot` and `sudo chown ec2-user:ec2-user /opt/qiqibot`
4. Publish the solution and copy the contents to the above directory
    - `scp -r -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem * ec2-user@3.15.144.92:/opt/qiqibot`
5. Copy the service file to `/etc/systemd/system/qiqibot.service`
6. `sudo systemctl daemon-reload`
7. `sudo systemctl start qiqibot.service`
    - To stop use `sudo systemctl stop qiqibot.service`

## Updating
1. Publish through Visual Studio or CLI and run the following from the publish directory
    - `scp -r -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem * ec2-user@3.15.144.92:/opt/qiqibot`
2. `sudo systemctl restart qiqibot.service`

## Logs
`sudo journalctl -u qiqibot.service -f`


