# Vidly

A small video rental store built with ASP.NET Core MVC. Staff can manage customers (with membership types and an 18+ rule for paid memberships) and the movie catalogue.

![CI](https://github.com/Milindpatel/Vidly-ASP.NET/actions/workflows/ci.yml/badge.svg)

## Features

- Browse and search customers and movies.
- Create, edit and delete customers and movies (requires an account).
- Membership types: Pay as You Go, Monthly, Quarterly, Annual. Paid memberships require the customer to be 18 or older.
- Register / log in with ASP.NET Core Identity.
- Client- and server-side validation, anti-forgery protection on every form.

## Tech stack

.NET 10, ASP.NET Core MVC + Razor, ASP.NET Core Identity, EF Core 10 with SQLite, Bootstrap 5, xUnit v3.

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project Vidly       # https://localhost:7080
dotnet test Vidly.slnx           # run the tests
```

The SQLite database (`vidly.db`) is created on first run and seeded with membership types, genres and a few movies. Register an account to add or edit records.

## Project layout

```
Vidly/
  Controllers/     Home, Customers, Movies
  Data/            EF Core DbContext and seed data
  Models/          Customer, Movie, Genre, MembershipType, validation
  ViewModels/      Form view models
  Views/           Razor views (Identity pages use the shared layout)
Vidly.Tests/       Validation unit tests and HTTP integration tests
```

## History

Originally built on ASP.NET MVC 5 / .NET Framework with Entity Framework 6 and SQL Server LocalDB; ported to ASP.NET Core in 2026.
