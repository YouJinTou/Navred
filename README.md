# Navred

**A route-finding experiment for intercity bus travel in Bulgaria.**

Navred set out to make fragmented bus schedules searchable in one place. It collected timetable data from different operators, modelled journeys as a graph, and searched for useful itineraries between Bulgarian destinations.

## What it does

- **Collects schedules** through provider-specific web crawlers and normalizes routes, stops, and departure times.
- **Builds a transport graph** from journey legs, with departure/arrival times and available price information.
- **Finds alternative itineraries** using a Dijkstra-based shortest-path implementation with additional path-search logic.
- **Exposes an HTTP API** for destination lookup and itinerary searches.
- **Provides an Angular interface** for searching routes and viewing results.

## Technology

- **Backend:** C#, ASP.NET Core, .NET Core 2.1/3.1
- **Frontend:** Angular 8, TypeScript, Angular Material
- **Data:** Amazon DynamoDB
- **Infrastructure:** AWS Lambda and related deployment tooling
- **Algorithms:** Graph modelling, Dijkstra-based path search, timetable-aware itinerary processing

## Repository structure

```text
src/
  Navred.Core/          Route models, search algorithms, and itinerary logic
  Navred.Crawling/      Operator-specific timetable crawlers
  Navred.Api/           HTTP endpoints
  Navred.*.Tests/       Tests for core, API, and crawling components
ui/                    Angular frontend
```

## Running the code

This repository is a historical project, **not a maintained or hosted service**. The original applications target end-of-life .NET and Angular versions, and running the full system requires AWS configuration and timetable data in DynamoDB. Provider websites may also have changed since the crawlers were written.

For reference, the original development commands were:

```bash
# Frontend (from the repository root)
cd ui
npm install
npm start

# Backend (in a separate terminal, from the repository root)
cd src/Navred.Api
dotnet run
```

These commands describe the project layout; they have **not been verified on current toolchains** and do not, by themselves, set up the required backend data or infrastructure.

## Status

**Discontinued (2020).** Navred was a personal exploration of timetable aggregation, route optimization, and serverless application architecture. It is preserved as a source-code project rather than an active travel service.
