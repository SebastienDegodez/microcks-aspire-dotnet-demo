# Aspire .NET Project Initialization — Step by Step

## 1. Confirm the Project Name
Ensure you have chosen your solution/project name (e.g., `MicrocksAspireDemo`).

## 2. Check for a .sln File
In the terminal:
```zsh
ls *.sln
```
- If a `.sln` file exists, continue.
- If not, create the solution:
  ```zsh
  dotnet new sln --name [project]
  ```

## 3. Install Aspire Templates
```zsh
dotnet new install Aspire.ProjectTemplates
```

## 4. Generate Aspire Projects
```zsh
dotnet new aspire-apphost --name [project].AppHost -o aspire/[project].AppHost

dotnet new aspire-servicedefaults --name [project].ServiceDefaults -o aspire/[project].ServiceDefaults
```

## 5. Add Aspire Projects to the Solution
```zsh
dotnet sln [project].sln add aspire/[project].AppHost/[project].AppHost.csproj
dotnet sln [project].sln add aspire/[project].ServiceDefaults/[project].ServiceDefaults.csproj
```

Replace `[project]` with your solution/project name (e.g., `MicrocksAspireDemo`).

---
**Documentation:**
- https://learn.microsoft.com/en-us/dotnet/aspire/