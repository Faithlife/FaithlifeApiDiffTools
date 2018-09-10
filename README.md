# Faithlife.ApiDiffTools

Tools for working with and manipulating APIs in .NET assemblies.

## FacadeGenerator

Generate a facade of the public interface of an assembly, stripping away implementation and private types and members.

[![NuGet](https://img.shields.io/nuget/v/Faithlife.FacadeGenerator.Tool.svg)](https://www.nuget.org/packages/Faithlife.FacadeGenerator.Tool)

To install:

```
dotnet tool install Faithlife.FacadeGenerator.Tool --global
```

## ApiDiffTool

Find differences in the public API between two versions of an assembly.

[![NuGet](https://img.shields.io/nuget/v/Faithlife.ApiDiffTool.Tool.svg)](https://www.nuget.org/packages/Faithlife.ApiDiffTool.Tool)

To install:

```
dotnet tool install Faithlife.ApiDiffTool.Tool --global
```

## PackageDiffTool

Find differences in the public API between two versions of a NuGet package and suggest an [Semantic Versioning](https://semver.org) version.

[![NuGet](https://img.shields.io/nuget/v/Faithlife.PackageDiffTool.Tool.svg)](https://www.nuget.org/packages/Faithlife.PackageDiffTool.Tool)

To install:

```
dotnet tool install Faithlife.PackageDiffTool.Tool --global
```

## Build Status

Ubuntu | Windows | NuGet
--- | --- | ---
[![Travis CI](https://img.shields.io/travis/Faithlife/FaithlifeApiDiffTools/master.svg)](https://travis-ci.org/Faithlife/FaithlifeApiDiffTools) | [![AppVeyor](https://img.shields.io/appveyor/ci/Faithlife/faithlifeapidifftools/master.svg)](https://ci.appveyor.com/project/Faithlife/faithlifeapidifftools) | [![NuGet](https://img.shields.io/nuget/v/Faithlife.ApiDiffTools.svg)](https://www.nuget.org/packages/Faithlife.ApiDiffTools)

## Documentation

* https://faithlife.github.io/FaithlifeApiDiffTools/
* License: [MIT](LICENSE)
* [Version History](VersionHistory.md)
* [Contributing Guidelines](CONTRIBUTING.md)
