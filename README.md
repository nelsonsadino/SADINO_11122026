# Tally by Nelson

Small .NET 10 API. Upload a CSV, pick a column, and it gives you back the average, min and max. It also keeps a history of every file it has processed.

## Running it

```
docker compose up --build
```

Then open http://localhost:8080/swagger. The default API key is `reviewer-key-change-me`. Set `TALLY_API_KEY` if you want a different one.

Without Docker (needs the .NET 10 SDK):

```
dotnet run --project src/Tally.Api
```

That runs on http://localhost:5080 and uses `local-dev-key` from appsettings.Development.json.

Tests: `dotnet test`

## Using it

Everything under /api needs an `X-API-Key` header. /health and /swagger don't.

- `POST /api/files?column=quantity` - upload the CSV in a form field called `file`
- `GET /api/files` - history, newest first
- `GET /api/files/summary` - totals and average processing time
- `GET /health`

Example (use `curl.exe` on Windows):

```
curl -X POST "http://localhost:8080/api/files?column=quantity" -H "X-API-Key: reviewer-key-change-me" -F "file=@samples/sales.csv"
```

```
{"fileName":"sales.csv","column":"quantity","rowCount":6,"skippedRows":1,"average":4,"min":1,"max":10}
```

CSV handling:
- needs a header row, column name isn't case sensitive
- blank or non-numeric cells are skipped and counted in skippedRows
- 400 if the column doesn't exist or has no numbers in it
- .csv only, 10 MB max

Errors come back as ProblemDetails JSON. A bad CSV is a 400, a missing or wrong key is a 401, and anything else is a 500 that gets logged without leaking details.

## File tracking

Every upload is recorded, failed ones too: file name, column, error if there was one, time and processing time. It's all in memory, so it's gone after a restart. If it needed to stick around I'd add a database version of `IFileTracker` and nothing else would have to change.

## Structure

```
src/Tally.Domain          records (FileResult, ProcessedFile, FileSummary)
src/Tally.Application     CsvCalculator, FileService, IFileTracker
src/Tally.Infrastructure  InMemoryFileTracker
src/Tally.Api             controller, API key middleware, error handler, Swagger
tests/                    unit and integration tests
```

I kept the clean architecture light. No MediatR or repositories, there's only one real endpoint.

## Notes

- The API key check is middleware because the brief asked for that. In a bigger app I'd probably move it to an auth handler.
- The app won't start if ApiKey isn't set, so it can't end up running without protection by accident.
- I used CsvHelper because quoted values like "Widget, Large" break a plain split on commas.
- HTTPS is off in the container. I'd expect a proxy in front of it to handle TLS.
- Swagger is on in every environment so it's easy to try in Docker. I'd turn that off for production.
