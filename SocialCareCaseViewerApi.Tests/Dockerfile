FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app


RUN apt-get update && apt-get install -y openjdk-11-jdk
RUN dotnet tool install --global dotnet-sonarscanner
RUN dotnet tool install --global dotnet-coverage
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj and restore as distinct layers
COPY ./SocialCareCaseViewerApi.sln ./
COPY ./SocialCareCaseViewerApi/SocialCareCaseViewerApi.csproj ./SocialCareCaseViewerApi/
COPY ./SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj ./SocialCareCaseViewerApi.Tests/

RUN dotnet restore ./SocialCareCaseViewerApi/SocialCareCaseViewerApi.csproj
RUN dotnet restore ./SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj

# Copy everything else, build and generate coverage file
COPY . .
COPY ./SocialCareCaseViewerApi/rds-ca-2019-root.pem /ssl/rds-ca-2019-root.pem

#build, test and SonarCloud scan
RUN chmod 777 ./SocialCareCaseViewerApi.Tests/startup.sh
CMD ./SocialCareCaseViewerApi.Tests/startup.sh
