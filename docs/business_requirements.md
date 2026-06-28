# Trading Journal

## Product Requirements Document (PRD)

**Version:** 1.0
**Status:** Draft

---

# 1. Introduction

## Purpose

Trading Journal is a web application that enables traders to document, organize, and analyze their trading activity.

The application replaces spreadsheets and scattered notes with a centralized journal where users can manually record trades or import them from supported trading platforms. It provides performance analytics, trade organization, and historical insights to help traders improve their decision-making over time.

Trading Journal **does not execute trades or connect to live financial markets**. Its purpose is to support post-trade review and continuous improvement.

---

# 2. Objectives

The application aims to help traders:

* Maintain an organized history of all completed trades.
* Record trades manually or import them from CSV files.
* Reduce repetitive data entry through automated imports.
* Attach notes and screenshots to trades.
* Organize trades using custom tags.
* Measure trading performance through meaningful statistics.
* Review historical trades through search and filtering.
* Identify strengths, weaknesses, and recurring trading patterns.

---

# 3. Target Users

The application is designed for individual retail traders who trade:

* Futures
* Forex
* Stocks
* Options
* Cryptocurrency

Users may trade through any broker or prop firm that provides trade history exports.

---

# 4. Project Scope

## In Scope

* User authentication
* Manual trade entry
* CSV trade imports
* Trade journal
* Performance dashboard
* Statistics and analytics
* Trade notes
* Screenshot uploads
* Trade tagging
* Search and filtering

## Out of Scope

* Trade execution
* Live market data
* Broker integrations (API)
* Portfolio management
* Copy trading
* Order management

---

# 5. Functional Requirements

## 5.1 User Accounts

The system shall allow users to:

* Register an account
* Log in
* Log out
* Manage their profile

Each user's journal and trading data shall remain private.

---

## 5.2 Trade Journal

The Trade Journal is the core feature of the application.

The system shall allow users to:

* Create trades manually
* Import trades from CSV files
* View all recorded trades
* Edit existing trades
* Delete trades
* View detailed trade information

Trades created manually and trades imported from CSV shall behave identically once stored in the system.

Each trade shall support the following information:

* Instrument
* Direction (Long / Short)
* Entry price
* Exit price
* Quantity
* Entry date and time
* Exit date and time
* Profit / Loss
* Commission
* Trade duration

---

## 5.3 CSV Import

The system shall allow users to import completed trades from CSV files as an alternative to manual trade entry.

The import process shall:

* Validate uploaded files
* Parse trade records
* Detect duplicate trades
* Store valid records
* Report invalid records

The import system should be extensible to support additional broker formats in future releases.

---

## 5.4 Trade Notes

Users shall be able to attach personal notes to every trade.

Typical notes may include:

* Trade rationale
* Mistakes made
* Emotional state
* Lessons learned
* Market observations

Notes shall remain editable at any time.

---

## 5.5 Screenshot Management

Users shall be able to upload one or more screenshots for each trade.

Example screenshots include:

* Entry chart
* Exit chart
* Trade setup
* Higher timeframe analysis

---

## 5.6 Trade Tags

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

## 5.7 Dashboard

The dashboard provides a high-level overview of the trader's performance.

Displayed information shall include:

* Total Profit/Loss
* Daily Profit/Loss
* Number of Trades
* Win Rate
* Average Winning Trade
* Average Losing Trade
* Profit Factor
* Best Trading Day
* Worst Trading Day
* Recent Trades

The dashboard shall present these metrics using summary cards and visual charts.

---

## 5.8 Performance Analytics

The system shall automatically calculate trading statistics based on all recorded trades.

Supported analytics include:

* Equity Curve
* Daily Profit/Loss
* Win Rate
* Average Winning Trade
* Average Losing Trade
* Profit Factor
* Trade Frequency

Statistics shall automatically update whenever trades are created, modified, imported, or deleted.

---

## 5.9 Search

Users shall be able to search trades by:

* Instrument
* Notes
* Tags

---

## 5.10 Filtering

Users shall be able to filter trades by:

* Date range
* Instrument
* Direction
* Profit/Loss
* Tags

Multiple filters should be usable simultaneously.

---

# 6. User Workflows

## Manual Trade Entry

1. Log in.
2. Create a new trade.
3. Enter trade details.
4. Add notes.
5. Upload screenshots.
6. Assign tags.
7. Save the trade.
8. Review updated dashboard statistics.

---

## CSV Import

1. Log in.
2. Upload a CSV file.
3. Review parsed trades.
4. Resolve any validation errors.
5. Confirm the import.
6. Add notes, screenshots, and tags as needed.
7. Review updated dashboard statistics.

---

# 7. Non-Functional Requirements

The application should:

* Support thousands of recorded trades.
* Import large CSV files efficiently.
* Provide fast search and filtering.
* Preserve user data reliably.
* Be responsive on desktop and tablet devices.
* Be deployable as a self-hosted application using a single executable.

---

# 8. Minimum Viable Product (MVP)

The first release shall include:

* User authentication
* Manual trade entry
* CSV trade import
* Trade journal
* Trade notes
* Screenshot uploads
* Custom tags
* Search and filtering
* Dashboard
* Equity curve
* Daily Profit/Loss
* Win Rate
* Basic performance statistics

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
