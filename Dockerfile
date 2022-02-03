FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Lumin.MQ.Rabbit.WorkerSample/Lumin.MQ.Rabbit.WorkerSample.csproj", "Lumin.MQ.Rabbit.WorkerSample/"]
RUN dotnet restore "Lumin.MQ.Rabbit.WorkerSample/Lumin.MQ.Rabbit.WorkerSample.csproj"
COPY . .
WORKDIR "/src/Lumin.MQ.Rabbit.WorkerSample"
RUN dotnet build "Lumin.MQ.Rabbit.WorkerSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lumin.MQ.Rabbit.WorkerSample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lumin.MQ.Rabbit.WorkerSample.dll"]