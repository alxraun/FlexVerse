# parents: @.frostline/graph/m-pickle.xml

Feature: Optional widget debugger API integration
  The base run verifies the target without tooling; --with flexverse.tools
  additionally verifies inspection, event observation and the inspector UI.

  Scenario: Optional widget debugger preserves targets and observes live instances when loaded
    Then optional widget debugging preserves the live FlexVerse API
