# Recruitment Task Project

## Project Overview
The Recruitment Task project is an ASP.NET Core 6.0 application designed to fetch, store, and provide market asset data through a REST API and WebSocket service. It includes functionality for obtaining access tokens, fetching asset data, updating bars, and providing real-time data updates via WebSocket.

## Prerequisites
- .NET 6 SDK
- SQL Server for the database
- Postman or curl for testing API endpoints and WebSocket connections

## Configuration
Add the following configuration settings in the `appsettings.json` file:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-BLK1FJ4\\MSSQLSERVER2022;Database=MarketAssetsDB;Trusted_Connection=True;"
  },
  "Settings": {
    "accessTokenUrl": "https://platform.fintacharts.com/identity/realms/fintatech/protocol/openid-connect/token",
    "url": "https://platform.fintacharts.com/",
    "assetsUrl": "api/instruments/v1/instruments?page=",
    "barsUrl": "api/bars/v1/bars/count-back?provider=oanda&interval=1&periodicity=day&barsCount=1000",
    "username": "r_test@fintatech.com",
    "password": "kisfiz-vUnvy9-sopnyv",
    "ws": "wss://platform.fintacharts.com/api/streaming/ws/v1/realtime"
  }
}
```

## API Endpoints
#### Get All Assets:

```http
  GET /api/MarketAssets
```

| Method | Response                |
| :-------- | :------------------------- |
| `GET` | Fetches and stores all market assets |

Example:
"https://localhost:6969/api/MarketAssets"

#### Get Market Assets:
```http
  GET /api/MarketAssets/assets
```

| Method | Response                |
| :-------- | :------------------------- |
| `GET` | Returns a list of market assets |

Example: https://localhost:6969/api/MarketAssets/assets

#### Get Asset Price:

```http 
  GET /api/MarketAssets/price${idAssets}
```

| Method | Parameter | Description | Response               | 
| :----- |:----------|:----------- |:---------------------- | 
| `GET` | idAssets |**Required** List of asset IDs |Returns a list of market assets |

Example: https://localhost:6969/api/MarketAssets/price?idAssets=ebefe2c7-5ac9-43bb-a8b7-4a97bf2c2576

## WebSocket Service
The WebSocket server provides real-time updates for asset prices. It is available at ws://0.0.0.0:8080/ws

Connecting to the WebSocket Server:

Use a WebSocket client to connect to the server.
Example connection URL: ws://localhost:8080/ws
Subscribing to Asset Updates:

Follow the link with the asset ID to subscribe to updates for that asset. 
```
ws://localhost:8080/ws?id=ad9e5345-4c3b-41fc-9437-1d253f62db52
```
Receiving Updates:
The server will send real-time updates for the subscribed asset.
