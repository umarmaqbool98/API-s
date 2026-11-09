# Student Marks Management API

ASP.NET Core 8 REST API using EF Core, PostgreSQL, JWT authentication, DTO validation, and Swagger.

## Requirements

- .NET 8 SDK
- PostgreSQL 16+ or Docker Desktop
- Postman (for the required collection)

## Start PostgreSQL

From this directory, Docker Compose can start PostgreSQL:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Wait until the API starts. It creates the database tables automatically.

- Swagger: <http://localhost:8080/swagger>
- Health check: <http://localhost:8080/health>

The sample `.env` values are for local development only. Do not use them in production.
To stop the app, press **Ctrl+C**. Run `docker compose down` to stop and remove the
containers; the database is kept in a Docker volume.

## Use Swagger

1. In `POST /api/auth/login`, click **Try it out**, enter
   `{"username":"teacher1","password":"Teacher123!"}`, then click **Execute**.
2. Copy the `token` from the response. Click **Authorize**, paste the token, and
   confirm.
3. Use `POST /api/Students` to add a student. The response includes the student's ID.
4. Use `GET /api/Students` to list students, or `GET /api/Students/{id}` to fetch one.

## Use Postman

Import `postman/MarksApi.postman_collection.json`. Send **Auth → Teacher login**,
then **Students → Create student**. Send **Get student (after Create student)** to
fetch the new record.

For submission, save screenshots of actual Postman requests and Collection Runner
results in `docs/screenshots/`.
