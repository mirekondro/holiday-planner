#!/usr/bin/env bash

set -euo pipefail

REPO="mirekondro/holiday-planner"
ME="mirekondro"    
MATE="sasahrad"                
PROJECT="holiday-planner"           

DRY_RUN="${DRY_RUN:-0}"
FORCE="${FORCE:-0}"

M1="M1: Setup"
M2="M2: Users & master data"
M3="M3: Core holiday engine"
M4="M4: Period end, payouts & special cases"
M5="M5: Deploy, testing & docs"

run() { if [[ "$DRY_RUN" == "1" ]]; then :; else "$@"; fi; }

# ---------------------------------------------------------------- safety check
if [[ "$DRY_RUN" != "1" && "$FORCE" != "1" ]]; then
  existing=$(gh issue list -R "$REPO" --state all --limit 1 --json number -q 'length')
  if [[ "$existing" != "0" ]]; then
    echo "Repo already has issues – aborting so nothing is duplicated. Use FORCE=1 to run anyway."
    exit 1
  fi
fi

# ---------------------------------------------------------------- labels
label() { run gh label create "$1" -R "$REPO" --color "$2" --description "$3" --force >/dev/null; }
label core         "B60205" "Holiday domain logic (ledger, accrual, rules)"
label backend      "1D76DB" ".NET Web API / Linq2db"
label frontend     "5319E7" "React client"
label database     "0E8A16" "Schema, migrations, data"
label auth         "D93F0B" "Authentication & authorization"
label testing      "FBCA04" "Unit / integration / E2E tests"
label devops       "006B75" "CI/CD, cloud, secrets"
label docs         "C5DEF5" "README, diagrams, presentation"
label special-case "F9D0C4" "Illness, cancellation, leaving, payouts"
label stretch      "EDEDED" "Nice to have if time allows"

# ---------------------------------------------------------------- milestones
for ms in "$M1" "$M2" "$M3" "$M4" "$M5"; do
  run gh api "repos/$REPO/milestones" -f title="$ms" >/dev/null 2>&1 || true
done

# ---------------------------------------------------------------- issues
count=0
issue() {
  local title="$1" labels="$2" ms="$3" who="$4" assignee=""
  [[ "$who" == "M" ]] && assignee="$ME"
  [[ "$who" == "C" ]] && assignee="$MATE"
  count=$((count + 1))
  if [[ "$DRY_RUN" == "1" ]]; then
    cat >/dev/null
    printf '%-3s %-42s %-2s %s\n' "$count" "[$ms]" "$who" "$title"
    return
  fi
  local args=(-R "$REPO" --title "$title" --label "$labels" --milestone "$ms" --body-file -)
  [[ -n "$assignee" ]] && args+=(--assignee "$assignee")
  [[ -n "$PROJECT" ]] && args+=(--project "$PROJECT")
  gh issue create "${args[@]}"
}

# ================================================================ M1: Setup
issue "Require CI checks in branch protection" "devops" "$M1" M <<'EOF'
Branch protection on `main` requires a PR, 1 approval **and** green `server` + `client` CI jobs.

- [ ] Required status checks `server` and `client` selected
- [ ] Direct pushes to `main` are blocked
EOF

issue "Prepare questions for the stakeholder meeting" "docs" "$M1" M <<'EOF'
Collect open questions about the case before the status meeting with Alex + stakeholder.

Ideas:
- Does only accounting approve, or also a line manager?
- Are hourly-paid staff in scope?
- Integration with *Feriepengeinfo* or just an export/report?
- Who enters salaries and working hours?
- Can requests overlap / be partially approved?

- [ ] Questions in `docs/stakeholder-questions.md`
- [ ] Answers written down after the meeting
EOF

issue "Design domain model & ERD" "core,database,docs" "$M1" M <<'EOF'
Design the data model the whole project builds on.

Entities (draft): Employee, User, WorkSchedule, SalaryHistory (month by month), HolidayAccount (weeks 1–5 / week 6), LedgerEntry, RuleSet, RuleParameter (effective from), PublicHoliday, HolidayRequest, TransferAgreement.

- [ ] ERD in `docs/` (image or Mermaid)
- [ ] All IDs are GUIDs
- [ ] Reviewed together by both group members
EOF

issue "Database setup: Postgres, DbUp migrations, Linq2db" "backend,database" "$M1" M <<'EOF'
- [ ] Local Postgres via `docker-compose.yaml`
- [ ] SQL migrations in `server/db/migrations/` run by DbUp on startup
- [ ] Linq2db `DataConnection` registered in DI
- [ ] Connection string from user-secrets / environment, never from git
EOF

issue "NSwag + first endpoint GET /api/employees" "backend" "$M1" M <<'EOF'
End-to-end "hello world" through the whole backend.

- [ ] NSwag configured, Swagger UI available
- [ ] `GET /api/employees` reads from the DB via Linq2db
- [ ] Default Microsoft.AspNetCore.OpenApi removed
EOF

issue "Secrets handling (local, CI, cloud)" "devops,auth" "$M1" M <<'EOF'
- [ ] Local: `dotnet user-secrets`
- [ ] CI: GitHub Secrets
- [ ] Cloud: environment variables
- [ ] `.env` and secrets ignored by git, nothing secret in the repository history
EOF

issue "Integration test infrastructure (WebApplicationFactory + Testcontainers)" "testing,backend" "$M1" M <<'EOF'
- [ ] `BE.Api.Tests` starts the API with `WebApplicationFactory`
- [ ] Real Postgres in a Testcontainer, migrations applied
- [ ] First integration test for `GET /api/employees`
- [ ] Runs in GitHub Actions
EOF

issue "Client shell: React Router, layout, placeholder pages" "frontend" "$M1" C <<'EOF'
- [ ] React Router with routes: Login, My holiday, Approvals, Employees, Admin
- [ ] Shared layout with navigation
- [ ] No `.js` files in client source
EOF

issue "Generate API client with swagger-typescript-api" "frontend" "$M1" C <<'EOF'
- [ ] npm script `gen:api` generating into `client/src/api/`
- [ ] Generated code is not edited by hand
- [ ] Employees placeholder page shows data from `GET /api/employees`
EOF

issue "README skeleton + running AI disclosure log" "docs" "$M1" C <<'EOF'
- [ ] Sections required by the exam: deployed links, current state, authorization policies, testing strategy, board progression, sustainability, further work, AI disclosure
- [ ] Screenshot of the project board at the start
- [ ] Both members add AI usage to the AI disclosure section as they go
EOF

# ================================================================ M2: Users & master data
issue "User accounts: registration/login with safe password hashing" "auth,backend,database" "$M2" C <<'EOF'
- [ ] User table linked to Employee
- [ ] Passwords hashed with a proper algorithm (e.g. ASP.NET `PasswordHasher` / BCrypt), never stored in plain text
- [ ] Server-side validation of input
- [ ] Integration tests for login success / failure
EOF

issue "JWT authentication + roles and authorization policies" "auth,backend" "$M2" C <<'EOF'
Roles: **Employee**, **Accounting**, **Admin**.

- [ ] JWT issued on login, validated on every request
- [ ] Policies: employees only see their own data; accounting sees/approves all; admin edits rules and master data
- [ ] Integration tests proving forbidden access returns 401/403
EOF

issue "Document authorization policies in README" "docs,auth" "$M2" C <<'EOF'
- [ ] Table: role × endpoint/feature → allowed or not
- [ ] Matches the actual policies in code
EOF

issue "Client: login page, auth state, protected routes per role" "frontend,auth" "$M2" C <<'EOF'
- [ ] Login form, token kept in memory/storage, logout
- [ ] Routes hidden/blocked based on role
- [ ] Generated API client sends the token
EOF

issue "Employee CRUD API with validation" "backend" "$M2" C <<'EOF'
- [ ] Create, read, update, deactivate employee
- [ ] Server-side validation (required fields, start date, working hours > 0 …)
- [ ] Integration tests
EOF

issue "Client: employee list, detail and edit pages" "frontend" "$M2" C <<'EOF'
- [ ] List with search
- [ ] Detail with balances placeholder
- [ ] Create/edit form with server-side validation errors shown
EOF

issue "Salary and working-hours history (month by month)" "backend,frontend,database" "$M2" C <<'EOF'
Needed for the DKK liability (salary in the earning period) and part-time accrual.

- [ ] Store salary + weekly hours per employee with effective month
- [ ] API + UI to add/view history
- [ ] Integration tests
EOF

issue "Employee work schedule (hours per weekday)" "backend,frontend,database" "$M2" C <<'EOF'
Default Business Esbjerg schedule: Mon–Thu 7.5 h, Fri 7 h, Sat–Sun 0 (37 h/week). Part-time employees have their own day lengths.

- [ ] Schedule per employee with effective date
- [ ] API + UI
EOF

issue "Public holidays & company days off calendar" "backend,frontend,database" "$M2" C <<'EOF'
- [ ] Danish public holidays + company-specific days off (e.g. 24 and 31 December)
- [ ] Admin API + UI to manage the calendar per year
- [ ] Used by the days→hours conversion (they never count as holiday days)
EOF

# ================================================================ M3: Core holiday engine
issue "Rule parameters with effective-from dates" "core,backend,database" "$M3" M <<'EOF'
Every number/rule is an admin-editable parameter with a start date. New values never rewrite history.

Parameters: days earned per month (2.08), earning/usage periods, main holiday length & window, notice periods, 6th week enabled, week-6 hours per month (3.083), request unit, holiday supplement (1% + 1.48%), payout percentages (12.5% / 2.5%), transfer deadlines.

- [ ] Lookup "value of parameter X on date D"
- [ ] Unit tests incl. a parameter changing mid-period
EOF

issue "Rule presets" "core,database" "$M3" M <<'EOF'
- [ ] "Ferieloven (minimum)" – 5 weeks
- [ ] "Ferieloven + 6. ferieuge (KL 05.11 model)" – Business Esbjerg
- [ ] "Ferieloven + feriefridage"
- [ ] "Hourly-paid staff" – feriegodtgørelse 12.5%
- [ ] Changing a value afterwards makes it a custom rule set
EOF

issue "Admin UI: edit rule parameters and choose preset" "frontend" "$M3" C <<'EOF'
- [ ] List parameters with current value and effective-from date
- [ ] Add a new value with a future start date
- [ ] Select a preset as starting point
EOF

issue "Ledger: two holiday accounts with append-only entries" "core,backend,database" "$M3" M <<'EOF'
Accounts: weeks 1–5 (earn 1 Sep–31 Aug, use until 31 Dec next year) and week 6 (earn calendar year, use 1 May–30 Apr). Balances kept in hours.

Entry types: accrual, usage, transfer, payout, expiry, correction.

- [ ] Balance = sum of entries, per account and per earning year
- [ ] Unit tests (TDD from case examples)
EOF

issue "Monthly accrual engine" "core,backend" "$M3" M <<'EOF'
- [ ] Weeks 1–5: 2.08 days/month (185 h/year full time)
- [ ] Week 6: 3.083 h/month (37 h/year full time)
- [ ] Proportional for part-time
- [ ] Mid-month start/leave: exact days (0.103 week-6 h per calendar day full time)
- [ ] Unit tests for full time, part time, mid-month start and end
EOF

issue "Days → hours conversion" "core,backend" "$M3" M <<'EOF'
Requests are in whole days; a day costs the hours of that weekday in the employee's schedule.

- [ ] Friday = 7 h, Mon–Thu = 7.5 h, full week = 37 h
- [ ] Weekends, public holidays and company days off cost 0
- [ ] Uses the employee's own schedule (part-time)
- [ ] Unit tests
EOF

issue "Holiday request validation" "core,backend" "$M3" M <<'EOF'
- [ ] Correct account (weeks 1–5 vs week 6)
- [ ] Enough balance in the usable period
- [ ] Notice: 3 months for main holiday, 1 month for other holiday (week 6 is placed at the employee's request)
- [ ] Main holiday rule: right to 15 consecutive working days 1 May–30 Sep
- [ ] Whole days only
- [ ] Validation errors returned as clear messages
- [ ] Unit tests for every rule
EOF

issue "Consumption order (earliest expiry first)" "core,backend" "$M3" M <<'EOF'
- [ ] Usage takes hours that expire first
- [ ] Transferred week-6 hours are used before newly earned ones
- [ ] Unit tests
EOF

issue "Holiday request API (create, list, withdraw)" "backend" "$M3" M <<'EOF'
- [ ] Employee creates a request; cost in hours is calculated server-side
- [ ] Employee lists own requests and withdraws a pending one
- [ ] Integration tests
EOF

issue "Approval API for accounting" "backend,auth" "$M3" M <<'EOF'
- [ ] Accounting approves/rejects with optional comment
- [ ] Approval writes the usage entry to the ledger
- [ ] Only Accounting role allowed (integration test)
EOF

issue "Client: request holiday page with live cost preview" "frontend" "$M3" M <<'EOF'
- [ ] Balances of both accounts with expiry dates (31 Dec / 30 Apr)
- [ ] Date range picker, shows cost in hours/days before submitting
- [ ] Shows validation errors from the API
EOF

issue "Client: accounting approval queue" "frontend" "$M3" M <<'EOF'
- [ ] List of pending requests with employee, dates, cost and remaining balance
- [ ] Approve / reject with comment
EOF

# ================================================================ M4: Period end, payouts & special cases
issue "Expiry handling and flags" "core,backend" "$M4" M <<'EOF'
- [ ] Weeks 1–5 expire 31 December, week 6 expires 30 April
- [ ] Flag balances that must be taken, transferred or paid out
- [ ] First 4 weeks unused → holiday fund (record only)
- [ ] Unit tests around the boundary dates
EOF

issue "Transfer agreements" "core,backend,frontend" "$M4" M <<'EOF'
- [ ] Record written agreement: 5th week by 31 Dec after period end, week 6 by 30 Sep
- [ ] Transfer creates ledger entries into the next period
- [ ] Accounting UI to register an agreement
EOF

issue "Payout triggers" "core,backend,special-case" "$M4" M <<'EOF'
Employees never request payouts – situations trigger them, accounting handles them.

- [ ] 5th week not taken/transferred by 31 Dec
- [ ] Week-6 hours not taken/transferred by 30 Apr (2.5%)
- [ ] Holiday obstacle at period end
- [ ] Retirement / leaving the labour market, death (paid to estate)
- [ ] Unit tests
EOF

issue "Holiday liability in DKK (feriepengeforpligtelse)" "core,backend" "$M4" M <<'EOF'
- [ ] Weeks 1–5: 12.5% of holiday-qualifying salary × share of days not taken
- [ ] Week 6: 2.5% of salary in the calendar year × share of hours not taken
- [ ] Holiday supplement not yet paid out
- [ ] Per employee and company total; uses parameters valid at the time
- [ ] Unit tests
EOF

issue "Employee leaving (fratræden) – final settlement" "core,backend,special-case" "$M4" M <<'EOF'
- [ ] Everything not taken is settled with the final salary
- [ ] Settlement shows amounts per account (first 4 weeks reported via Feriepengeinfo, 5th/6th week paid directly)
- [ ] Unit + integration tests
EOF

issue "Illness before/during holiday (holiday obstacle)" "core,backend,frontend,special-case" "$M4" C <<'EOF'
- [ ] Ill before holiday starts → holiday not started, hours returned
- [ ] Ill during weeks 1–5 → replacement days after the first 5 sick days, with documentation
- [ ] Ill during week 6 → suspension in special cases
- [ ] API + UI for accounting to register it
- [ ] Tests
EOF

issue "Employer cancellation of approved holiday" "backend,frontend,special-case" "$M4" C <<'EOF'
- [ ] Only before the holiday starts; never interrupt a started holiday
- [ ] Reason recorded, financial loss of employee noted
- [ ] Hours returned to the ledger
- [ ] Tests
EOF

issue "Client: accounting overview & liability report" "frontend" "$M4" C <<'EOF'
- [ ] All employees with balances in hours/days and DKK
- [ ] Expiring / must-be-paid flags highlighted
- [ ] Company total liability
EOF

issue "Hourly-paid staff (feriegodtgørelse)" "core,backend,stretch" "$M4" M <<'EOF'
- [ ] Holiday pay as cash percentage (12.5% weeks 1–5, 2.5% week 6) instead of paid time off
- [ ] Tests
EOF

# ================================================================ M5: Deploy, testing & docs
issue "Deploy centralized database to the cloud" "devops,database" "$M5" M <<'EOF'
- [ ] Managed Postgres in the cloud, migrations applied
- [ ] Both members develop against it (or a copy) – credentials only via secrets
EOF

issue "Deploy API and client to the cloud" "devops" "$M5" M <<'EOF'
- [ ] API with Swagger reachable publicly
- [ ] Client reachable publicly, talking to the deployed API
- [ ] Config via environment variables only
- [ ] Links in README
EOF

issue "E2E tests with Playwright" "testing,frontend" "$M5" C <<'EOF'
- [ ] Flow: login as employee → request holiday → login as accounting → approve → balance decreased
- [ ] Written in TypeScript
- [ ] Runs in GitHub Actions
EOF

issue "Seed / demo data" "database" "$M5" C <<'EOF'
- [ ] Demo users for each role, a few employees with history, holiday requests
- [ ] Only for dev/demo, not real data
EOF

issue "Sustainability: Lighthouse before/after" "frontend,docs" "$M5" C <<'EOF'
- [ ] Lighthouse report before optimisation (screenshot)
- [ ] Improvements (bundle size, lazy routes, caching, images…)
- [ ] Report after (screenshot) – both in README
EOF

issue "README final version" "docs" "$M5" B <<'EOF'
- [ ] Current state, known bugs
- [ ] Testing strategy (TDD for domain, test-last elsewhere)
- [ ] Board progression screenshots (halfway + end)
- [ ] Further work
- [ ] AI disclosure complete (files generated, feedback, brainstorming, text)
EOF

issue "Exam presentation" "docs" "$M5" B <<'EOF'
10-minute presentation, max 1 minute of GUI demo, focus on implementation details.

- [ ] Slides with large code examples
- [ ] Each member can explain the whole solution (code ownership)
EOF

echo
echo "Done: $count issues $( [[ "$DRY_RUN" == "1" ]] && echo '(dry run, nothing created)' || echo 'created' )."
