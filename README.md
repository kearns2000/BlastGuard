![BlastGuard](https://raw.githubusercontent.com/kearns2000/blastguard/main/docs/blastguard-logo.png)

# BlastGuard

[![NuGet](https://img.shields.io/nuget/v/BlastGuard?style=flat&logo=nuget)](https://www.nuget.org/packages/BlastGuard)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Build](https://github.com/kearns2000/blastguard/actions/workflows/ci.yml/badge.svg)](https://github.com/kearns2000/blastguard/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/kearns2000/blastguard)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-xUnit-5C2D91?style=flat&logo=xunit)](tests/BlastGuard.Core.Tests)

**Target framework:** `net10.0` · **Type:** .NET global tool · **Test runner:** xUnit

**Topics:** pull request review · blast radius · code review · risk scoring · GitHub Actions · EF Core migrations · API contracts · security · .NET CLI

**BlastGuard gives reviewers a warning label before they start reviewing a pull request.**

BlastGuard is a lightweight .NET CLI for scoring pull request risk based on changed files, contracts, configuration, migrations, security-sensitive areas, runtime behaviour, and test signals. It analyses the files changed in a branch or pull request, applies simple deterministic rules, and produces a risk score that helps reviewers understand how far a change could spread if it is wrong.

BlastGuard does not prove a pull request is safe or unsafe. It highlights signals that usually deserve more careful review.

 

Pull requests can look small while still touching public contracts, database migrations, production configuration, or security-sensitive code. BlastGuard gives reviewers a practical warning label before they start reviewing, so they know where to spend extra attention.

## Installation

Install BlastGuard as a .NET global tool:

```bash
dotnet tool install --global BlastGuard
```

See [PUBLISHING.md](PUBLISHING.md) for how releases are published via NuGet trusted publishing.

## Basic usage

From a git repository:

```bash
blastguard analyse --base main --head HEAD
```

Compare against a remote base branch:

```bash
blastguard analyse --base origin/main --head HEAD
```

Write JSON output to a file:

```bash
blastguard analyse --format json --output blastguard-report.json
```

Write a Markdown report:

```bash
blastguard analyse --format markdown --output blastguard-report.md
```

Fail a CI step when the score is high:

```bash
blastguard analyse --format github --fail-threshold 90
```

Use a custom configuration file:

```bash
blastguard analyse --config blastguard.json
```

The American spelling `analyze` is also supported as a command alias.

## Example output

```text
BlastGuard report

Score: 76 / 100
Risk: High

Main risk signals:
- Public contract file changed
- EF Core migration changed
- Production appsettings changed
- Authentication code touched
- No matching test changes found

Suggested review focus:
- Check whether API consumers, message consumers, or generated clients are affected.
- Confirm the migration is backwards compatible.
- Confirm the new configuration exists in every required environment.
- Review authentication, authorisation, claims, and permission behaviour carefully.
- Add or update tests around the changed contract.
```

## GitHub Actions usage

### Using the BlastGuard action (recommended)

BlastGuard is published on the [GitHub Marketplace](https://github.com/marketplace/actions/blastguard-pr-blast-radius). The action installs .NET, installs the tool, runs it, and adds the report to the job summary.

To add it from the Marketplace:

1. Open the [BlastGuard PR Blast Radius](https://github.com/marketplace/actions/blastguard-pr-blast-radius) listing.
2. Click **Use latest version** and copy the snippet, or use the workflow below.
3. Commit the workflow to `.github/workflows/blastguard.yml` in your repository.

```yaml
name: BlastGuard

on:
  pull_request:
    branches:
      - main

jobs:
  blastguard:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - uses: kearns2000/BlastGuard@v1
        with:
          base: origin/main
          head: HEAD
```

Pin to a specific release (for example `kearns2000/BlastGuard@v1.0.0`) if you prefer reproducible runs over automatic minor updates.

#### Inputs

| Input | Description | Default |
| --- | --- | --- |
| `base` | Base git ref to compare against. | `origin/main` |
| `head` | Head git ref to analyse. | `HEAD` |
| `repo` | Path to the git repository. | `.` |
| `format` | Output format: `text`, `json`, `markdown`, or `github`. | `github` |
| `output` | Output file path for the report. | `blastguard-report.md` |
| `config` | Optional path to a `blastguard.json` configuration file. | `''` |
| `fail-threshold` | Fail the step when the score meets or exceeds this value. Leave empty to never fail. | `''` |
| `include-suggestions` | Include suggested review focus in the output. | `true` |
| `version` | BlastGuard tool version to install. Defaults to the latest release. | `''` |
| `dotnet-version` | .NET SDK version to install. | `10.0.x` |
| `job-summary` | Append the report to the GitHub Actions job summary. | `true` |

### Running the tool directly

If you prefer to install and run the tool yourself:

```yaml
name: BlastGuard

on:
  pull_request:
    branches:
      - main

jobs:
  blastguard:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v5
        with:
          fetch-depth: 0

      - name: Install .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Install BlastGuard
        run: dotnet tool install --global BlastGuard

      - name: Run BlastGuard
        run: blastguard analyse --base origin/main --head HEAD --format github --output blastguard-report.md

      - name: Add report to job summary
        run: cat blastguard-report.md >> $GITHUB_STEP_SUMMARY
```

## Configuration

BlastGuard reads `blastguard.json` from the repository root by default. You can also pass `--config <path>`.

See `blastguard.sample.json` for a useful starting point.

Supported configuration areas:

- `thresholds`: customise Low, Medium, High, and Critical score boundaries
- `ignorePatterns`: glob patterns for generated or irrelevant files
- `boundedAreaRoots`: path roots used to infer feature or module areas
- `securityPathHints`: extra path hints for security-sensitive detection
- `rules`: enable or disable individual rules by id

Example:

```json
{
  "thresholds": {
    "medium": 25,
    "high": 50,
    "critical": 75
  },
  "ignorePatterns": [
    "**/*.Designer.cs",
    "**/*.g.cs"
  ],
  "rules": {
    "database": { "enabled": true },
    "security": { "enabled": true }
  }
}
```

## Scoring model

BlastGuard starts at 0 and adds or subtracts points based on deterministic rules. The final score is clamped between 0 and 100.

Default risk levels:

| Score | Risk level |
|------:|------------|
| 0-24 | Low |
| 25-49 | Medium |
| 50-74 | High |
| 75-100 | Critical |

Rules cover:

- bounded area spread
- public contracts and endpoints
- database and migration changes
- configuration and infrastructure files
- security-sensitive code
- runtime behaviour such as retries, queues, and background workers
- test signals
- dependency changes
- large change size
- documentation-only changes

Negative findings are supported. For example, documentation-only or test-only changes can reduce the score.

## Output formats

Supported formats:

- `text`: human-readable console output
- `json`: machine-readable output
- `markdown`: report artefact for local use or upload
- `github`: compact Markdown suitable for PR comments or job summaries

## Design principles

- Deterministic rules over opaque heuristics
- Fast analysis based only on changed files and patch text
- Minimal dependencies
- Clear output aimed at reviewers, not blame
- Easy to extend with new rules later

## Known limitations

- BlastGuard does not compile the solution or analyse Roslyn symbols
- Git integration uses the installed `git` executable
- Route and migration detection uses simple text matching
- Test area matching is intentionally basic in v1
- Binary files are handled gracefully but not deeply analysed

## Future ideas

- OpenAPI diffing
- Deeper EF Core migration analysis
- Roslyn-based public API analysis
- GitHub PR comment publishing
- Architecture-specific rules
- SARIF output

## Local development

```bash
dotnet restore
dotnet build
dotnet test
dotnet pack
```

Run locally without installing the tool:

```bash
dotnet run --project src/BlastGuard.Cli -- analyse --base main --head HEAD
```

## Licence

MIT
