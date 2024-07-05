FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Recruitment task/Recruitment task.csproj", "Recruitment task/"]
RUN dotnet restore "Recruitment task/Recruitment task.csproj"

COPY . .
WORKDIR "/src/Recruitment task"
RUN dotnet build "Recruitment task.csproj" -c Release -o /app/build

RUN dotnet publish "Recruitment task.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

EXPOSE 5000
EXPOSE 6969

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Recruitment task.dll"]
