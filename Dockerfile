FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Lum.MQ.Rabbit.WorkerSample/Lum.MQ.Rabbit.WorkerSample.csproj", "Lum.MQ.Rabbit.WorkerSample/"]
RUN dotnet restore "Lum.MQ.Rabbit.WorkerSample/Lum.MQ.Rabbit.WorkerSample.csproj"
COPY . .
WORKDIR "/src/Lum.MQ.Rabbit.WorkerSample"
RUN dotnet build "Lum.MQ.Rabbit.WorkerSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lum.MQ.Rabbit.WorkerSample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lum.MQ.Rabbit.WorkerSample.dll"]