FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/LeaveRequests.API/LeaveRequests.API.csproj src/LeaveRequests.API/
RUN dotnet restore src/LeaveRequests.API/LeaveRequests.API.csproj

COPY src/LeaveRequests.API/ src/LeaveRequests.API/
RUN dotnet publish src/LeaveRequests.API/LeaveRequests.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LeaveRequests.API.dll"]
