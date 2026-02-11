echo 'Building'
dotnet publish QiQiBot/QiQiBot.csproj -p:PublishProfile=Linux64
echo 'Stopping'
ssh -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem ec2-user@3.15.144.92 "sudo systemctl stop qiqibot.service"
echo 'Copying'
scp -r -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem ./QiQiBot/bin/publish/* ec2-user@3.15.144.92:/opt/qiqibot
echo 'Reloading Daemon'
ssh -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem ec2-user@3.15.144.92 "sudo systemctl daemon-reload"
echo 'Restarting'
ssh -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem ec2-user@3.15.144.92 "sudo systemctl start qiqibot.service"