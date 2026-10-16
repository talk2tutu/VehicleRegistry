# Design Notes

## 1. Overall architecture

A **three-tier client–server application**: a React single-page app, an ASP.NET Core REST API,
and SQL Server. The backend is a **modular monolith** — one deployable application with clear
internal separation — sized deliberately to the brief.

Inside the backend I used a **layered structure with a dependency-free core**
(a lightweight form of Clean Architecture):

| Concern | Where |
|---|---|
| Presentation / API | `web/` (React) and `Api/Controllers` (HTTP only) |
| Application logic | `Api/Services` — coordinates each operation |
| Business rules and domain | `Core/Rules`, `Core/Domain` — no EF Core, no ASP.NET |
| Data access | `Api/Data` — EF Core `AppDbContext` and migrations |

Dependencies point inward: the API references Core; Core references nothing. The critical
category logic can therefore be unit-tested in milliseconds without a database.

I deliberately stopped at two backend projects. A separate Infrastructure project, the
repository pattern (EF Core's `DbContext` already provides it), CQRS or microservices would add
files and indirection without benefit at this size. If the application grew, the first step
would be moving `Data/` into its own project, which the current dependency direction already
allows.

## 2. Database design

| Table | Key columns | Integrity rules |
|---|---|---|
| `Manufacturers` | Id, Name | Name required, unique |
| `WeightCategories` | Id, Name, **MinWeightKg** `decimal(9,2)`, IconKey | Name unique; **MinWeightKg unique**; check `MinWeightKg >= 0` |
| `Vehicles` | Id, OwnerName, ManufacturerId (FK), YearOfManufacture, WeightKg `decimal(9,2)` | FK with `ON DELETE RESTRICT`; check `WeightKg > 0` |

Decisions the brief asked about (section 17):

- **Manufacturers** are a lookup table with seed data, referenced by foreign key — not an enum
  or hard-coded strings. Adding a manufacturer is a new row (via a migration), with no code
  change. The frontend loads the list from the API.
- **Category ranges are stored as a single lower bound (`MinWeightKg`).** Each category's
  upper bound is the next category's lower bound; the highest category is open-ended.
  See section 3.
- **A vehicle's category is calculated, not stored.** `Vehicles` has no `CategoryId`. The category
  is resolved from the weight whenever vehicles are read, so it can never become inconsistent
  with the configuration (brief section 6).
- **Icons** are stored as a short key (`bike`, `car`, `van`, `truck`, `bus`, `tractor`) from a
  fixed list validated by the API. The frontend maps each key to an image in one component
  (`CategoryIcon.tsx`), so icons can be redesigned without a database change.
- **Weights** use `decimal(9,2)`, not floating point, so values such as 500.00 compare exactly
  at the boundaries and two decimal places are enforced by the database itself.
- **Defence in depth:** the database enforces what it can express cheaply (precision, positive
  weight, non-negative and unique starting weights, foreign keys). Rules about the whole set of
  categories are enforced in the application.

The schema is created by **EF Core migrations** committed under `Api/Data/Migrations`,
including seed data.

## 3. Category calculation and boundary rules

### Boundary rule (brief section 5.3)

> **A category includes its lower bound and excludes its upper bound:**
> `MinWeightKg ≤ weight < next category's MinWeightKg`.

| Category | Starts at | Effective range |
|---|---|---|
| Light | 0 kg | 0 kg to **under** 500 kg |
| Medium | 500 kg | 500 kg to **under** 2,500 kg |
| Heavy | 2,500 kg | 2,500 kg and above |

So **500.00 kg is Medium** and **2,500.00 kg is Heavy**. Because weights have exactly two
decimal places, 499.99 kg is the heaviest Light vehicle.

### Calculation

`CategoryResolver.Resolve` picks the category with the **highest `MinWeightKg` that is still ≤ the
vehicle's weight**. It is a pure function with no I/O and is covered by boundary-value tests.

### Why gaps and overlaps are impossible by design

Because only start points are stored, ranges are contiguous by construction. The only invalid
configurations left are:

1. **No category starting at 0 kg** → a gap at the bottom (brief 5.1).
2. **Two categories with the same starting weight** → an overlap (brief 5.2).

`CategoryRules.Validate` checks exactly one category starts at 0, starting weights are unique,
and names, icons and precision are valid.

### How category changes are applied safely

Every create, edit and delete:

1. loads **all** categories inside a **serializable transaction**,
2. applies the change in memory,
3. validates the **whole proposed set** with `CategoryRules`,
4. saves only if valid; otherwise returns 400 with every message and nothing is saved.

The serializable transaction prevents a race where two administrators each pass validation
against the old state and together create an overlap.

**Deleting** a category is safe by design: the category below automatically takes over its range.
Deleting the 0 kg category is rejected because it would leave a gap. The UI disables that button
as a hint; the API is the authority.

## 4. Validation and error handling

| Layer | Purpose |
|---|---|
| Browser (HTML attributes) | Instant feedback only — never relied on |
| API / Core rules | Authoritative; returns 400 `ValidationProblemDetails` listing every problem |
| Database constraints | Safety net if the API were bypassed |

- Missing records return **404**.
- Unexpected errors (for example, the database being unavailable) are handled by the
  ASP.NET Core exception handler and returned as a generic **ProblemDetails** response —
  no stack traces or internal details reach the user.
- Invalid user input is returned as a value (`OperationResult`), not thrown, because it is an
  expected outcome; exceptions are reserved for genuinely unexpected failures.

## 5. Testing strategy

- **Unit tests (29)** cover the business rules using **boundary value analysis**: just below,
  exactly on and just above each boundary; the brief's 2000 kg / 2200 kg example; gap, overlap,
  empty, missing-icon and unknown-icon configurations; and vehicle validation (including
  year 1885 rejected / 1886 accepted).
- **Integration tests (9)** use `WebApplicationFactory` to run the real API in memory against
  a dedicated SQL Server database rebuilt from the migrations each run. They cover the
  end-to-end behaviours: boundary categorisation, validation returning 400 with nothing saved,
  sorting, **re-categorisation of existing vehicles when a category changes**, overlap and gap
  rejection, deleting a middle category, and icon validation.
- Each integration test resets its own data, so tests are independent. Trivial code
  (DTOs, simple mapping) is not tested directly.

## 6. Important assumptions

1. **"Light: up to 500 kg" excludes 500 kg**, so that the brief's "Medium: 500 kg to 2500 kg"
   can include it and every weight maps to exactly one category.
2. **Year of manufacture** must be between 1886 (the first petrol car) and next year
   (models are often released early).
3. **Weight** must be greater than 0, at most 2 decimal places, and at most 9,999,999.99 kg
   (the `decimal(9,2)` limit).
4. Category **names are unique** (ignoring case and surrounding spaces).
5. **Manufacturers are managed through migrations**, not through the UI, because the brief
   describes them as predefined and rarely changing.
6. The brief requires adding and viewing vehicles; **editing and deleting vehicles** were not
   required and are not implemented.
7. Vehicle categories are shown by name and icon; **weight is also shown** in the list because
   it explains the category  and is a sort column.

## 7. Security considerations

- **Server-side validation** for every input; the browser is never trusted.
- **SQL injection** is prevented by EF Core's parameterised queries; no raw SQL is built from
  user input.
- **No secrets in source control:** the connection string lives in user secrets or an environment
  variable.
- **Error responses** use ProblemDetails without stack traces.
- **Dependencies** are checked with `dotnet list package --vulnerable --include-transitive` and
  `npm audit`. Two vulnerable transitive packages were pinned to patched versions:
  `Microsoft.OpenApi` 2.7.5 (GHSA-v5pm-xwqc-g5wc) and `System.Security.Cryptography.Pkcs` 10.0.13
  (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7).
- **Deliberately omitted for this exercise** (would be required in production):
  authentication and authorisation (especially for category administration), HTTPS
  certificate configuration, rate limiting, audit logging of category changes, and a
  least-privilege database login instead of `sa`.

## 8. Known limitations

- Migrations are applied automatically on startup **in Development only**. In production they
  would be a controlled deployment step, because several instances starting at once could
  try to migrate concurrently.
- The vehicle list is not paged; it loads all vehicles.
- Vehicles cannot be edited or deleted; manufacturers cannot be managed from the UI.
- No authentication — anyone with access can change categories.
- The frontend has no automated component tests (business behaviour is covered by the
  backend tests).
- Emoji are used as category icons; rendering varies slightly between operating systems.

## 9. Scaling and future improvements

**More users or data:**
- Add **paging and server-side filtering** to the vehicle list, with an index on each sortable column.
- Because categories are few and change rarely, the category resolution is already cheap
  (categories are loaded once per request). At very large scale, categories could be **cached**
  with invalidation on change, or the category could be computed in SQL with a range join.
- Run the API as multiple stateless instances behind a load balancer; the serializable
  transaction still protects category consistency.

**With more development time:**
- Authentication and role-based access for category administration.
- Edit and delete for vehicles, and manufacturer administration.
- An **audit history** of category changes, so it is clear why a vehicle's category changed.
- Frontend component tests (Vitest + Testing Library) and end-to-end tests (Playwright).
- A **CI pipeline** (for example GitHub Actions) running build, lint and all tests, with a SQL Server
  service container.
- A `docker-compose.yml` to start SQL Server, the API and the frontend with one command.