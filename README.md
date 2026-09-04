# RaceDay
## Description

RaceDay is a full-stack, web-based event management system built for the South African road running, walking, and cycling community. Many community races are currently managed through paper-based registration, spreadsheets, and disconnected communication channels — RaceDay replaces that with a single platform.

Event Organisers can create and manage events, categories, and participant results. Participants can browse upcoming events, enter events by selecting a category, track their personal performance history, and prepare for race day using venue information.

This repository covers Part 1 — System Planning and Database: an Entity Relationship Diagram, a full API endpoint plan, and a SQL Server database script. No application code is written in this part so Parts 2 (RESTful API) and 3 (MVC web application) will be build on this planning.

## Roles

The system supports two distinct user roles:

Organiser — can create, edit and delete events, manage event categories, capture participant results, and view all event enrolments.
Participant — can create an account, browse events, enter an event by selecting a category, view their own enrolments, and track their personal results.

Role-based access will be enforced at the API level in Part 2 and reflected consistently in the MVC interface in Part 3.

## Repository Structure
docs/
├── raceday_erd.png                  # Section A — Entity Relationship Diagram
├── raceday_api_endpoint_plan.md     # Section B — API Endpoint Plan
└── raceday_database_script.sql      # Section C — SQL Database Script
.github/
└── workflows/
    └── validate-docs.yml            # CI/CD workflow validating /docs structure
## Database Design (Section A)

The ERD (docs/raceday_erd.png) contains 6 entities: User, Event, Category, Venue, Enrolment, Result.

User holds both Organisers and Participants, distinguished by a role column.
Event belongs to an Organiser (User) and has many Category and Venue records.
Category belongs to an Event; Participants enrol into a Category, not directly into an Event.
Enrolment resolves the many-to-many relationship between Participants and Categories.
Result is captured against an Enrolment once the event has taken place.
## API Endpoint Plan (Section B)

See docs/raceday_api_endpoint_plan.md for the full table of 19 endpoints covering Authentication, User Profile, Events, Categories, Enrolments, and Results, each with HTTP method, route, description, role required, request body, and expected response.

## Database Script (Section C)

docs/raceday_database_script.sql creates the full schema in SQL Server (SSMS), with primary keys, foreign keys, and constraints (NOT NULL, UNIQUE, DEFAULT, CHECK) for all 6 tables, and seeds the database with:

2 Organisers and 2 Participants
3 Events
6 Categories (across the 3 events)
3 Venues
4 sample Enrolments
2 sample Results

To run: open the script in SSMS and execute it — it creates the RaceDayDB database if it does not already exist.

## CI/CD

A GitHub Actions workflow (.github/workflows/validate-docs.yml) runs on every push to main and validates that the /docs folder exists and contains the ERD, endpoint plan, and SQL script.

Successful build screenshot:

[INSERT SCREENSHOT HERE]

## Video Walkthrough

Unlisted YouTube video explaining the planning documents, ERD decisions, endpoint plan choices, and a live run of the SQL script in SSMS:

[INSERT YOUTUBE LINK HERE]
