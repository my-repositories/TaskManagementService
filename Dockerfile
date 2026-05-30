FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/TaskManagementService.Domain/*.csproj ./src/TaskManagementService.Domain/
COPY src/TaskManagementService.Dal/*.csproj ./src/TaskManagementService.Dal/
COPY src/TaskManagementService.Api/*.csproj ./src/TaskManagementService.Api/
COPY src/TaskManagementService.Listener.Http.Api/*.csproj ./src/TaskManagementService.Listener.Http.Api/
COPY src/TaskManagementService.Listener.Rabbit.Api/*.csproj ./src/TaskManagementService.Listener.Rabbit.Api/
COPY tests/TaskManagementService.Tests/*.csproj ./tests/TaskManagementService.Tests/
RUN dotnet restore

COPY . .
ARG PROJ_PATH
RUN dotnet publish ${PROJ_PATH} -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
