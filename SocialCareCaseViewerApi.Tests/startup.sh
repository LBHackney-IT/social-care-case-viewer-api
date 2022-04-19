#!/bin/bash
dotnet restore ./SocialCareCaseViewerApi/SocialCareCaseViewerApi.csproj
dotnet restore ./SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj

dotnet sonarscanner begin /k:"LBHackney-IT_social-care-case-viewer-api" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}" /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml

dotnet build -c debug -o out SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj --no-incremental
dotnet-coverage collect 'dotnet test' -f xml -o 'coverage.xml'

dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"