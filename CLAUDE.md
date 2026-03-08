# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Aitive Framework is a modular .NET 10.0 application framework by Aitive Technologies GmbH. It provides plugin architecture, multi-tenancy, Orleans integration, and ASP.NET Core hosting — published as `Aitive.Framework.*` NuGet packages.

## Build & Test Commands

```bash
dotnet build                    # Build entire solution
dotnet test                     # Run all tests
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run single test
dotnet pack -c Release -o ./nupkgs  # Create NuGet packages
dotnet format                   # Apply editorconfig formatting
```

No special setup beyond having .NET SDK 10.0+ installed. `dotnet build` from root builds everything.

## Architecture

### Plugin System (central abstraction)

The framework's core pattern is a plugin architecture with discovery, resolution, and binding:

- **IPlugin** — exposes a manifest and queryable interface implementations
- **IPluginHost** — manages loaded plugins; created by `DefaultPluginHostBuilder`
- **PluginAttribute** — assembly-level attribute declaring plugin identity (id, version, dependencies)
- **PluginResolver** — resolves dependency graphs using `SemVersionRange` matching
- **IPluginBindPointBuilder** — strategy for converting plugins into registered services:
  - `ServicePluginBindPointBuilder` — extracts `IServiceModule` registrations
  - `ApplicationPartsPluginBindPointBuilder` — registers MVC controllers/views
  - `WebRootPluginBindPointBuilder` — merges static assets
  - `ConfigurationPluginBindPointBuilder` — extracts `[ConfigurationOptions]` types
  - `OrleansPluginBindPointBuilder` — registers Orleans `IGrain` implementations

### Application Bootstrap Lifecycle

`Application<TBuilder, THost, TSelf>` orchestrates startup:

1. `OnCreateBuilder` → `OnSetupConfiguration` → `LoggingProvider`
2. `OnCreatePluginHost` → plugin discovery via providers (e.g., `ProjectPluginProvider`)
3. `OnConfigureBuilder` → bind plugin services, register `IServiceModule` implementations
4. `OnConfigureServices` → user service registration hook
5. Build host → run `IApplicationStartupTask` (ordered via `IOrdered`) → `OnRunHost`

Configuration loads in priority: base JSON → environment JSON → env vars → CLI args.

### Multi-Tenancy

- `ITenant` / `ITenantResolver` — tenant identity and resolution
- `ITenantHttpModuleProvider` / `ITenantHttpRouter` — per-tenant HTTP pipeline routing

### Key Functional Types

- `Optional<T>` — safe nullable alternative (monadic)
- `Result<T, TError>` — railway-oriented error handling
- `QueryResult<T, TCursor, TError>` — paginated query results
- `Unit` — void replacement for functional APIs

### Module Organization

Each `Aitive.Framework.*` project is a focused module. Key ones:

| Module | Purpose |
|--------|---------|
| `Framework` | Core utilities: collections, reflection, functional types, patterns |
| `Framework.Application` | Application lifecycle, startup tasks, logging providers |
| `Framework.Plugins` | Plugin discovery, loading, resolution, binding |
| `Framework.AspNetCore` | `WebApplication<TSelf>`, `IHttpModule` route binding |
| `Framework.Orleans` | Orleans grain integration, Npgsql storage setup |
| `Framework.DependencyInjection` | `IServiceModule` registration pattern |
| `Framework.Tenancy` | Multi-tenant routing and resolution |
| `Framework.Configuration` | `[ConfigurationOptions]` attribute-driven config binding |
| `Framework.Data` | `IQueryOperation`, query result abstractions |
| `Framework.Slang` | Core language features and extensions |
| `Framework.SourceGenerators` | C# source generators |

## Coding Conventions

Detailed conventions live in `ai/claude.csharp.md`. Key points:

- **C# 14+**, file-scoped namespaces, nullable reference types always enabled
- **Prefer C# 14 extension members** over older `static class` extension methods
- **Never** single-line `if`/`for`/`while` without braces
- **Prefer `Lock`** class over `lock(object)` for mutex-style locking
- **Async**: use `Task`/`ValueTask` for APIs that may trigger async I/O
- **Immutable data structures** preferred; functional style where it makes sense
- **Logging**: use `Microsoft.Extensions.Logging` with [source generation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/source-generation)
- **Testing**: TUnit + NSubstitute + Shouldly + AutoFixture
- **Benchmarks**: BenchmarkDotNet

### Naming

- Assemblies: `Aitive.Framework.[Area].[Assembly]`
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Interfaces: `IName`; type parameters: `TName`
- Everything else follows standard .NET PascalCase (see `.editorconfig`)

## Project Configuration

- **Central package management**: all NuGet versions in `Directory.Packages.props` — never specify versions in `.csproj`
- **Shared build props**: `Directory.Build.props` — target framework, nullable, lang version inherited by all projects
- **Versioning**: Nerdbank.GitVersioning via `version.json` — versions derived from git history
- **Solution format**: `.slnx` (modern XML solution format)

## CI/CD

GitHub Actions (`.github/workflows/dotnet.yml`): builds on push/PR to `main`/`develop`, runs tests, packs and publishes NuGet packages to nuget.org.
