# ReviewHub — Restaurant & Movie Reviews

A reviews platform where users rate restaurants and movies, write reviews, and browse community ratings.

## Stack

- **Backend:** ASP.NET Core 8 Web API, Clean Architecture (Domain / Application / Infrastructure / Api)
- **Database:** SQL Server with Entity Framework Core (code-first, auto-created and seeded on startup)
- **Auth:** JWT bearer tokens, PBKDF2 password hashing
- **Frontend:** Vanilla HTML/CSS/JS single-page app (responsive, mobile-first), deployed on Vercel
- **Backend hosting:** MonsterASP.NET

## Project structure

```
ReviewHub.Domain/             Entities (User, Place, Review)
ReviewHub.Application/        DTOs, service interfaces, business logic (Auth, Places, Reviews)
ReviewHub.Infrastructure/     EF Core DbContext, seed data, JWT + password hashing, DI
ReviewHub.Api/                Controllers, Program.cs, Swagger
client/                       Static frontend (deployed to Vercel)
```

## API overview

| Method | Endpoint | Description |
| ------ | -------- | ----------- |
| POST | `/api/auth/register` | Create an account, returns JWT |
| POST | `/api/auth/login` | Sign in, returns JWT |
| GET | `/api/places?kind=&search=` | List places with average rating and review count |
| GET | `/api/places/{id}` | Place details with all reviews |
| POST | `/api/places/{id}/reviews` | Add or update my review (auth, one per place) |
| GET | `/api/reviews/mine` | My reviews (auth) |

Interactive docs are available at `/swagger` on the running API.

## Running locally

1. Set `ConnectionStrings:DefaultConnection` in `ReviewHub.Api/appsettings.json` to a SQL Server instance.
2. `dotnet run --project ReviewHub.Api` — the database is created and seeded automatically.
3. Serve `client/` with any static server and set `API_BASE` at the top of `client/app.js` to the API URL.
