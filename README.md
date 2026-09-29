# VirginActiveAssignment

**Integration Technical Assessment: Integration Service API (Rock Commitment Tracker)**

ASP.NET Core Web API built with .NET 10.

---

## Requirements

- .NET 10 SDK
- Visual Studio 2026 or VS Code

---

## How to build and run the service locally

1. Clone the repository.
2. Open the solution (`MemberCommitment.API/MemberCommitment.API.slnx`), or go to the solution folder in a terminal:
   ```bash
   cd MemberCommitment.API
   ```
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Run the API:
   ```bash
   dotnet run --project VirginActiveAssignment
   ```
5. Open Swagger in your browser: https://localhost:7281/swagger/index.html

### Run tests

From the `MemberCommitment.API` folder:
```bash
dotnet test
```

### API documentation

OpenAPI definition: https://localhost:7281/openapi/v1.json

### NuGet packages

**API (`VirginActiveAssignment`)**

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.OpenApi` | 10.0.12 | Generates the OpenAPI definition |
| `Swashbuckle.AspNetCore` | 10.2.3 | Swagger UI, including the API key Authorize button |
| `Microsoft.Extensions.Http.Resilience` | 10.10.0 | Retry policy with exponential backoff and jitter for the external profile service (built on Polly) |
| `Microsoft.AspNetCore.Diagnostics` | 2.3.13 | Global exception handling (`IExceptionHandler`) |

**Tests (`MemberCommitment.API.Tests`)**

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.3 | Test framework |
| `xunit.runner.visualstudio` | 3.1.4 | Runs xUnit tests from `dotnet test` and Visual Studio |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | .NET test platform |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.12 | Hosts the API in memory for integration tests (`WebApplicationFactory`) |
| `coverlet.collector` | 6.0.4 | Code coverage collection |

---

## How to test the endpoints

### Testing with Swagger

1. Open Swagger using the URL above.
2. Click **Authorize**.
3. Enter the API key in the `X-Api-Key` field (you can find this in the `appsettings.json` file).
4. Click **Authorize** and close the dialog.

### Endpoints to test

Base URL: `https://localhost:7281/api`

| # | Method | Endpoint | Description |
|---|--------|----------|-------------|
| 1 | `GET` | `/members/{memberId}/rocks` | Get a member's rocks, optionally filtered by status |
| 2 | `POST` | `/members/{memberId}/rocks` | Create a rock for a member |
| 3 | `PATCH` | `/members/{memberId}/rocks/{rockId}` | Update a rock's status |
| 4 | `GET` | `/members/{memberId}/profile/enriched` | Get the member's profile together with their rocks |

---

#### 1. GET `/members/{memberId}/rocks`

**Without status param**

```bash
curl -X 'GET' \
  'https://localhost:7281/api/members/1/rocks' \
  -H 'accept: text/plain' \
  -H 'X-Api-Key: 40aa15f6-2dcf-4fff-8aec-8ee63a4a9d3f'
```

Response:
```json
[
  {
    "rockId": "72654231-2726-4105-ab4a-43001c0bedcf",
    "memberId": 1,
    "title": "test",
    "category": "Revenue",
    "dueDate": "2026-09-30T19:52:13.842+00:00",
    "status": "Completed",
    "note": ""
  }
]
```

**With status param** (`Pending`, `Completed` or `Missed`)

```bash
curl -X 'GET' \
  'https://localhost:7281/api/members/1/rocks?rockStatus=Completed' \
  -H 'accept: text/plain' \
  -H 'X-Api-Key: 40aa15f6-2dcf-4fff-8aec-8ee63a4a9d3f'
```

Response:
```json
[
  {
    "rockId": "72654231-2726-4105-ab4a-43001c0bedcf",
    "memberId": 1,
    "title": "test",
    "category": "Revenue",
    "dueDate": "2026-09-30T19:52:13.842+00:00",
    "status": "Completed",
    "note": ""
  }
]
```

---

#### 2. POST `/members/{memberId}/rocks`

Creates a rock for the member.

- Category values: `Revenue`, `Health`, `Career`, `Other`

```bash
curl -X 'POST' \
  'https://localhost:7281/api/members/1/rocks' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -H 'X-Api-Key: 40aa15f6-2dcf-4fff-8aec-8ee63a4a9d3f' \
  -d '{
    "memberId": 1,
    "title": "Complete a first aid course",
    "category": "Career",
    "dueDate": "2026-12-15T00:00:00+00:00",
    "note": "Required for promotion to team lead"
  }'
```

**Test data** (member id: `1`)

Positive:
```json
{
  "memberId": 1,
  "title": "test",
  "category": "Revenue",
  "dueDate": "2027-01-29T19:52:13.842Z",
  "note": ""
}
```

Negative (invalid due date):
```json
{
  "memberId": 1,
  "title": "test",
  "category": "Revenue",
  "dueDate": "2026-09-29T19:52:13.842Z",
  "note": ""
}
```

Response:
```json
{
  "rockId": "72654231-2726-4105-ab4a-43001c0bedcf",
  "memberId": 1,
  "title": "test",
  "category": "Revenue",
  "dueDate": "2026-09-30T19:52:13.842+00:00",
  "status": "Pending",
  "note": ""
}
```

---

#### 3. PATCH `/members/{memberId}/rocks/{rockId}`

Updates a rock's status.

- Only `Pending` rocks can move to `Completed` or `Missed`; any other transition returns `422`.
- Use the `rockId` returned by the POST request.

```bash
curl -X 'PATCH' \
  'https://localhost:7281/api/members/1/rocks/{rockId}?rockStatus=Completed' \
  -H 'accept: */*' \
  -H 'X-Api-Key: 40aa15f6-2dcf-4fff-8aec-8ee63a4a9d3f'
```

**Test data**

Positive:
```
https://localhost:7281/api/members/1/rocks/72654231-2726-4105-ab4a-43001c0bedcf?rockStatus=Completed
```

Negative (invalid transition):
```
https://localhost:7281/api/members/1/rocks/72654231-2726-4105-ab4a-43001c0bedcf?rockStatus=Pending
```

Response:
```json
{
  "title": "Invalid State Transition",
  "status": 422,
  "detail": "Cannot transition from Completed to Pending",
  "instance": "/api/members/1/rocks/72654231-2726-4105-ab4a-43001c0bedcf"
}
```

---

#### 4. GET `/members/{memberId}/profile/enriched`

- Returns the member's profile from the external service together with their rocks.
- If the external service is unavailable, the rocks are still returned with `enrichmentDataAvailable` set to `false`.

```bash
curl -X 'GET' \
  'https://localhost:7281/api/members/1/profile/enriched' \
  -H 'accept: text/plain' \
  -H 'X-Api-Key: 40aa15f6-2dcf-4fff-8aec-8ee63a4a9d3f'
```

Response:
```json
{
  "member": {
    "id": 1,
    "name": "Leanne Graham",
    "username": "Bret",
    "email": "Sincere@april.biz",
    "address": {
      "street": "Kulas Light",
      "suite": "Apt. 556",
      "city": "Gwenborough",
      "zipcode": "92998-3874",
      "geo": {
        "lat": "-37.3159",
        "lng": "81.1496"
      }
    },
    "phone": "1-770-736-8031 x56442",
    "website": "hildegard.org",
    "company": {
      "name": "Romaguera-Crona",
      "catchPhrase": "Multi-layered client-server neural-net",
      "bs": "harness real-time e-markets"
    }
  },
  "rocks": [
    {
      "rockId": "72654231-2726-4105-ab4a-43001c0bedcf",
      "memberId": 1,
      "title": "test",
      "category": "Revenue",
      "dueDate": "2026-09-30T19:52:13.842+00:00",
      "status": "Completed",
      "note": ""
    }
  ],
  "enrichmentDataAvailable": true
}
```

---

## The design decisions

1. **CQRS design pattern (light)**: separation of concerns.
   - Each area will have a Command file and a Query file.
2. **Strategy pattern**: initially wanted to use a Factory pattern, but changed to strategy.
   - Each category has its own validator behind a shared `IValidation` interface: `RevenueValidator`, `HealthValidator`, `CareerValidator` and `OtherValidator`, which holds the base rules.
   - (I kept the factory classes as this was my initial approach.)
3. **IMemoryCache**: used for creating, updating and querying for Rocks.

---

## Category Validation Strategies

**Strategy pattern**: initially wanted to use a Factory pattern, but changed to strategy.

- Each category has its own validator behind a shared `IValidation` interface: `RevenueValidator`, `HealthValidator`, `CareerValidator` and `OtherValidator`, which holds the base rules.
- It is easy to add new categories with their own validation, as well as add extra validation to the current logic.

---

## Cloud Deployment Architecture

### 1. Which Azure compute service would you use (Azure App Service, Azure Functions, Azure Container Apps, or another) and why? Consider the nature of this workload when making your choice.

- **Azure App Service**
- This runs web apps and APIs on managed servers. It gives auto scaling and managed identity.

### 2. How would you expose the API securely to external clients? Describe the role of Azure API Management and Azure Front Door services in your architecture and when each applies.

- This is exposed via an API gateway.
- **Azure Front Door**: protects the network and gets traffic there fast. This is for public-facing traffic, users in many regions, or failover between regions.
- **Azure API Management**: controls who uses the API and how much. This is for external clients who need their own keys, throttling or documentation.

### 3. How would secrets such as the API key and any connection strings be managed at rest and at runtime in Azure?

- The API key or any other connection strings can be stored in Azure Key Vault.

### 4. How would you provision the Azure infrastructure and deploy the application? Describe the tooling you would use and what a production-ready pipeline would look like.

Though I have not created infrastructure in Azure previously, I would opt to use **Terraform** for the following reasons:

1. **Security**: this can be part of a PR, so no manual changes.
2. **Consistency**: all required infrastructure is defined in templates that can be used across environments, which helps ensure that all environments are aligned.
3. **Access control**: permissions and access control can also be defined here.

This can be deployed via **Azure DevOps**.

**Production pipeline:**

1. **Build & Test**: restore, build, run unit tests, publish the artifact. Runs on every pull request.
2. **Plan**: `terraform validate` and `terraform plan`, with the plan saved for review.
3. **Approve**: a manual gate before production.
4. **Apply**: `terraform apply` runs the reviewed plan.
5. **Deploy**: the artifact is released to the App Service staging slot.
6. **Smoke test**: checks key endpoints on the staging slot.
7. **Swap**: staging is swapped into production with zero downtime and instant rollback.

---

## What you would add or improve given more time

### Improve

- Move business logic out of controllers into a service layer. The controllers currently handle caching, validation and state changes themselves.
- Check that `memberId` in the route matches `memberId` in the request body.
- Return validation errors as a structured list per field.
- Broaden the integration tests.
- Fix the remaining nullable reference warnings.

### Trade-offs

- The in-memory store loses data on restart and doesn't work across multiple instances.
- There's one shared API key.
- The code checks the member exists before validating the input.
