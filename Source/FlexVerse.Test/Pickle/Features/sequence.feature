# parents: @.frostline/graph/m-pickle.xml

Feature: FlexVerse SequenceBehavior semantics
  Ordered chain facts (strict child ordering, corridor retargeting, restart
  replay, rapid retargets, same-target no-op, idle restart, abandoned-child
  convergence) verified programmatically through the public SequenceBehavior
  API in a live RimWorld session — the game plane companion to the offline
  core suite.

  Scenario: A cold chain plays children strictly in order and completes once
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 1.5 seconds in 0.05 second steps
    Then the sequence value is 6.0 within 0.001
    And the sequence reports completion exactly once
    And the children played strictly in order

  Scenario: Retargeting mid second child replays from the first child and reaches the new target
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 0.75 seconds in 0.05 second steps
    And the sequence target changes to 9.0
    And the sequence advances for a total of 2.0 seconds in 0.05 second steps
    Then the sequence value is 9.0 within 0.001
    And the sequence reports completion exactly once

  Scenario: Retargeting during the last child still reaches the new target
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 1.25 seconds in 0.05 second steps
    And the sequence target changes to 3.0
    And the sequence advances for a total of 2.5 seconds in 0.05 second steps
    Then the sequence value is 3.0 within 0.001
    And the sequence reports completion exactly once

  Scenario: Two rapid retargets converge to the last target without stalling
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 0.6 seconds in 0.05 second steps
    And the sequence target changes to 9.0
    And the sequence advances for a total of 0.1 seconds in 0.05 second steps
    And the sequence target changes to 1.0
    And the sequence advances for a total of 3.0 seconds in 0.05 second steps
    Then the sequence value is 1.0 within 0.001
    And the sequence reports completion exactly once

  Scenario: Retargeting to the current target is a no-op
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 0.3 seconds in 0.05 second steps
    And the sequence target changes to 6.0
    And the sequence advances for a total of 2.0 seconds in 0.05 second steps
    Then the sequence value is 6.0 within 0.001
    And the children completed exactly 1, 1 and 1 times

  Scenario: Retargeting after completion starts a second run
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 1.5 seconds in 0.05 second steps
    And the sequence advances for a total of 0.1 seconds in 0.05 second steps
    And the sequence target changes to 9.0
    And the sequence advances for a total of 2.0 seconds in 0.05 second steps
    Then the sequence value is 9.0 within 0.001
    And the sequence reports completion exactly twice

  Scenario: An abandoned mid-run child completes exactly once more during the replay
    Given an active sequence of 3 tweens toward 2.0, 4.0 and 6.0 each 0.5 seconds
    When the sequence advances for a total of 0.75 seconds in 0.05 second steps
    And the sequence target changes to 9.0
    And the sequence advances for a total of 2.0 seconds in 0.05 second steps
    Then the children completed exactly 2, 1 and 1 times
    And the sequence reports completion exactly once
