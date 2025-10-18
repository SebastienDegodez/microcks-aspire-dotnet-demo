---
applyTo: '**'
description: Require Developer Certificate of Origin (DCO) Signed-off-by in commits and provide guidance to configure and use commit signing.
---

# Commit Signing / DCO

## Purpose
Ensure all commits include a Signed-off-by trailer to comply with the Developer Certificate of Origin (DCO) checks, improve auditability, and make contribution history explicit.

## Scope
Applies to all contributors and PRs affecting this repository. The instruction targets contributors creating commits locally and CI checks that verify DCO compliance.

## Rules
- Every commit must include a `Signed-off-by: Your Name <you@example.com>` trailer. Use `git commit -s` to automatically append this trailer.
- Commit messages must follow the repository's Conventional Commits format (e.g., `type(scope): short description`).
- For automated tooling or bots that create commits, ensure the committer is configured to add the trailer.

## How to sign commits (DCO)
1. Stage your changes:

```bash
git add <files>
```

2. Create a commit with a Signed-off-by trailer (DCO):

```bash
git commit -s -m "type(scope): short description"
```

This will append a `Signed-off-by: Your Name <you@example.com>` line using your configured Git user name and email.

## Verifying signed commits
- Verify the DCO trailer is present:

```bash
git show --pretty=fuller --show-signature HEAD
```

For GPG-specific signature verification, use `git show --show-signature` if you also use GPG in your workflow.

## Examples
- Good:

```
git commit -s -m "fix(api): correct null handling in handler"
```

- Good (GPG + DCO):

```
git commit -S -s -m "feat(tests): add new integration test for microcks"
```
