# Publishing to NuGet

BlastGuard uses [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) from GitHub Actions. No long-lived API keys are stored in the repo.

## One-time setup (maintainers)

### 1. Create a trusted publishing policy on nuget.org

1. Sign in at [nuget.org](https://www.nuget.org)
2. Click your username then **Trusted Publishing**
3. **Add new policy** with:

| Field | Value |
|-------|-------|
| Policy name | `blastguard` (or any label) |
| Package owner | Your nuget.org account |
| Repository owner | `kearns2000` |
| Repository | `blastguard` |
| Workflow file | `ci.yml` |
| Environment | *(leave empty)* |

The workflow file must be exactly `ci.yml`, not the full path. Build, test, and publish all live in `ci.yml`; the publish job only runs on `v*` tags.

The nuget.org username in the `NUGET_USER` secret must be the account that **created** the trusted publishing policy, not just the package owner.

Docs: [Trusted Publishing on Microsoft Learn](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)

### 2. Add a GitHub repository secret

**Settings then Secrets and variables then Actions then New repository secret**

| Name | Value |
|------|-------|
| `NUGET_USER` | Your **nuget.org username** (profile name, not your email) |

Trusted publishing still needs your NuGet username for the login step; the temporary API key comes from OIDC.

### 3. Publish the first version

1. Ensure `Version` in `src/BlastGuard.Cli/BlastGuard.Cli.csproj` is correct (e.g. `1.1.0`)
2. Commit and push to `main`
3. Create and push a version tag:

```bash
git tag v1.1.0
git push origin v1.1.0
```

4. Watch **Actions then ci** on GitHub (the publish job runs after build)
5. After validation, the package appears at https://www.nuget.org/packages/BlastGuard


## GitHub repository topics (recommended)

After creating the repository, add these topics under **Settings → General → Topics** to improve discoverability:

```
dotnet
dotnet-tool
global-tool
cli
pull-request
code-review
pr-review
blast-radius
risk-scoring
github-actions
ci
devops
ef-core
migrations
security
api
csharp
developer-tools
```

## Releasing a new version

1. Bump `<Version>` in `src/BlastGuard.Cli/BlastGuard.Cli.csproj`
2. Commit, push to `main`
3. Tag and push: `git tag v1.1.1 && git push origin v1.1.1`

Each tag triggers build then test then pack then trusted publish.

## Notes

- Tags must match `v*` (e.g. `v1.0.0`, `v1.2.3`)
- NuGet does not allow republishing the same version, so bump the version for every release
- The temporary API key from `NuGet/login@v1` expires in about an hour; push immediately after login
- Do **not** store a `NUGET_API_KEY` secret, as trusted publishing replaces it
- Package readme images must use an [allowlisted domain](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#allowed-domains-for-images) (e.g. `raw.githubusercontent.com`). Relative paths do not render on nuget.org
