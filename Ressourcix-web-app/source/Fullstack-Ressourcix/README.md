# Fullstack Picture Gallery Demo

## 📚 Chapters

- 🧭 [Overview](#-overview)
- 🧱 [Architecture and Structure](#-architecture-and-structure)
  - 🗂️ [Project Structure](#project-structure)
- 🧰 [Technology Stack](#-technology-stack)
  - 🖥️ [Backend and Runtime](#backend-and-runtime)
  - 🎨 [Frontend and UI](#-frontend-and-ui)
  - 🧭 [Navigation and Localization](#-navigation-and-localization)
- ✅ [Prerequisites](#-prerequisites)
- ▶️ [How to Run](#how-to-run)
  - 🧪 [With CLI](#-with-cli)
  - 🐞 [With C# Dev Kit (Debugging)](#-with-c-dev-kit-debugging)
- 🧩 [Workspace Extension Recommendations](#-workspace-extension-recommendations)
  - 🎨 [Styling and UI](#-styling-and-ui)
  - ✅ [Linting and Formatting](#-linting-and-formatting)
  - 🌍 [Localization](#-localization)
  - 🔧 [.NET and General Dev Tools](#-net-and-general-dev-tools)

## 🧭 Overview

This demo is a fullstack picture gallery application with:

- ⚙️ An ASP.NET Core backend that serves static files and exposes image APIs.
- 🖼️ A Vue frontend with Vuetify UI components.
- 🌍 Localization support with vue-i18n (DE, EN, FR).
- 🔁 SPA development middleware integration for improved local developer experience with hot-reloading, proxying, and deployment.

## 🧱 Architecture and Structure

### Project Structure

- 📄 `Program.cs`: ASP.NET Core application setup and image API endpoints.
- 📁 `app/`: Vue frontend application.
- 🌐 `app/src/i18n/`: Translation files (`de.json`, `en.json`, `fr.json`).
- ⚙️ `appsettings.json`: SPA middleware and development server settings.

## 🧰 Technology Stack

### Backend and Runtime

#### ASP.NET Core

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white)

Backend web framework used for minimal APIs, static files, and HTTP pipeline configuration.

- Project/GitHub: [https://github.com/dotnet/aspnetcore](https://github.com/dotnet/aspnetcore)
- Documentation: [https://learn.microsoft.com/aspnet/core](https://learn.microsoft.com/aspnet/core)

#### Node.js and NPM

![Node.js](https://img.shields.io/badge/Node.js-5FA04E?logo=node.js&logoColor=white)
![npm](https://img.shields.io/badge/npm-CB3837?logo=npm&logoColor=white)

Used to install frontend dependencies and run/build the Vue application.

- Node.js: [https://nodejs.org](https://nodejs.org)
- NPM: [https://www.npmjs.com](https://www.npmjs.com)

#### Smap Middleware (SPA Middleware)

![NuGet](https://img.shields.io/badge/NuGet-004880?logo=nuget&logoColor=white)

This project uses `Mumrich.SpaDevMiddleware` to connect backend and frontend development workflows.

- NuGet package: [https://www.nuget.org/packages/Mumrich.SpaDevMiddleware](https://www.nuget.org/packages/Mumrich.SpaDevMiddleware)

### 🎨 Frontend and UI

#### Vue.js

![Vue.js](https://img.shields.io/badge/Vue.js-4FC08D?logo=vuedotjs&logoColor=white)

Frontend framework for building the application UI.

- Project: [https://vuejs.org](https://vuejs.org)
- GitHub: [https://github.com/vuejs/core](https://github.com/vuejs/core)

#### Vuetify

![Vuetify](https://img.shields.io/badge/Vuetify-1867C0?logo=vuetify&logoColor=white)

Material Design UI component framework for Vue.

- Project: [https://vuetifyjs.com](https://vuetifyjs.com)
- GitHub: [https://github.com/vuetifyjs/vuetify](https://github.com/vuetifyjs/vuetify)

### 🧭 Navigation and Localization

#### Vue-i18n

![Vue i18n](https://img.shields.io/badge/Vue--i18n-41B883?logo=vuedotjs&logoColor=white)

Internationalization library used for multi-language translations.

- Project: [https://vue-i18n.intlify.dev](https://vue-i18n.intlify.dev)
- GitHub: [https://github.com/intlify/vue-i18n](https://github.com/intlify/vue-i18n)

#### Vue Router

![Vue Router](https://img.shields.io/badge/Vue%20Router-4FC08D?logo=vuedotjs&logoColor=white)

Client-side routing for page navigation (Home, Gallery, About).

- Project: [https://router.vuejs.org](https://router.vuejs.org)
- GitHub: [https://github.com/vuejs/router](https://github.com/vuejs/router)

## ✅ Prerequisites

Install the following tools before running the project:

- ![.NET](https://img.shields.io/badge/.NET%20SDK-512BD4?logo=dotnet&logoColor=white) .NET SDK compatible with `net10.0`
  - Download: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
- ![Node.js](https://img.shields.io/badge/Node.js-5FA04E?logo=node.js&logoColor=white) Node.js (recommended: current LTS)
  - Download: [https://nodejs.org](https://nodejs.org)
- ![npm](https://img.shields.io/badge/npm-CB3837?logo=npm&logoColor=white) npm (bundled with Node.js)
  - Docs: [https://docs.npmjs.com/downloading-and-installing-node-js-and-npm](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)

Useful verification commands:

```bash
dotnet --version
node --version
npm --version
```

## How to Run

### 🧪 With CLI

1. Open a terminal in the frontend folder and install dependencies:

```bash
cd app
npm install
```

1. Start the backend from the project root:

```bash
cd ..
dotnet run
```

1. Open the app in your browser (based on `launchSettings.json`):

- [http://localhost:5167](http://localhost:5167)

Official docs:

- `dotnet run`: [https://learn.microsoft.com/dotnet/core/tools/dotnet-run](https://learn.microsoft.com/dotnet/core/tools/dotnet-run)

### 🐞 With C# Dev Kit (Debugging)

1. Open this folder in VS Code:

- `source/FullstackRessourcix`

1. Ensure the extension is installed:

- C# Dev Kit: [https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

1. Open the Run and Debug view and choose a .NET launch profile (`http` or `https`).
1. Press `F5` to start debugging.
1. Set breakpoints in `Program.cs` or related backend files and trigger requests from the browser.

Official docs:

- C# in VS Code: [https://code.visualstudio.com/docs/csharp/get-started](https://code.visualstudio.com/docs/csharp/get-started)
- C# debugging in VS Code: [https://code.visualstudio.com/docs/csharp/debugging](https://code.visualstudio.com/docs/csharp/debugging)

## 🧩 Workspace Extension Recommendations

The workspace recommends the following VS Code extensions:

### 🎨 Styling and UI

#### UnoCSS

![UnoCSS](https://img.shields.io/badge/UnoCSS-333333?logo=css&logoColor=white)

UnoCSS IntelliSense and tooling support.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=antfu.unocss](https://marketplace.visualstudio.com/items?itemName=antfu.unocss)

#### Vue - Official

![Vue - Official](https://img.shields.io/badge/Vue%20Official-4FC08D?logo=vuedotjs&logoColor=white)

Vue language server support and type-checking for Single File Components.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=Vue.volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar)

#### Vuetify VS Code Extension

![Vuetify](https://img.shields.io/badge/Vuetify-1867C0?logo=vuetify&logoColor=white)

Vuetify snippets and tooling support.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=vuetifyjs.vuetify-vscode](https://marketplace.visualstudio.com/items?itemName=vuetifyjs.vuetify-vscode)

### ✅ Linting and Formatting

#### CSharpier - Code Formatter

![CSharpier](https://img.shields.io/badge/CSharpier-239120?logo=csharp&logoColor=white)

CSharpier formatting integration for C#.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=csharpier.csharpier-vscode](https://marketplace.visualstudio.com/items?itemName=csharpier.csharpier-vscode)

#### ESLint

![ESLint](https://img.shields.io/badge/ESLint-4B32C3?logo=eslint&logoColor=white)

ESLint diagnostics and fixes for JavaScript/TypeScript/Vue.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint)

#### Prettier - Code Formatter

![Prettier](https://img.shields.io/badge/Prettier-F7B93E?logo=prettier&logoColor=1A2B34)

Prettier code formatter integration.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode)

#### Roslynator

![Roslynator](https://img.shields.io/badge/Roslynator-512BD4?logo=dotnet&logoColor=white)

Roslyn analyzers and refactorings for C#.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=josefpihrt-vscode.roslynator](https://marketplace.visualstudio.com/items?itemName=josefpihrt-vscode.roslynator)

### 🌍 Localization

#### i18n Ally

![i18n Ally](https://img.shields.io/badge/i18n%20Ally-2F80ED?logo=visualstudiocode&logoColor=white)

Translation key management and localization editing.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=lokalise.i18n-ally](https://marketplace.visualstudio.com/items?itemName=lokalise.i18n-ally)

### 🔧 .NET and General Dev Tools

#### Git Graph

![Git Graph](https://img.shields.io/badge/Git%20Graph-F05032?logo=git&logoColor=white)

Visual Git commit graph and history explorer.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=mhutchie.git-graph](https://marketplace.visualstudio.com/items?itemName=mhutchie.git-graph)

#### C# Dev Kit

![C# Dev Kit](https://img.shields.io/badge/C%23%20Dev%20Kit-239120?logo=csharp&logoColor=white)

.NET and C# development tooling for VS Code.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

#### Version Lens

![Version Lens](https://img.shields.io/badge/Version%20Lens-0A84FF?logo=visualstudiocode&logoColor=white)

Inline package version and update insights.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=pflannery.vscode-versionlens](https://marketplace.visualstudio.com/items?itemName=pflannery.vscode-versionlens)

#### XML

![XML](https://img.shields.io/badge/XML-E44D26?logo=xml&logoColor=white)

XML language support, validation, and formatting.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml)

#### YAML

![YAML](https://img.shields.io/badge/YAML-CB171E?logo=yaml&logoColor=white)

YAML language support, validation, and schema integration.

- Marketplace: [https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml)
