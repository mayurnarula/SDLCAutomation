@feature @wm @WM-9 @WM-10 @WM-11 @WM-12
Feature: Worker Management CRUD and error scenarios
  In order to manage worker records via the Worker Management API
  As an API consumer and test automation suite
  I want BDD scenarios that validate create, read, update and delete operations plus common error conditions

  # Mapping:
  # WM-9: Create a worker through the REST API
  # WM-10: Retrieve workers through the REST API
  # WM-11: Update an existing worker through the REST API
  # WM-12: Delete a worker through the REST API

  @smoke @create
  Scenario: Create a new worker with valid payload (happy path)  # covers WM-9 AC: required fields, persistence, retrieval
    Given the system has no worker with identifier "WKR-123"
    When a client POSTs a valid worker payload with identifier "WKR-123", name "Alice Example", role "Engineer" and email "alice@example.com"
    Then the API responds with HTTP 201 Created
    And the response body contains the worker identifier "WKR-123"
    And subsequently a GET for worker "WKR-123" returns HTTP 200 and the worker data matches the created values

  @regression @create_negative
  Scenario Outline: Create worker fails when required fields are missing or invalid  # covers WM-9 AC: validation rules
    Given the payload contains: identifier: "<identifier>", name: "<name>", role: "<role>", email: "<email>"
    When the client attempts to POST the payload to /workers
    Then the API responds with HTTP 400 Bad Request
    And the response contains validation error for "<failingField>"

    Examples:
      | identifier  | name           | role     | email             | failingField |
      | ""         | "Alice"       | "Eng"   | "alice@example.com" | identifier    |
      | "WKR-124"  | ""            | "Eng"   | "alice@example.com" | name          |
      | "WKR-125"  | "Bob"         | ""      | "bob@example.com"   | role          |
      | "WKR-126"  | "Carol"       | "Eng"   | "not-an-email"      | email         |

  @regression @create_negative @duplicate
  Scenario: Create worker with duplicate identifier is rejected  # covers WM-9 AC: duplicate identifiers
    Given a worker with identifier "WKR-200" already exists
    When the client POSTs a valid worker payload with identifier "WKR-200"
    Then the API responds with HTTP 409 Conflict
    And the response indicates duplicate identifier

  @regression @retrieve
  Scenario: Retrieve collection of workers returns stored workers  # covers WM-10 AC: collection retrieval
    Given the system contains workers with identifiers "WKR-300","WKR-301"
    When the client requests GET /workers
    Then the API responds with HTTP 200 OK
    And the response is a JSON array containing workers with identifiers "WKR-300" and "WKR-301"

  @regression @retrieve @notfound
  Scenario Outline: Retrieve single worker by id - not found behavior  # covers WM-10 AC: not-found behavior
    Given no worker exists with identifier "<id>"
    When the client requests GET /workers/<id>
    Then the API responds with HTTP 404 Not Found
    And the response contains an error message indicating worker not found

    Examples:
      | id         |
      | "WKR-404" |
      | "UNKNOWN" |

  @regression @update
  Scenario: Update an existing worker's mutable fields successfully  # covers WM-11 AC: valid updates, data preservation
    Given a worker exists with identifier "WKR-400" and name "Old Name" and role "Technician"
    When the client PUTs an update payload for "WKR-400" changing name to "New Name" and role to "Senior Technician"
    Then the API responds with HTTP 200 OK
    And a subsequent GET /workers/WKR-400 returns the updated name "New Name" and role "Senior Technician"
    And unchanged fields (for example identifier and created timestamp) remain preserved

  @regression @update_negative
  Scenario: Updating a non-existent worker returns 404  # covers WM-11 AC: not-found behavior
    Given no worker exists with identifier "WKR-999"
    When the client attempts to PUT an update for worker "WKR-999"
    Then the API responds with HTTP 404 Not Found

  @regression @delete
  Scenario: Delete an existing worker and verify subsequent not-found  # covers WM-12 AC: deletion and subsequent not-found behavior
    Given a worker exists with identifier "WKR-500"
    When the client DELETEs /workers/WKR-500
    Then the API responds with HTTP 204 No Content
    And a subsequent GET /workers/WKR-500 responds with HTTP 404 Not Found

  @regression @delete_negative
  Scenario: Deleting a non-existent worker returns appropriate status  # covers WM-12 AC: nonexistent-worker handling
    Given no worker exists with identifier "WKR-501"
    When the client DELETEs /workers/WKR-501
    Then the API responds with HTTP 404 Not Found

  @edge @persistence
  Scenario: Persistence preserves existing records across restarts  # covers WM-5 related AC (persistence) and ties into overall CRUD
    Given the system has workers persisted including identifier "WKR-600"
    When the application restarts (or persistence is reloaded)
    Then a GET /workers/WKR-600 returns HTTP 200 and the worker data is unchanged

  @security @errors
  Scenario: API returns consistent server error structure for unexpected failures  # covers WM-7 AC: consistent server errors
    Given an unexpected server-side exception occurs during a request
    When the client performs any request that triggers the exception
    Then the API responds with HTTP 500 Internal Server Error
    And the response body contains a consistent error structure without leaking sensitive details
