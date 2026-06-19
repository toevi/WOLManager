# Contributing to WOL Manager

Thank you for your interest in contributing!

## Reporting bugs

Open an [issue](../../issues/new) and include:

- Windows version and architecture
- .NET version (`dotnet --version`)
- Steps to reproduce
- What you expected vs. what happened

## Suggesting features

Open an issue with the `enhancement` label. Describe the use case — what problem it solves and how you imagine it working.

## Pull requests

1. Fork the repo and create a branch from `main`
2. Make your changes — keep them focused on a single concern
3. Build with `dotnet build -c Release` and verify there are no new errors
4. Open a pull request with a clear description of what changed and why

## Development setup

- Visual Studio 2022 or VS Code with C# extension
- .NET 9 SDK
- Windows 10/11 (WinForms requires Windows)

```
git clone https://github.com/toevi/WOLManager.git
cd WOLManager
dotnet build
dotnet run
```

## Code style

- C# naming conventions (PascalCase for methods/properties, camelCase for locals)
- Comments in English
- No unused imports or dead code
