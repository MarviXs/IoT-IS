# Repository Guidelines

## Project Structure & Module Organization
The backend lives in `backend/src/Api`; domain-specific logic is split between `Features`, `Services`, and `BackgroundServices`, and EF Core migrations stay under `Data/Migrations`. Integration tests reside in `backend/tests/Api.IntegrationTests`. The Quasar frontend lives in `frontend/src` with route views in `pages/`, shared widgets in `components/`, and state stores in `stores/`. Device simulators and protocol helpers are grouped in `device_gateways/NuviaMSU`, while performance scripts stay in `benchmark/k6`. Local infrastructure, compose files, and the `.env.template` are collected under `docker`.

## Build, Test, and Development Commands
- `cd docker && docker compose up -d` – boots local Postgres, Redis, and MQTT services.
- `dotnet restore backend/IsApi.sln && dotnet build backend/IsApi.sln` – installs dependencies and compiles the backend solution.
- `cd backend/src/Api && dotnet ef database update --context AppDbContext` (repeat for `TimeScaleDbContext`) – applies EF Core migrations to both databases.
- `cd backend/src/Api && dotnet watch` – runs the API with hot reload.
- `cd frontend && npm install --legacy-peer-deps` – installs Vue dependencies.
- `cd frontend && npm run dev` – serves the UI locally.
- `cd frontend && npm run build` – generates the production bundle used by Docker builds.

## Coding Style & Naming Conventions
Respect `frontend/.editorconfig`: UTF-8, LF endings, and two-space indentation. Name Vue components in PascalCase (`MyWidget.vue`) and composables in camelCase. Backend C# code should keep PascalCase types, camelCase locals, and append `Async` to asynchronous methods. Run `npm run lint` for ESLint and `dotnet format backend/IsApi.sln` before opening a PR.

## Testing Guidelines
Integration coverage uses xUnit in `backend/tests/Api.IntegrationTests`; mirror feature folder names and describe expectations with `Should_DoExpectation` method names. Execute `dotnet test backend/tests/Api.IntegrationTests/Api.IntegrationTests.csproj /p:CollectCoverage=true` to run the suite with Coverlet. Frontend automated tests are not yet configured, so document manual verification steps for UI changes until component specs exist.

## Commit & Pull Request Guidelines
Current history shows short subjects; aim for `type(scope): imperative summary` (for example, `feat(devices): add bulk registration`). Group related changes per commit, reference issue IDs, and avoid committing generated artifacts. Pull requests should outline backend, frontend, and infrastructure impacts, call out new environment variables or migrations, and attach screenshots or API samples for user-facing updates.

## Security & Configuration Tips
Create environment files from `docker/.env.template` and keep secrets out of git. Review `frontend/src/config.ts` and backend `appsettings*.json` for sensitive overrides before pushing. When adding services, document required ports and credentials in `docker/` alongside any compose changes so automation remains repeatable.
