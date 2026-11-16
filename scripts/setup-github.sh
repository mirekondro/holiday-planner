#!/usr/bin/env bash
# One command to set up GitHub for holiday-planner: project board, labels, milestones and all issues.
# Generated with AI (Claude) – see README "AI disclosure".
#
# Usage (from the repo root):  bash scripts/setup-github.sh

set -euo pipefail
cd "$(dirname "$0")/.."

OWNER="mirekondro"
REPO="mirekondro/holiday-planner"
PROJECT_NUMBER=3
PROJECT_TITLE="Holiday Planner"
MATE="sasahrad"

echo "1/5 Checking gh permissions…"
if ! gh auth status 2>&1 | grep -q "'project'"; then
  echo "    gh needs the 'project' scope – a browser window will ask you to confirm."
  gh auth refresh -s project
fi

echo "2/5 Renaming project #$PROJECT_NUMBER to '$PROJECT_TITLE' and linking it to the repo…"
gh project edit "$PROJECT_NUMBER" --owner "$OWNER" --title "$PROJECT_TITLE" >/dev/null
gh project link "$PROJECT_NUMBER" --owner "$OWNER" --repo "$REPO" >/dev/null 2>&1 || true

echo "3/5 Checking that $MATE has accepted the repo invite…"
if ! gh api "repos/$REPO/collaborators/$MATE" >/dev/null 2>&1; then
  echo "    $MATE is not a collaborator yet (invite not accepted)."
  echo "    Wait until he accepts and run this script again – nothing else was changed."
  exit 1
fi

echo "4/5 Creating labels, milestones and issues…"
MATE="$MATE" PROJECT="$PROJECT_TITLE" bash scripts/create-issues.sh

echo "5/5 Applying the 16 Nov case clarifications…"
MATE="$MATE" PROJECT="$PROJECT_TITLE" bash scripts/update-issues.sh

echo
echo "All done. Board: https://github.com/users/$OWNER/projects/$PROJECT_NUMBER"
