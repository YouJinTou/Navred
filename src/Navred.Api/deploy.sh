  !/usr/bin/env bash

STAGE=${1:-qa}

 dotnet tool install -g Amazon.Lambda.Tools

 dotnet tool update -g Amazon.Lambda.Tools

 cd "../Navred.Api.Tests"

 dotnet test

 cd "../Navred.Api"

dotnet lambda deploy-serverless $STAGE --s3-bucket $STAGE'buckets3' \
	--profile 'navred' \
	--region 'eu-central-1' \
	--configuration 'Release' \
	--framework 'netcoreapp2.1' \
	--s3-prefix 'Navred.Api/' \
	--template 'deploy-template.json' \
	--template-parameters "stage=$STAGE"