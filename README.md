# CluedIn.Crawling.DropBox

CluedIn crawler for DropBox.

[![Build Status](https://dev.azure.com/CluedIn-io/CluedIn%20Crawlers/_apis/build/status/CluedIn-io.CluedIn.Crawling.DropBox?branchName=master)](https://dev.azure.com/CluedIn-io/CluedIn%20Crawlers/_build/latest?definitionId=42&branchName=master)

------

## Overview

This repository contains the code and associated tests for the [DropBox](https://www.dropbox.com/developers) crawler.

## Usage

### NuGet Packages

To use the `DropBox` crawler and provider with the `CluedIn` server you will have to add the following NuGet packages to the `Providers.csproj` project file:

```PowerShell
Install-Package CluedIn.Crawling.DropBox

Install-Package CluedIn.Provider.DropBox
```

The NuGet packages specified are available on the [internal development feed](https://dev.azure.com/CluedIn-io/CluedIn%20Crawlers/_packaging?_a=feed&feed=develop).

### Debugging

To debug the `DropBox` Provider/Crawler:

- Clone the [CluedIn.Crawling.DropBox](https://github.com/CluedIn-io/CluedIn.Crawling.DropBox) repository
- Open `Crawling.DropBox.sln` in Visual Studio
- Rebuild All
- Copy DLL and PDB files from `\**\bin\debug\net452` to the servers `ServerComponents` folder
- Run CluedIn backend server using `.\build.ps1 run`
- In Visual Studio with the `DropBox` crawler solution open, use `Debug -> Attach to Process` on `CluedIn.Server.ConsoleHostv2.exe`
- In the UI, add a new configuration for the `DropBox` provider and invoke `Re-Crawl`

## Working with the Code

Load [Crawling.DropBox.sln](.\Crawling.DropBox.sln) in Visual Studio or your preferred development IDE.

### Running Tests

A mocked environment is required to run `integration` and `acceptance` tests. The mocked environment can be built and run using the following [Docker](https://www.docker.com/) command:

```Shell
docker-compose up --build -d
```

Use the following commands to run all `Unit` and `Integration` tests within the repository:

```Shell
dotnet test .\Crawling.DropBox.sln --filter Unit
dotnet test .\Crawling.DropBox.sln --filter Integration
```

To run [Pester](https://github.com/pester/Pester) `acceptance` tests

```PowerShell
invoke-pester
```

To review the [WireMock](http://wiremock.org/) HTTP proxy logs

```Shell
docker-compose logs wiremock
```

## References

- [DropBox API](https://www.dropbox.com/developers/documentation/http/documentation)

### Tooling

- [Docker](https://www.docker.com/)
- [Pester](https://github.com/pester/Pester)
- [WireMock](http://wiremock.org/)
