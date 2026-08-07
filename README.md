---
title: "Item Sharing App readme"
parent: StarterApp
author: Richard West
---

# Sharing App README

This document aims to lay out how to get started with the Item Sharing App.


This version of the app uses PostgreSQL for data storage and Entity Framework Core for object-relational mapping
and migrations.

To fully understand how it works, you should follow an appropriate set of tutorials such as 
[this one](https://edinburgh-napier.github.io/SET09102/tutorials/csharp/) which covers all of the main
concepts and techniques used here. However, if you want to jump straight in and work out any problems
as you go along, that will also work. The code uses structured comments for use with the 

You can use any development environment with this project including

* [Rider](https://www.jetbrains.com/rider/)
* [Visual Studio](https://visualstudio.microsoft.com/)
* [Visual Studio Code](https://code.visualstudio.com/)

This was built using VSCode on a Windows 11 machine.

The instructions assume you will be using VSCode since that is a lowest-common-denominator choice.

## Compatibility

This app is built using the following tool versions.

| Name                                                                                      | Version     |
|-------------------------------------------------------------------------------------------|-------------|
| [.NET](https://dotnet.microsoft.com/en-us/)                                               | 10.0.0    |
| [PostgreSQL Docker image](https://hub.docker.com/_/postgres)                              | 16          |


## Getting started

### Prerequisites

Before using this app, ensure you have:

1. **.NET SDK 8.0** or later installed
2. **Docker** installed and running
3. **PostgreSQL container** running (see [dev-environment tutorial](https://edinburgh-napier.github.io/SET09102/tutorials/csharp/dev-environment/))

### Configuration

1. Copy `StarterApp.Database/appsettings.json.template` to `StarterApp.Database/appsettings.json`
2. Update the connection string with your PostgreSQL credentials:
   ```json
   {
   "ConnectionStrings": {
      "DevelopmentConnection": "Host=AppDB;Port=5432;Database=appdb;Username=app_user;Password=app_password"
      "}
   } 
   ```

### Initial Setup

1. Open project in VSCode. When prompted, open in Dev container. If no dev container installed, please follow this guide to get the environment prepared properly: https://edinburgh-napier.github.io/SET09102/tutorials/csharp/dev-environment/

2. Navigate to the Migrations project and create the initial migration:
   ```bash
   cd StarterApp.Migrations
   dotnet ef migrations add InitialCreate
   ```

3. Apply the migration to create the database:
   ```bash
   dotnet ef database update
   ```

4. Build and run the application:
   ```bash
   cd ../StarterApp
   dotnet build
   dotnet run
   ```


### Tutorial

For a comprehensive guide on using this app and understanding its architecture, see the
[MAUI + MVVM + Database Tutorial](https://edinburgh-napier.github.io/SET09102/tutorials/csharp/maui-mvvm-database/).
