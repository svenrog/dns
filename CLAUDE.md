# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A DNS library in C#: message parsing/serialization plus a small DNS client and proxy server. The shipping package is the `DNS` project; `DNS.Deprecated` holds legacy code kept for reference.

## Build & test

- Multi-targeted: `DNS`, `Tests`, and `DNS.Benchmark` build for both `net9.0` and `net10.0`. Build the whole solution with `dotnet build DNS.slnx`.
- Tests use **xUnit**: `dotnet test Tests`. Run a single test with `dotnet test Tests --filter "FullyQualifiedName~TestName"`.
- Benchmarks (BenchmarkDotNet) run in Release: `dotnet run -c Release --project DNS.Benchmark`.

## Constraints (will fail the build if violated)

- `TreatWarningsAsErrors=true` on the `DNS` project — any compiler warning is a build error. Don't leave unused usings, unreachable code, etc.
- `Nullable` is enabled — respect nullable annotations; don't suppress with `!` unless genuinely safe.
- `ImplicitUsings` is enabled — common namespaces (`System`, `System.Linq`, etc.) are already imported; don't add redundant `using` directives.
- `PublishAot=true` — avoid patterns that break AOT/trimming (unbounded reflection, dynamic codegen).

## Git workflow

- Don't commit directly to `master`. Create a feature branch and open a PR for review. Branch names are unconstrained.

## Code style (non-default, from .editorconfig)

- Do **not** add braces to single-line `if`/`for` bodies (`csharp_prefer_braces = false`).
- Do **not** convert to primary constructors (`csharp_style_prefer_primary_constructors = false`).

## Layout

- `DNS/Protocol` — message, header, question, resource records, (de)serialization.
- `DNS/Client` — `DnsClient`, `ClientRequest`, and `IRequestResolver` implementations (UDP/TCP).
- `DNS/Server` — `DnsServer`, `MasterFile`.
- `Examples/` — runnable Client / Server / ClientServer samples.
