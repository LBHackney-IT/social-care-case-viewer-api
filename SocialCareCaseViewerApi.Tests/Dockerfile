FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./SocialCareCaseViewerApi.sln ./
COPY ./SocialCareCaseViewerApi/SocialCareCaseViewerApi.csproj ./SocialCareCaseViewerApi/
COPY ./SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj ./SocialCareCaseViewerApi.Tests/

RUN dotnet restore ./SocialCareCaseViewerApi/SocialCareCaseViewerApi.csproj
RUN dotnet restore ./SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj

# Copy everything else and build
COPY . .
COPY ./SocialCareCaseViewerApi/rds-ca-2019-root.pem /ssl/rds-ca-2019-root.pem
RUN dotnet build -c debug -o out SocialCareCaseViewerApi.Tests/SocialCareCaseViewerApi.Tests.csproj

CMD dotnet test