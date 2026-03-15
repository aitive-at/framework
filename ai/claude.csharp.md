## Coding Conventions

* Target NET10.0
* Add a global.json file with the SDK version pinned.
* Use Central Package Management with a Directory.Packages.props file in the root directory.
* Use Nerdbank.GitVersioning for deriving assembly version information from the git history. Main a
  version.json file in the root directory.
* Maintain .editorconfig file that matches your coding conventions.
* Maintain a .gitignore file that excludes common .net/chsharp build artifacts.
* Make use of common source generators.
* Use the modern .slnx Solution file format.
* Refactor common build and project properties into a Directory.Build.props file in the root directory. Add
  more such files for sub areas of the project if needed. Keep the actual csproj files small, e.g properties
  for nullability or targer framework can be inherited from the Directory.Build.props file
* Use nullable reference types, and mark methods with nullability attributes/annotations as needed
* Never ever use single line if/for/while loops where the statement to execute is just indented
* Perfer C# 14 Extension mebmers to older c# extension methods
* Design Async APIs using Task or ValueTask where something can potentially trigger Async IO or where
  complex threading rendezvous behaviour is needed
* Prefer the new Lock class for simple mutex style locking, instead of locking on an object
* Make use of high profile liberally licensed open source libraries from Nuget. Always research
  if they are still maintained
* Maintain common abstractions for similar usage patterns
* Use TUnit for testing, NSubstitute for mocking, Shouldly for assertions and AutoFixture for values
* Use Benchmark.DotNet if you need micro benchmarks.
* Prefer immutable data structures and data types
* Try to write in a functional style as far as it makes sense
* Keep methods small and succinct
* Keep classes small as well
* Favor composition over inheritance except when it really makes sense to use inheritance
* Strongly typed apis as far as possible

## Naming conventions

* Assemblies/Projects should be named [Company].[Project].[Area].[Assembly], following the microsoft 
  standard. The Current company name is "Aitive" 


## Directory Layout

All directories below are relative to the root directory of the project

specs/ Contains feature specification as markdown files
plans/ Contains implementation plans and status as markdown files
src/ Contains the source code, projects are placed in 
  [Area]/[Company].[Project].[Area].[Assembly] subdirectories
tests/ Contains test projects that mimic the same directory layout as src. Test Projects are named like the   project they test with the .Tests suffix
assets/ static assets e.g images
docs/ documentation for the project and in a subfolder developers for developers

## Build

* After a fresh checkout dotnet build in the root directory should build everything ready to run. No additional
  complicated build steps.
* Maintain a github action to build and test the project

## Architecture

* Define areas of the project that can be layered on top. Use Microsoft conventions for that
* Seperate the areas both in the source code as well as in the directory layout
* Keep domain specific and reusable code between projects seperate e.g in a Framework Area.
* Clean code, minimal dependencies and layered architecture are always important. 
* Create abstractions for implementations that will have more than one implementation, if there will
  ever be only one, then don't create an abstraction
* Code should always check errors and handle them, write reliable code that handles errors and performs
  retries if feasible ( e.g for a http request ) 
* Add descriptive logging everwhere it makes sense, and use https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/source-generation logging source generation to make it performance and avoid overhead when not output.