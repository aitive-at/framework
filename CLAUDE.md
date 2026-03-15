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

## Directory Layout

```
src/          Source projects: Aitive.Framework.* (flat, one dir per module)
tests/        Test projects mirroring src/ layout with .Tests suffix
samples/      Sample applications demonstrating framework usage
ai/           AI coding conventions (claude.csharp.md)
specs/        Feature specifications (markdown)
plans/        Implementation plans (markdown)
```

## Architecture

### Plugin System (central abstraction)

The framework's core pattern is a plugin architecture with discovery, resolution, and binding:

- **IPlugin** — exposes a manifest and queryable interface implementations via `Query<T>()`
- **IPluginHost** — manages loaded plugins; created by `DefaultPluginHostBuilder`
- **PluginAttribute** — assembly-level attribute declaring plugin identity (id, version, dependencies)
- **PluginResolver** — resolves dependency graphs with topological sorting, cycle detection, and `SemVersionRange` matching
- **IPluginBindPointBuilder** — strategy for converting plugins into registered services:
  - `ServicePluginBindPointBuilder` — extracts `IServiceModule` registrations
  - `ApplicationPartsPluginBindPointBuilder` — registers MVC controllers/views
  - `WebRootPluginBindPointBuilder` — merges static assets
  - `ConfigurationPluginBindPointBuilder` — extracts `[ConfigurationOptions]` types
  - `OrleansPluginBindPointBuilder` — registers Orleans `IGrain` implementations

A plugin declares itself via `[assembly: Plugin("id", "version")]` in a manifest file and exposes services through the `Query<T>()` pattern. See `samples/Aitive.Framework.Samples.PluginWeb.Plugin01/` for a concrete example.

### Application Bootstrap Lifecycle

`Application<TBuilder, THost, TSelf>` orchestrates startup. `WebApplication<TSelf>` extends it for ASP.NET Core:

1. `OnCreateBuilder` → `OnSetupConfiguration` → `LoggingProvider`
2. `OnCreatePluginHost` → plugin discovery via providers (e.g., `ProjectPluginProvider`)
3. `OnConfigureBuilder` → bind plugin services, register `IServiceModule` implementations
4. `OnConfigureServices` → user service registration hook
5. Build host → register host services globally via `Globals`
6. `OnConfigureHost` → configure HTTP modules and middleware (web apps)
7. Run `IApplicationStartupTask` (ordered via `IOrdered`) → `OnRunHost`

Configuration loads in priority: base JSON → environment JSON → env vars → CLI args.

### Key Extension Points

- **IServiceModule** — single-method interface (`Register(IServiceCollection)`) for declaring DI registrations. Discovered automatically from plugins by `ServicePluginBindPointBuilder`.
- **IHttpModule** — registers routes and middleware via `Register(IServiceProvider, IWebHostEnvironment, IApplicationBuilder, IEndpointRouteBuilder)`. Modules are ordered via `IOrdered`.
- **IApplicationStartupTask** — runs at startup after host is built, before `OnRunHost`.
- **IOrdered** — ordering interface (default `Order = 0`) used throughout for service prioritization.

### Multi-Tenancy

- `ITenant` / `ITenantResolver` — tenant identity and resolution
- `ITenantHttpModuleProvider` — supplies tenant-specific `IHttpModule` instances
- `ITenantHttpRouter` / `PathPrefixTenantHttpRouter` — per-tenant HTTP pipeline routing based on URL prefix

### Key Functional Types

- `Optional<T>` — monadic nullable alternative with value/undefined/null states; supports `Select`, `OfType`, `Cast`, `Coalesce`
- `Result<T, TError>` — railway-oriented error handling with implicit operators
- `QueryResult<T, TCursor, TError>` — paginated query results with cursor support
- `Unit` — void replacement for functional APIs

### Source Generators

- **TypedIdGenerator** — generates strongly-typed ID types from `[TypedId]`-decorated record structs, including JSON converters, type converters, comparison operators, casting, and parsing
- **ApplicationDescriptionGenerator** — generates application metadata when using Application Framework with Nerdbank.GitVersioning
- Built on Scriban templating via `TemplatedSourceGenerator` base class

### Module Organization

Each `Aitive.Framework.*` project is a focused module. Key ones:

| Module | Purpose |
|--------|---------|
| `Framework` | Core utilities: collections, reflection, functional types, patterns |
| `Framework.Slang` | Core language features and extensions |
| `Framework.Application` | Application lifecycle, startup tasks, logging providers |
| `Framework.Plugins` | Plugin discovery, loading, resolution, binding |
| `Framework.DependencyInjection` | `IServiceModule` registration pattern |
| `Framework.Configuration` | `[ConfigurationOptions]` attribute-driven config binding |
| `Framework.AspNetCore` | `WebApplication<TSelf>`, `IHttpModule` route binding |
| `Framework.Tenancy` | Multi-tenant routing and resolution |
| `Framework.Data` | `IQueryOperation`, query result abstractions |
| `Framework.Orleans` | Orleans grain integration, Npgsql storage setup |
| `Framework.SourceGenerators` | C# source generators (`TypedId`, `ApplicationDescription`) |
| `Framework.Versioning` | Semantic versioning (`SemVersion`, `SemVersionRange`, range parsers) |
| `Framework.Cryptography` | Hashing abstractions (`IHashAlgorithm`, `IHashProvider`, SHA implementations) |
| `Framework.Interop` | Native library interop (`INativeLibrary`, `INativeLibraryResolver`) |
| `Framework.Json` | JSON configuration and serialization extensions |
| `Framework.Marten` | Marten document database integration |
| `Framework.EntityFrameworkCore` | EF Core integration |
| `Framework.YesSql` | YesSql document database integration |
| `Framework.Serilog` | Serilog logging provider |
| `Framework.Autofac` | Autofac DI container integration |
| `Framework.Http` | HTTP utilities (stub) |
| `Framework.Vcs` | Version control abstractions (foundational) |
| `Framework.Cli` | CLI tooling (stub) |

## Coding Conventions

Detailed conventions live in `ai/claude.csharp.md`. Key points:

- **C# 14+**, file-scoped namespaces, nullable reference types always enabled
- **Prefer C# 14 extension members** over older `static class` extension methods:
  ```csharp
  // Preferred (C# 14)
  extension<T>(IEnumerable<Optional<T>> items)
  {
      public Optional<T> FirstOrNone() { ... }
  }

  // Avoid (old style)
  public static class EnumerableExtensions
  {
      public static Optional<T> FirstOrNone<T>(this IEnumerable<Optional<T>> items) { ... }
  }
  ```
- **Never** single-line `if`/`for`/`while` without braces
- **Prefer `Lock`** class over `lock(object)` for mutex-style locking
- **Async**: use `Task`/`ValueTask` for APIs that may trigger async I/O
- **Immutable data structures** preferred; functional style where it makes sense
- **Logging**: use `Microsoft.Extensions.Logging` with [source generation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/source-generation)
- **Testing**: xunit + coverlet (current); `ai/claude.csharp.md` specifies TUnit + NSubstitute + Shouldly + AutoFixture for new code
- **Benchmarks**: BenchmarkDotNet
- **Favor composition over inheritance** except when inheritance genuinely fits

### Naming

- Assemblies: `Aitive.Framework.[Area].[Assembly]`
- Private fields: `_camelCase`; private static fields: `s_camelCase`
- Interfaces: `IName`; type parameters: `TName`
- Everything else follows standard .NET PascalCase (see `.editorconfig`)

## Project Configuration

- **Central package management**: all NuGet versions in `Directory.Packages.props` — never specify versions in `.csproj`
- **Shared build props**: `Directory.Build.props` — target framework, nullable, lang version inherited by all projects
- **Versioning**: Nerdbank.GitVersioning via `version.json` — versions derived from git history; `pathFilters` scoped to `src/`
- **Solution format**: `.slnx` (modern XML solution format)
- **SDK pinned**: `global.json` pins .NET SDK 10.0 with `rollForward: latestMinor`

## CI/CD

GitHub Actions (`.github/workflows/dotnet.yml`): builds on push/PR to `main`/`develop`, runs tests, packs and publishes NuGet packages to nuget.org.

## Samples

- `samples/Aitive.Framework.Samples.PluginWeb/` — plugin loading with MVC controllers and Razor components from plugins
- `samples/Aitive.Framework.Samples.PluginWeb.Plugin01/` — concrete plugin example with `[TypedId]`, `IServiceModule`, controllers
- `samples/Aitive.Framework.Samples.MultiTenantWeb/` — per-tenant HTTP modules with `PathPrefixTenantHttpRouter`
