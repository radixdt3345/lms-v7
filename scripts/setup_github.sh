#!/usr/bin/env bash
# setup_github.sh — Create all labels and milestones for lms-v7
# Run once after repo creation: bash scripts/setup_github.sh
# Requires: gh CLI authenticated (gh auth login)

set -euo pipefail

REPO="radixdt3345/lms-v7"

echo "Creating labels..."

# Wave labels
gh label create "wave:1" --color "0075ca" --description "Wave 1 — Foundation" --repo "$REPO" --force
gh label create "wave:2" --color "e4e669" --description "Wave 2 — Core Workflows" --repo "$REPO" --force
gh label create "wave:3" --color "d93f0b" --description "Wave 3 — Enhancement & Reporting" --repo "$REPO" --force

# Layer labels
gh label create "layer:DB"   --color "5319e7" --description "Database schema & migrations" --repo "$REPO" --force
gh label create "layer:API"  --color "0e8a16" --description "Controllers, services, DTOs" --repo "$REPO" --force
gh label create "layer:UI"   --color "e99695" --description "React components & pages" --repo "$REPO" --force
gh label create "layer:INT"  --color "f9d0c4" --description "Integration: wire DB+API+UI" --repo "$REPO" --force
gh label create "layer:TEST" --color "bfd4f2" --description "Integration tests (IT-)" --repo "$REPO" --force
gh label create "layer:E2E"  --color "d4c5f9" --description "E2E browser tests (E2E-)" --repo "$REPO" --force

# Priority labels
gh label create "priority:must"   --color "b60205" --description "MUST have" --repo "$REPO" --force
gh label create "priority:should" --color "e4e669" --description "SHOULD have" --repo "$REPO" --force
gh label create "priority:could"  --color "0075ca" --description "COULD have" --repo "$REPO" --force

# Feature labels
for i in $(seq -w 1 15); do
  gh label create "feature:f$i" --color "c5def5" --description "Feature F-$i" --repo "$REPO" --force
done

# Agent labels
gh label create "agent:ralph-impl" --color "1d76db" --description "Assigned to ralph-impl" --repo "$REPO" --force
gh label create "agent:ralph-test" --color "0075ca" --description "Assigned to ralph-test" --repo "$REPO" --force
gh label create "agent:ralph-e2e"  --color "006b75" --description "Assigned to ralph-e2e" --repo "$REPO" --force

# Domain labels
for domain in AUTH EMP DEPT POLICY BALANCE REQUEST COMPOFF APPROVAL HOLIDAY NOTIFY REPORT AUDIT SEED; do
  gh label create "domain:${domain,,}" --color "ededed" --description "Domain: $domain" --repo "$REPO" --force
done

# Misc
gh label create "needs-human"      --color "b60205" --description "Blocked, needs human decision" --repo "$REPO" --force
gh label create "env-issue"        --color "e4e669" --description "Environment unreachable" --repo "$REPO" --force
gh label create "lsp-blocked"      --color "d93f0b" --description "LSP errors after 3 attempts" --repo "$REPO" --force
gh label create "ui-impl-missing"  --color "b60205" --description "UI issue closed without React file" --repo "$REPO" --force

echo "Labels created."
echo ""
echo "Creating milestones (one per Epic)..."

gh api repos/$REPO/milestones -f title="[EPIC-01] Authentication & Identity"        -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-02] Employee Management"               -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-03] Department Management"             -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-04] Leave Type & Policy Management"   -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-05] Leave Balance Management"          -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-06] Leave Application & Workflow"      -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-07] Comp-Off Management"               -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-08] Approval Workflow"                 -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-09] Notifications & Email"             -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-10] Public Holiday Management"         -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-11] Dashboards"                        -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-12] Reports & CSV Export"              -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-13] Audit Trail"                       -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-14] Seed Data & Initial Setup"         -f state="open" --silent
gh api repos/$REPO/milestones -f title="[EPIC-15] Background Jobs (Hangfire)"        -f state="open" --silent

echo "Milestones created."
echo ""
echo "Setup complete! Run /push-to-pms next (or assign milestones to issues via GitHub UI)."
