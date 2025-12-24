# Hacker News Challenge

A full-stack Hacker News viewer built with **ASP.NET (.NET 10)** and **Angular 20**.

The application fetches and caches the newest Hacker News stories, supports paging and search, and provides a keyboard-driven terminal-style UI.

---

## Overview

* Displays newest Hacker News stories
* Server-side paging and search
* Client-side debounced search input
* Keyboard navigation
  *    `\` focuses search input
  *    `left` and `right` arrow keys traverse pages
  *    `up` and `down` arrow keys traverse stories
  *    `escape` clears input and blurs focused stories
* Search term highlighting
* In-memory caching on the server
* Full unit and integration test coverage
* Single deployable artifact (API serves Angular)

---

## Running Locally (Development)

### Prerequisites

* Node.js 20+
* npm
* .NET SDK 10.x
* Angular CLI 20.x

---

### Start the API

```bash
cd server/HackerNewsChallenge.Api
dotnet run
```

API runs at:

```
http://localhost:5287
```

### Start the Angular client

```bash
cd client
npm install
npm start
```

Client runs at:

```
http://localhost:4200
```

API requests are proxied via `proxy.conf.json`.

---

## Running Locally (Production-Style)

This mirrors how the app is deployed to Azure.

### Build Angular into the API

```bash
cd client
npm ci
npm run build:server
```

This outputs Angular files to:

```
server/HackerNewsChallenge.Api/wwwroot
```

### Run the API

```bash
cd server/HackerNewsChallenge.Api
dotnet run
```

Open:

```
http://localhost:5287
```

Angular is served directly by ASP.NET.

---

## Running Tests

### Client tests

```bash
cd client
npm test
```

---

### All server tests

```bash
dotnet test HackerNewsChallenge.sln
```

### Server unit tests only

```bash
dotnet test server/HackerNewsChallenge.Api.UnitTests
```

### Server integration tests only

```bash
dotnet test server/HackerNewsChallenge.Api.IntegrationTests
```

---

## Deployment

The application is deployed to **Azure App Service** using **GitHub Actions**.

The deployment pipeline:

1. Install client dependencies
2. Build Angular into API `wwwroot`
3. Run client tests
4. Run server tests
5. Publish the API
6. Deploy to Azure

The workflow is defined in `.github/workflows`.

