 #!/usr/bin/env bash

STAGE=${1:-qa}
CONFIG_FILE=aws-defaults.$STAGE.json

dotnet tool install -g Amazon.Lambda.Tools

dotnet tool update -g Amazon.Lambda.Tools

cd "../Navred.Api.Tests"

dotnet test

cd "../Navred.Api"

dotnet lambda deploy-serverless --config-file $CONFIG_FILE
