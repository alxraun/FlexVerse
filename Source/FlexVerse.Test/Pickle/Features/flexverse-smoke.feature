# parents: @.frostline/graph/m-pickle.xml

Feature: flexverse smoke
  The first in-game verification slice for FlexVerse: the framework assembly
  loads in a live session, its verification log markers are emitted, generated
  widget code executes, and a behavior lifecycle behaves through the public API.

  Scenario: flexverse debug verification markers reach the game log
    When I wait 30 frames
    When I emit a flexverse verify marker "flexverse-smoke-load"
    Then the flexverse verify marker "flexverse-smoke-load" was recorded

  Scenario: generated widget property code executes in a live session
    Given a smoke widget with prop "Speed" set to 2.5
    Then the smoke widget prop "Speed" is 2.5

  Scenario: a static behavior lifecycle completes through the public API
    Given an active static behavior with duration 1.0
    When it updates for a total of 1.0 seconds
    Then it reports completion
