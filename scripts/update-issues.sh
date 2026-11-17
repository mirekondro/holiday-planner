#!/usr/bin/env bash
# Updates the existing GitHub issues after the case clarifications from the status meeting (16 Nov 2026).
# Generated with AI (Claude) – see README "AI disclosure".
#
# Usage:
#   DRY_RUN=1 bash scripts/update-issues.sh   # only prints what would change
#   bash scripts/update-issues.sh
#
# Issues are found by their exact title. If a title was renamed by hand, that step is skipped with a warning.
# Edited issues get a NEW body (old checkboxes are replaced).

set -euo pipefail

REPO="mirekondro/holiday-planner"
ME="mirekondro"
MATE="${MATE-sasahrad}"
PROJECT="${PROJECT-Holiday Planner}"

DRY_RUN="${DRY_RUN:-0}"

M2="M2: Users & master data"
M3="M3: Core holiday engine"
M4="M4: Period end, payouts & special cases"

WHY="Case clarification 16 Nov: the system only tracks time (hours), money is handled by payroll."

num() {
  gh issue list -R "$REPO" --state all --limit 300 --json number,title \
    --jq ".[] | select(.title == \"$1\") | .number" | head -1
}

# close_issue "<title>" "<completed|not planned>" "<comment>"
close_issue() {
  local n; n=$(num "$1")
  if [[ -z "$n" ]]; then echo "WARN not found, skipped: $1"; return; fi
  if [[ "$DRY_RUN" == "1" ]]; then echo "CLOSE  #$n ($2)  $1"; return; fi
  gh issue close "$n" -R "$REPO" --reason "$2" --comment "$3"
}

# edit "<old title>" "<new title>" "<labels to add or empty>"   (new body from stdin)
edit() {
  local n; n=$(num "$1")
  if [[ -z "$n" ]]; then echo "WARN not found, skipped: $1"; cat >/dev/null; return; fi
  if [[ "$DRY_RUN" == "1" ]]; then
    cat >/dev/null
    if [[ "$1" == "$2" ]]; then echo "EDIT   #$n  $1"; else echo "EDIT   #$n  $1  ->  $2"; fi
    return
  fi
  local args=(-R "$REPO" --title "$2" --body-file -)
  [[ -n "$3" ]] && args+=(--add-label "$3")
  gh issue edit "$n" "${args[@]}"
}

# issue "<title>" "<labels>" "<milestone>" <M|C|B>   (body from stdin)
issue() {
  local title="$1" labels="$2" ms="$3" who="$4" assignee=""
  [[ "$who" == "M" ]] && assignee="$ME"
  [[ "$who" == "C" ]] && assignee="$MATE"
  if [[ -n "$(num "$title")" ]]; then echo "WARN already exists, skipped: $title"; cat >/dev/null; return; fi
  if [[ "$DRY_RUN" == "1" ]]; then cat >/dev/null; echo "NEW    ($who)  $title"; return; fi
  local args=(-R "$REPO" --title "$title" --label "$labels" --milestone "$ms" --body-file -)
  [[ -n "$assignee" ]] && args+=(--assignee "$assignee")
  [[ -n "$PROJECT" ]] && args+=(--project "$PROJECT")
  gh issue create "${args[@]}"
}

# ================================================================ CLOSE

close_issue "Prepare questions for the stakeholder meeting" "completed" \
  "Answered in the case clarifications from the status meeting on 16 Nov (Moodle: Case Details Clarifications)."
close_issue "Holiday liability in DKK (feriepengeforpligtelse)" "not planned" "$WHY"
close_issue "Salary and working-hours history (month by month)" "not planned" \
  "$WHY Working-hours history is covered by the schedule periods in 'Employee work schedule'."
close_issue "Hourly-paid staff (feriegodtgørelse)" "not planned" \
  "Case clarification 16 Nov: hourly-paid employees are irrelevant (they use the governmental Feriekonto)."

# ================================================================ EDIT – users & roles

edit "User accounts: registration/login with safe password hashing" \
     "User accounts created by system owner + login with safe password hashing" "" <<'EOF'
- No self-registration: the **system owner** creates users.
- Every person has an **employee** account with an organization email `x@businessesbjerg.com`.
- There is **exactly one accountant** account and **exactly one system owner** account. Their logins are separate from their own employee accounts.
- 3rd-party authentication is **not** required (own login is fine).

- [ ] Email must be unique and end with `@businessesbjerg.com`
- [ ] Passwords hashed (ASP.NET `PasswordHasher` / BCrypt), never plain text
- [ ] Seed the single accountant + system owner accounts
- [ ] Server-side validation
- [ ] Integration tests for login success / failure
EOF

edit "JWT authentication + roles and authorization policies" \
     "JWT authentication + roles and authorization policies" "" <<'EOF'
Roles: **Employee**, **Accountant**, **SystemOwner**.

| Feature | Employee | Accountant | SystemOwner |
|---|---|---|---|
| Own requests & own balance | ✔ | | |
| See everyone's **planned holiday** (calendar) | ✔ | ✔ | ✔ |
| Approve / decline requests (holiday, cancellation, transfer) | | ✔ | |
| Edit work schedules | | ✔ | ✔ |
| Create users, global rules, public holidays | | | ✔ |

- [ ] JWT issued on login, validated on every request
- [ ] Policies implemented as in the table
- [ ] Integration tests proving forbidden access returns 401/403
EOF

edit "Employee CRUD API with validation" "Employee CRUD API with validation" "" <<'EOF'
- [ ] Only the system owner creates / edits / deactivates employees
- [ ] Creating an employee **requires a work schedule** (see work schedule issue)
- [ ] Server-side validation (org email domain, required fields, start date)
- [ ] Integration tests
EOF

edit "Employee work schedule (hours per weekday)" \
     "Employee work schedule periods (Mon–Fri From–To) with full history" "core" <<'EOF'
- Schedule = for each of Mon–Fri a time **From HH:MM to HH:MM** (or day off). Hours per day are derived from it.
- A schedule belongs to a **period** (e.g. week X – week Y in year Z). People can go full time ↔ part time ↔ different part time.
- **Full history** must always be kept – accrual and holiday cost depend on which schedule was valid when.
- Only **accountant or system owner** can edit schedules, never the employee.

- [ ] DB: schedule periods per employee, no overlapping periods
- [ ] API + UI to add a new period / view history
- [ ] Domain: "hours for employee E on date D"
- [ ] Tests
EOF

# ================================================================ EDIT – core

edit "Rule parameters with effective-from dates" "Rule parameters with effective-from dates" "" <<'EOF'
Every rule is an admin-editable parameter with a start date. **A change must never affect past or current activity** – e.g. changing accrual from 2.08 to 2.16 must not change holiday years where 2.08 was used. Correctness of data is most important.

Parameters (time only, no money): days earned per month (2.08), earning/usage periods, main holiday length & window, notice periods, 6th week enabled, week-6 hours per month (3.083), max holiday debt (1 week), transfer allowed + max (1 week), transfer deadlines.

- [ ] Lookup "value of parameter X for holiday year / date D"
- [ ] Unit tests incl. a change between holiday years
EOF

edit "Rule presets" "Rule presets" "" <<'EOF'
- [ ] "Ferieloven (minimum)" – 5 weeks
- [ ] "Ferieloven + 6. ferieuge (KL 05.11 model)" – Business Esbjerg
- [ ] "Ferieloven + feriefridage"
- [ ] Changing a value afterwards makes it a custom rule set
EOF

edit "Ledger: two holiday accounts with append-only entries" \
     "Ledger: two holiday accounts with append-only entries" "" <<'EOF'
Accounts: weeks 1–5 (earn 1 Sep–31 Aug, use until 31 Dec next year) and week 6 (earn calendar year, use 1 May–30 Apr).

- Unit is **hours with two decimals** (`decimal`, `numeric(…,2)` in DB). Only time – no money.
- Entry types: accrual, usage, cancellation, transfer, schedule conversion, expiry, payout flag.

- [ ] Balance = sum of entries, per account and per earning year
- [ ] Unit tests (TDD from case examples)
EOF

edit "Monthly accrual engine" "Monthly accrual engine" "" <<'EOF'
- [ ] Weeks 1–5: 2.08 days/month, week 6: 3.083 h/month (full time)
- [ ] Proportional to the **schedule period** valid in that month (part time)
- [ ] Mid-month start/leave: exact days
- [ ] Rounding to 2 decimals without drift: round the **cumulative** total and post the difference (month n = round(total_n) − round(total_n-1)), so a full year gives exactly 185.00 h / 37.00 h
- [ ] Unit tests: full time, part time, schedule change mid-year, mid-month start/end
EOF

edit "Holiday request validation" "Holiday request validation" "" <<'EOF'
- [ ] Decide which account is used (weeks 1–5 vs week 6 – separate balances)
- [ ] **Holiday debt** allowed, max **1 week** (5 days of the employee's schedule)
- [ ] Balance may not be negative on **31 August** (must be 0 or positive)
- [ ] Notice: 3 months main holiday / 1 month other holiday (week 6 at the employee's request)
- [ ] Main holiday rule: 15 consecutive working days 1 May–30 Sep
- [ ] Whole days only, cost in hours from the schedule period
- [ ] Clear validation messages
- [ ] Unit tests for every rule
EOF

edit "Holiday request API (create, list, withdraw)" \
     "Holiday request API (create, list, withdraw)" "" <<'EOF'
- [ ] Employee creates a request; cost in hours calculated server-side
- [ ] Employee lists own requests and withdraws a **pending** one
- [ ] Approved but not yet used holiday can only be cancelled via a cancellation request (separate issue)
- [ ] No email / 3rd-party notifications needed
- [ ] Integration tests
EOF

edit "Client: request holiday page with live cost preview" \
     "Client: request holiday page with live cost preview" "" <<'EOF'
- [ ] Balances of both accounts in hours (+ day equivalent) with expiry dates
- [ ] Date range picker, shows cost before submitting
- [ ] **Future balance**: pick a date and see how much holiday could be taken then ("if I wait until December")
- [ ] Shows validation errors from the API
EOF

# ================================================================ EDIT – period end & special cases

edit "Transfer agreements" "Transfer requests (max 1 week) approved by accountant" "" <<'EOF'
By law max **1 week (5 days)** can be transferred to the next holiday period, only with written agreement. It works like a holiday request: **employee requests → accountant approves/denies**.

Business Esbjerg does not allow it today, but it must be implemented (future policy) – enabled/disabled by a rule parameter.

- [ ] Request + approve/deny flow, deadlines from parameters
- [ ] Approved transfer creates ledger entries into the next period
- [ ] Disabled by parameter → request rejected with a clear message
- [ ] Tests
EOF

edit "Payout triggers" "Flag hours for payout (time only)" "" <<'EOF'
The system stores no money. It only shows **which hours** payroll must pay out, and why.

- [ ] 5th week not taken/transferred by 31 Dec
- [ ] Week-6 hours not taken/transferred by 30 Apr
- [ ] Holiday obstacle at period end
- [ ] Employee leaving
- [ ] Unit tests
EOF

edit "Employee leaving (fratræden) – final settlement" \
     "Employee leaving (fratræden) – stop accrual and show remaining hours" "" <<'EOF'
- [ ] End date on the employee; accrual stops (exact days in the last month)
- [ ] Remaining hours per account shown for payroll (no amounts)
- [ ] Unit + integration tests
EOF

edit "Client: accounting overview & liability report" \
     "Client: accounting overview of balances" "" <<'EOF'
- [ ] All employees with balances (hours + days) per account
- [ ] Expiring / must-be-taken / payout flags highlighted
- [ ] Pending requests count
EOF

# ================================================================ NEW

issue "Schedule change: convert remaining balance so days stay the same" "core,backend" "$M3" M <<'EOF'
Holiday is a right to **days off**; hours are only the unit. When a work schedule changes, the remaining balance is converted so the **number of days stays the same** (hours recalculated with the new day length).

The money difference (*feriedifference*) is payroll's job – the system stores nothing extra.

- [ ] Conversion entry in the ledger on the schedule change date
- [ ] Decide & document how "day length" is defined for uneven schedules
- [ ] Unit tests: full time → part time and back
EOF

issue "Future balance projection" "core,backend" "$M3" M <<'EOF'
"I could take 3 days now, but how much if I wait until December?"

- [ ] API: balance per account on a future date = current balance + future accrual (schedule-aware) − approved/pending future holiday − expiry
- [ ] Unit tests
EOF

issue "Holiday calendar / Gantt overview of all employees" "frontend,backend" "$M3" C <<'EOF'
Everyone can see everyone's planned holiday. It must be easy to see how many people are absent/present on a given day (calendar or Gantt). The old system's "Månedsoversigt" (rows = people, columns = days) is a good model. Correct data matters more than styling.

- [ ] API: planned/approved holiday for all employees in a date range (no balances of others)
- [ ] Month view: rows = employees, columns = days, weekends/public holidays marked
- [ ] Count of absent people per day
- [ ] Own holiday plan view for the employee
EOF

issue "Cancellation request for approved holiday" "core,backend,frontend" "$M3" M <<'EOF'
Approved holiday that has not been used yet can only be cancelled by **request → accountant approves/denies**.

- [ ] Employee creates a cancellation request for a future approved holiday
- [ ] Approval returns the hours to the ledger (correct account & period)
- [ ] Not possible for holiday already started/used
- [ ] Tests
EOF

echo
echo "Done$( [[ "$DRY_RUN" == "1" ]] && echo ' (dry run, nothing changed)' )."
