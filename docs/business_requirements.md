# Trading Journal

## Product Requirements Document (PRD)

**Version:** 2.0
**Status:** Living document — reflects the shipped release

> **How to read this document.** Every requirement is tagged **[Implemented]**,
> **[Planned]**, or **[Out of scope]**. The shipped data model and API are
> described in the [technical design document](./technical_document.md) and the
> [entity diagram](./entity_diagram.png); features that are designed but not yet
> built are also tracked in the [roadmap](../README.md#roadmap).

---

# 1. Introduction

## Purpose

Trading Journal is a web application that enables traders to document, organize, and analyze their trading activity.

The application replaces spreadsheets and scattered notes with a centralized journal where users record their completed trades and review performance analytics. It provides performance statistics, trade organization, and historical insight to help traders improve their decision-making over time.

Trading Journal **does not execute trades or connect to live financial markets**. Its purpose is to support post-trade review and continuous improvement.

> **Current release.** Trades are entered manually and summarized on a
> performance dashboard. CSV import, trade notes, screenshots, and tags are
> designed but not yet implemented (see [§5.5–§5.8](#55-csv-import-planned) and
> [§9](#9-future-enhancements)).

---

# 2. Objectives

The application aims to help traders:

* **[Implemented]** Maintain an organized history of all completed trades.
* **[Implemented]** Record trades manually.
* **[Implemented]** Measure trading performance through meaningful statistics.
* **[Implemented]** Review historical trades through search and filtering.
* **[Implemented]** Identify strengths, weaknesses, and recurring patterns — via
  win rate, profit factor, average win/loss, and the equity curve.
* **[Planned]** Import trades from CSV files to reduce repetitive data entry.
* **[Planned]** Attach notes and screenshots to trades.
* **[Planned]** Organize trades using custom tags.

---

# 3. Target Users

The application is designed for individual retail traders who trade:

* Futures
* Forex
* Stocks
* Options
* Cryptocurrency

Users may trade through any broker or prop firm. Manual entry is always
available; CSV import (planned) will require brokers to provide a trade-history
export.

---

# 4. Project Scope

## Implemented

* User accounts (register / log in / log out / view profile)
* Market management (create, view, edit, and delete markets)
* Manual trade entry and full trade CRUD
* Per-user trade journal where every trade belongs to a market and inherits its
  currency
* Server-computed profit/loss and commission
* Performance dashboard with statistics and an equity curve
* Search, filtering, and sorting of trades
* Light / dark / system theme
* Single-artifact, self-hosted deployment

## Planned

* CSV trade imports
* Trade notes
* Screenshot uploads
* Trade tagging

## Out of scope

* Trade execution
* Live market data
* Broker integrations (API)
* Portfolio management
* Copy trading
* Order management

---

# 5. Functional Requirements

## 5.1 User Accounts (Implemented)

The system shall allow users to:

* Register an account with a username, email, and password.
* Log in with email and password.
* Log out.
* View their own profile.

Rules:

* Each user's journal and trading data shall remain private and accessible only
  to that user.
* Email addresses and usernames shall be unique; registering an existing one
  shall be rejected with a `409 Conflict`.
* Passwords shall be stored only as a one-way hash.

> **Planned:** password reset, email verification, profile editing, and refresh
> tokens.

---

## 5.2 Markets & Trade Journal (Implemented)

### Markets

A **market** groups a user's trades and defines the currency they are recorded
in. The system shall allow users to:

* Create a market with a name and a currency.
* View all of their markets and the number of trades in each.
* Edit a market's name or currency.
* Delete a market, but only when it has no associated trades.

Rules:

* A market belongs to exactly one user; users only ever see their own markets.
* A market name shall be unique per user.
* A currency shall be a 3-letter ISO 4217 code (for example `USD`).
* Every trade belongs to exactly one market and inherits the market's currency.
* Deleting a market that still has trades shall be refused (`409 Conflict`) with
  a message explaining that the trades must be deleted or reassigned first; the
  trade history shall never be silently discarded.

### Trade Journal

The Trade Journal is the core feature of the application.

The system shall allow users to:

* Create trades manually
* View all recorded trades
* View a single trade's details
* Edit existing trades
* Delete trades

Each trade shall support the following information:

| Field | Entered or derived | Notes |
|---|---|---|
| Market | Entered | Every trade belongs to a market (required) |
| Instrument | Entered | Free text, max 20 characters |
| Direction | Entered | Long or Short (the enum also reserves Sideways) |
| Quantity | Entered | Whole number of units |
| Entry price | Entered | Decimal |
| Exit price | Entered | Decimal |
| Entry date and time | Entered | Stored in UTC |
| Exit date and time | Entered | Stored in UTC |
| Commission per unit | Entered | Flat rate charged per unit |
| Currency | **Derived** | Inherited from the trade's market |
| Commission | **Derived** | `CommissionPerUnit × Quantity`, computed by the server |
| Profit / Loss | **Derived** | Computed by the server (see below) |
| Trade duration | **Derived** | Exit time − entry time |

Rules:

* A trade always belongs to exactly one user and exactly one market.
* A trade inherits its currency from its market.
* Profit/loss shall be computed by the server as
  `(exit − entry) × quantity − commission` for a Long trade, inverted for a
  Short trade. The client never supplies profit/loss.
* The client supplies a **per-unit commission**; the server derives the total
  commission as `CommissionPerUnit × Quantity`. There is no hard-coded default.
* Editing a trade shall re-derive its commission and profit/loss from the stored
  values.

> **Planned:** CSV import (created and imported trades behave identically once
> stored).

---

## 5.3 Dashboard (Implemented)

The dashboard provides a high-level overview of the trader's performance. It
shall display:

* Total trades
* Net profit/loss
* Win rate
* Gross profit and gross loss
* Total commission paid
* Average winning trade
* Average losing trade
* Profit factor
* Best trade and worst trade
* An equity curve
* A recent-trades table

Metrics shall be derived from the user's full trade history and update
automatically whenever trades are created, edited, or deleted.

Because each market carries a single currency and every trade inherits its
market's currency, all monetary metrics are computed per market — net P/L,
average win/loss, best/worst trade, total commission, and the equity curve are
never summed across currencies. A compact market selector switches the
dashboard between an **All markets** view, which shows only currency-independent
metrics (total trades, overall win rate, and market/currency counts), and a
single market's performance panel, which shows its monetary metrics, commission,
and equity curve in that market's own currency. The recent-trades table and the
selection are independent, and it formats each trade in its own market currency.
The application never converts or combines amounts across currencies.

> **Planned:** daily profit/loss, best/worst trading day, trade frequency, and
> server-side statistics for large histories.

---

## 5.4 Search, Filtering & Sorting (Implemented)

Users shall be able to:

* Search trades by instrument (case-insensitive substring match).
* Filter trades by direction (Long / Short).
* Filter trades by exit-date range.
* Filter with multiple criteria simultaneously.
* Sort the trade table by instrument, exit time, or profit/loss.

> **Planned:** search and filter by notes, tags, and profit/loss.

---

## 5.5 CSV Import (Planned)

The system shall allow users to import completed trades from CSV files as an alternative to manual trade entry.

The import process shall:

* Validate uploaded files
* Parse trade records
* Detect duplicate trades
* Store valid records
* Report invalid records

The import system should be extensible to support additional broker formats in future releases.

---

## 5.6 Trade Notes (Planned)

Users shall be able to attach personal notes to every trade.

Typical notes may include:

* Trade rationale
* Mistakes made
* Emotional state
* Lessons learned
* Market observations

Notes shall remain editable at any time.

---

## 5.7 Screenshot Management (Planned)

Users shall be able to upload one or more screenshots for each trade.

Example screenshots include:

* Entry chart
* Exit chart
* Trade setup
* Higher timeframe analysis

---

## 5.8 Trade Tags (Planned)

Users shall be able to assign custom tags to trades.

Example tags include:

* Breakout
* Reversal
* News
* A+
* FOMO
* Revenge Trade
* Scalping

Tags shall be searchable and filterable.

---

# 6. User Workflows

## Manual Trade Entry (Implemented)

1. Log in.
2. Open the **Markets** page and create a market (name + currency) if one does
   not already exist.
3. Open the **Trades** page and choose **Add trade**.
4. Select the market and enter the instrument, direction, quantity, entry/exit
   prices, and entry/exit times.
5. Enter the per-unit commission; the server derives the total.
6. Save the trade.
7. Review the updated dashboard statistics.

---

## Reviewing Performance (Implemented)

1. Log in.
2. Open the dashboard. It opens on **All markets**, showing currency-independent
   metrics (total trades, overall win rate, and market/currency counts).
3. Use the market selector to pick a market and see its net profit/loss, win
   rate, profit factor, average win/loss, total commission, and equity curve in
   that market's own currency.
4. Open the **Trades** page to search, filter, and sort the underlying trades.
5. Edit or delete a trade and watch the metrics update.

---

## CSV Import (Planned)

1. Log in.
2. Upload a CSV file.
3. Review parsed trades.
4. Resolve any validation errors.
5. Confirm the import.
6. Add notes, screenshots, and tags as needed.
7. Review updated dashboard statistics.

---

# 7. Non-Functional Requirements

The application shall:

* **[Implemented]** Persist data reliably in an embedded database file, with no
  database server to install or operate.
* **[Implemented]** Isolate every user's data; a user can never read or modify
  another user's trades.
* **[Implemented]** Require authentication for all trade data.
* **[Implemented]** Be responsive on desktop and tablet devices.
* **[Implemented]** Be deployable as a self-hosted, single executable that serves
  both the API and the web UI.
* **[Planned]** Support thousands of recorded trades efficiently.
* **[Planned]** Import large CSV files efficiently.
* **[Planned]** Provide fast search and filtering over large histories.

---

# 8. Release Status (MVP)

| Area | Status |
|---|---|
| User authentication | Implemented |
| Market management (CRUD) | Implemented |
| Manual trade entry | Implemented |
| Trade journal (CRUD) | Implemented |
| Server-side profit/loss & commission | Implemented |
| Dashboard & performance statistics | Implemented |
| Equity curve | Implemented |
| Search, filtering & sorting | Implemented |
| Light / dark theme | Implemented |
| CSV trade import | Planned |
| Trade notes | Planned |
| Screenshot uploads | Planned |
| Custom tags | Planned |
| Daily profit/loss & best/worst day | Planned |

---

# 9. Future Enhancements

Potential future features include:

* AI-generated trade reviews
* Strategy performance tracking
* Monthly performance reports
* Risk metrics (R-Multiple, Expectancy)
* Mobile companion application
* Public trade sharing
* Goal tracking
* Habit tracking
