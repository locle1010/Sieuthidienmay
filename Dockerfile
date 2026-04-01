FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WebsiteBanHang/Web_dienmay.csproj", "WebsiteBanHang/"]
RUN dotnet restore "WebsiteBanHang/Web_dienmay.csproj"

COPY . .
WORKDIR "/src/WebsiteBanHang"
RUN dotnet publish "Web_dienmay.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web_dienmay.dll"]