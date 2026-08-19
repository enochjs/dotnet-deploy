#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"

echo "Applying pending migrations..."
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj

echo "Seeding development data..."
dotnet run --project scripts/SeedDevData/SeedDevData.csproj -- "$root"
