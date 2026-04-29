# Contributing to SplashSphere

## Branching Strategy

SplashSphere uses a **two-tier branch model**:

| Branch | Role | Lifetime |
|--------|------|----------|
| `main` | Production. Mirrors what is deployed. | Permanent |
| `staging` | Active development. All features land here first. | Permanent |
| `feature/*` | Short-lived branches for individual features or fixes. | Until merged |

### Flow

```
feature/* ──(squash or rebase merge)──► staging ──(merge --no-ff)──► main
```

### Critical rule: never squash-merge `staging` → `main`

A **squash merge** of `staging` into `main` collapses every staging commit into one new commit on main, severing the commit graph. Git then sees every original staging commit as "unmerged" the next time you merge, producing massive false conflicts even though the content is identical on both sides.

This bit us once already — a 418-file/45k-line squash on main caused a 14-conflict merge during the 2026-04-29 release. Don't repeat it.

**Rule:** `staging → main` must always use `--no-ff` (a true merge commit that preserves the staging history).

```bash
# Correct: preserves the commit graph link
git checkout main
git merge --no-ff staging
git push origin main
```

```bash
# WRONG: severs the link, guarantees pain on the next release
git merge --squash staging   # ❌ never do this for staging → main
```

`feature/*` → `staging` may use squash or rebase merge — feature branches are short-lived and history compaction there is fine.

### Sync `staging` back from `main` after every release

Right after merging staging into main, fast-forward staging from main so the two never drift:

```bash
git checkout staging
git merge main          # should be a clean fast-forward
git push origin staging
```

This guarantees the next release cycle starts from a clean base.

### Branch protection on `main` (GitHub)

Configure under **Settings → Branches → Branch protection rules** for `main`:

- ✅ Require a pull request before merging
- ✅ Require status checks to pass (CI build + tests)
- ✅ Restrict who can push to matching branches (admins only)
- ✅ **Allow merge commits** — and **disable "Squash and merge"** for this branch
  - This is the GitHub UI safeguard for the rule above. Without it, anyone can accidentally squash-merge a staging PR.

### Release cadence

Pick a regular release cadence (e.g., weekly on Fridays) and ship `staging → main` on that cycle. Long-lived divergence between `staging` and `main` amplifies merge risk regardless of strategy.

---

## Commit Message Conventions

Use Conventional Commits-style prefixes:

- `feat(<scope>):` — new user-facing feature
- `fix(<scope>):` — bug fix
- `refactor(<scope>):` — restructuring without behavior change
- `perf(<scope>):` — performance improvement
- `docs(<scope>):` — documentation only
- `chore(<scope>):` — build, deps, tooling
- `merge:` — merge commits between long-lived branches

Scope is the affected area: `admin`, `pos`, `customer`, `marketing`, `api`, `domain`, `infrastructure`, `connect`, `settings`, etc.

The first line should be ≤72 chars. Use the body for details and bullet points in imperative mood.

---

## Living Documentation

After every task with code changes:

1. Append an entry to `CHANGELOG.md`.
2. Update `docs/API_ENDPOINTS.md` if you added/changed endpoints.
3. Update `docs/PAGE_INVENTORY.md` if you added/changed pages.
4. Add a business rule to `CLAUDE.md` if you implemented new domain logic.

See `CLAUDE.md` for the full architecture spec, business rules, and tech stack.
