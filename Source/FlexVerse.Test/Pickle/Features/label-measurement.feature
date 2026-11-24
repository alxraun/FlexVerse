# parents: @.frostline/graph/m-pickle.xml

Feature: Label content measurement
  Label measures content under the constraints supplied by Flexbox.

  Scenario: Exact and maximum constraints apply with or without wrapping and content
    Then Label measurement respects exact and maximum content constraints

  Scenario: Box sizing stays in Flexbox and text globals remain isolated
    Then Label measurement leaves box constraints to Flexbox and restores text globals
