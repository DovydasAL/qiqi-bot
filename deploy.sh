echo 'Building'
dotnet publish QiQiBot/QiQiBot.csproj -p:PublishProfile=Linux-x64
echo 'Copying'
scp -r -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem ./QiQiBot/bin/publish/* ec2-user@3.15.144.92:/opt/qiqibot
echo 'Restarting'
ssh -i  ~/.ssh/LightsailDefaultKey-us-east-2.pem ec2-user@3.15.144.92 "sudo systemctl restart qiqibot.service"