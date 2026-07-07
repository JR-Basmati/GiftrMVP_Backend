# Giftr — Backend API
> **Status:** Early MVP — under active development. Core auth and data-access
> plumbing are in place; feature endpoints are being built out against the
> planned data model below.

## Overview

Giftr is a web app for keeping track of gift-giving: who you know, their
interests, the gifts you're planning or have given, and the events those gifts
are tied to (birthdays, holidays, and so on). This repository is the backend only.

The project is a personal build with two goals: to ship something I'd actually
use, and to translate backend patterns I know well from Node.js into the
ASP.NET Core ecosystem.

## Tech Stack

| Layer          | Technology                                              |
| -------------- | ------------------------------------------------------- |
| Framework      | ASP.NET Core (Minimal APIs)                             |
| Language       | C#                                                      |
| Data access    | Entity Framework Core                                   |
| Database       | PostgreSQL                                              |
| Authentication | ASP.NET Core Identity + JWT bearer tokens               |
| Frontend       | React (separate repository)                             |

## Current Features

- **Authentication** — user registration and login via ASP.NET Core Identity,
  issuing JWT bearer tokens for the React single-page app.
- **Data access** — Entity Framework Core over PostgreSQL, with migrations
  managing the schema.
- **CORS** — configured to allow the React frontend origin.
- **Minimal API endpoints** — an initial set covering auth (register, login)
  and recipient management (e.g. create recipient), with more being added.

## Planned Structure

The full data model is designed ahead of implementation. The planned schema
covers:

- **Recipients** — the people you're tracking.
- **Interests / likes** — attached to recipients to inform gift ideas.
- **Gifts** — planned and given, with status and location tracking (via enums
  for where a gift is sourced or stored).
- **Events** — occasions a gift is due for, linked to recipients.

**Full ERD:**
<img width="1368" height="944" alt="image" src="https://github.com/user-attachments/assets/7d59da7f-6d5c-4ec6-99da-86fa2046e746" />



## Getting Started

> Prerequisites: .NET SDK and a PostgreSQL instance.

```bash
# Restore dependencies
dotnet restore

# Set your PostgreSQL connection string
# (via appsettings.Development.json or user secrets)

# Apply the database schema
dotnet ef database update

# Run the API
dotnet run
```

## Related Repositories

- **Frontend** — React client that consumes this API is still listed as "Private" for now.

## Roadmap

- [ ] Flesh out recipient, gift, and event endpoints against the full data model
- [ ] -- **Stop and review our MVP - any directional changes needed?** --
- [ ] Wire up the events → gifts → recipients relationships
- [ ] Explore sharing lists between multiple users (E.g. Partners sharing the folks the family members to buy gifts for)
