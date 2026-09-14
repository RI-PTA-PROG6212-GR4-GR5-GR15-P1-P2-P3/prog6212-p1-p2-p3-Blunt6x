# RaceDay API Endpoint Plan

This document defines the API endpoints planned for the RaceDay system.

Planned endpoints for the RaceDay system, derived from the ERD in `/docs/raceday_erd.png`. Two roles exist: **Organiser** (creates and manages events) and **Participant** (enrols in categories and views results). Endpoints marked **Public** require no authentication; **Any** means any authenticated user regardless of role; **Owner** means the acting user must own the resource (e.g. the Organiser who created the Event).

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Register a new user as either an Organiser or a Participant | Public | `{ fullName, email, password, role }` | 201 Created — `{ userId, email, role, token }` |
| POST | /api/auth/login | Authenticate a user and issue a session token | Public | `{ email, password }` | 200 OK — `{ token, userId, role }` |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Retrieve the logged-in user's profile (including role-specific fields) | Any | — | 200 OK — user profile object |
| PUT | /api/users/me | Update the logged-in user's profile details | Any | `{ fullName, contactNumber / dateOfBirth, emergencyContact }` | 200 OK — updated profile object |
| GET | /api/users/{id} | View a specific user's public profile (e.g. an Organiser viewing a Participant who enrolled) | Organiser | — | 200 OK — user profile object |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | List all events, with optional filters (date, location, status) | Public | — | 200 OK — array of events |
| GET | /api/events/{id} | Retrieve full details for a single event | Public | — | 200 OK — event object |
| POST | /api/events | Create a new event | Organiser | `{ eventName, eventDate, location, description }` | 201 Created — event object |
| PUT | /api/events/{id} | Update an existing event | Organiser (Owner) | `{ eventName, eventDate, location, description, status }` | 200 OK — updated event object |
| DELETE | /api/events/{id} | Cancel/delete an event | Organiser (Owner) | — | 204 No Content |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | List all categories (e.g. 5km, 10km) for an event | Public | — | 200 OK — array of categories |
| GET | /api/categories/{id} | Retrieve details for a single category | Public | — | 200 OK — category object |
| POST | /api/events/{eventId}/categories | Add a category to an event | Organiser (Owner) | `{ categoryName, distanceKm, maxParticipants, entryFee }` | 201 Created — category object |
| PUT | /api/categories/{id} | Update a category | Organiser (Owner) | `{ categoryName, distanceKm, maxParticipants, entryFee }` | 200 OK — updated category object |
| DELETE | /api/categories/{id} | Remove a category | Organiser (Owner) | — | 204 No Content |

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/categories/{categoryId}/enrolments | Enrol the logged-in participant into a category | Participant | `{ paymentStatus }` (optional) | 201 Created — enrolment object |
| GET | /api/users/me/enrolments | List the logged-in participant's own enrolments | Participant | — | 200 OK — array of enrolments |
| GET | /api/categories/{categoryId}/enrolments | List all enrolments for a category (roster) | Organiser (Owner) | — | 200 OK — array of enrolments |
| DELETE | /api/enrolments/{id} | Cancel an enrolment | Participant (Owner) | — | 204 No Content |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments/{id}/result | Capture a result for a completed enrolment | Organiser (Owner) | `{ finishTime, position, status }` | 201 Created — result object |
| PUT | /api/results/{id} | Correct/update an existing result | Organiser (Owner) | `{ finishTime, position, status }` | 200 OK — updated result object |
| GET | /api/categories/{categoryId}/results | View the results/leaderboard for a category | Public | — | 200 OK — array of results, sorted by position |
| GET | /api/users/me/results | View the logged-in participant's own results | Participant | — | 200 OK — array of results |
