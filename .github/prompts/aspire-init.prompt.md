# Aspire .NET Project Initialization — Étape par étape

## 1. Confirmer le nom du projet
Assurez-vous d’avoir le nom de votre solution/projet (ex : `MicrocksAspireDemo`).

## 2. Vérifier la présence d’un fichier .sln
Dans le terminal :
```zsh
ls *.sln
```
- Si un fichier `.sln` existe, continuez.
- Sinon, créez d’abord la solution avec :
  ```zsh
  dotnet new sln --name [project]
  ```

## 3. Installer les templates Aspire
```zsh
dotnet new install Aspire.ProjectTemplates
```

## 4. Générer les projets Aspire
```zsh
dotnet new aspire-apphost --name [project].AppHost -o aspire/[project].AppHost
dotnet new aspire-servicedefaults --name [project].ServiceDefaults -o aspire/[project].ServiceDefaults
```

## 5. Ajouter les projets Aspire à la solution
```zsh
dotnet sln [project].sln add aspire/[project].AppHost/[project].AppHost.csproj
dotnet sln [project].sln add aspire/[project].ServiceDefaults/[project].ServiceDefaults.csproj
```

Remplacez `[project]` par le nom de votre solution/projet (ex : `MicrocksAspireDemo`).

---
**Documentation :**
- https://learn.microsoft.com/en-us/dotnet/aspire/