# Contributing to AIKit

Thank you for your interest in contributing to AIKit! We welcome contributions from the community to help improve this .NET library for AI integration.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Building the Project](#building-the-project)
- [Running Tests](#running-tests)
- [Coding Standards](#coding-standards)
- [Submitting Changes](#submitting-changes)
- [Reporting Issues](#reporting-issues)
- [Documentation](#documentation)

## Code of Conduct

This project follows a code of conduct to ensure a welcoming environment for all contributors. By participating, you agree to:

- Be respectful and inclusive
- Focus on constructive feedback
- Accept responsibility for mistakes
- Show empathy towards other contributors
- Help create a positive community

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/your-username/AIKit.git
   cd AIKit/src
   ```
3. **Create a feature branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Git](https://git-scm.com/)

### Environment Setup

1. Ensure you have .NET 10 installed:

   ```bash
   dotnet --version
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Building the Project

AIKit uses the standard .NET build process:

```bash
# Build all projects in Release configuration
dotnet build --configuration Release

# Or build a specific project
dotnet build AIKit.Clients/AIKit.Clients.csproj --configuration Release
```

## Running Tests

The project includes comprehensive unit tests. Run them using:

```bash
# Run all tests
dotnet test

# Run tests with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Run tests for a specific project
dotnet test AIKit.Clients.Tests/AIKit.Clients.Tests.csproj
```

Test results are generated in the `TestResults` directory.

## Coding Standards

### C# Guidelines

- Follow the [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused on a single responsibility
- Use async/await for asynchronous operations

### Code Style

The project uses EditorConfig for consistent formatting. Key settings:

- Use explicit types (IDE0008 is disabled for readability)
- Follow standard .NET naming conventions
- Maintain consistent indentation and spacing

### Commit Messages

Use clear, descriptive commit messages:

```
feat: add support for new AI provider
fix: resolve memory leak in vector store
docs: update README with new examples
```

## Submitting Changes

1. **Ensure your code builds** and all tests pass
2. **Update documentation** if needed (README, XML comments)
3. **Write tests** for new functionality
4. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: description of your changes"
   ```
5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```
6. **Create a Pull Request** on GitHub:
   - Provide a clear description of the changes
   - Reference any related issues
   - Ensure CI checks pass

### Pull Request Requirements

- All CI checks must pass (build, tests, linting)
- Code must be reviewed by at least one maintainer
- Follow the existing code style and architecture patterns
- Include tests for new features or bug fixes
- Update documentation as needed

## Reporting Issues

Found a bug or have a feature request? Please:

1. Check existing [issues](https://github.com/durmisi/AIKit/issues) first
2. Create a new issue with:
   - Clear title and description
   - Steps to reproduce (for bugs)
   - Expected vs. actual behavior
   - Environment details (.NET version, OS, etc.)
   - Code samples if applicable

## Documentation

- Update README.md for significant changes
- Add XML documentation comments to public APIs
- Update package descriptions in project files
- Consider adding examples for new features

## Architecture Guidelines

When contributing new features:

- **Follow existing patterns**: Use builders for client creation, implement interfaces consistently
- **Maintain modularity**: Keep provider-specific code separate
- **Consider extensibility**: Design for easy addition of new providers
- **Performance matters**: Use async operations, consider memory usage
- **Error handling**: Provide meaningful exceptions and logging

## Testing Guidelines

- Write unit tests for all public methods
- Use mocking for external dependencies
- Test error conditions and edge cases
- Maintain high code coverage
- Integration tests should be separate from unit tests

## Release Process

Maintainers handle releases. The process includes:

- Version bumping in project files
- NuGet package publishing
- Release notes generation
- GitHub release creation

## Questions?

If you have questions about contributing:

- Check the [README](README.md) for usage examples
- Open a [discussion](https://github.com/durmisi/AIKit/discussions) on GitHub
- Contact maintainers through issues

Thank you for contributing to AIKit! 🎉

---

</content>
<parameter name="filePath">c:\Projects\AIKit\src\CONTRIBUTING.md
