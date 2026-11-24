# parents: @.frostline/graph/m-pickle.xml

Feature: FlexVerse TweenBehavior semantics
  Duration-based interpolation facts (linear progression, exact completion,
  easing application, mid-flight retarget) verified programmatically through
  the public TweenBehavior API in a live RimWorld session — the game plane
  companion to the offline core suite.

  Scenario: A tween moves linearly toward its target under frame-sized updates
    Given an active tween behavior from 0.0 to 10.0 over 2.0 seconds
    When it advances for a total of 1.0 seconds in 0.1 second steps
    Then its value is 5.0 within 0.001

  Scenario: A tween reaches its target exactly and reports completion once
    Given an active tween behavior from 0.0 to 10.0 over 2.0 seconds
    When it advances for a total of 2.0 seconds in 0.1 second steps
    Then its value is 10.0 within 0.001
    And it reports completion exactly once

  Scenario: An easing curve shapes the interpolated value in-game
    Given an active quad-ease-in tween behavior from 0.0 to 10.0 over 2.0 seconds
    When it advances for a total of 1.0 seconds in 0.1 second steps
    Then its value is 2.5 within 0.001

  Scenario: Retargeting mid-flight continues from the current value
    Given an active tween behavior from 0.0 to 10.0 over 2.0 seconds
    When it advances for a total of 1.0 seconds in 0.1 second steps
    And its target changes to 20.0
    And it advances for 0.2 seconds in 0.1 second steps
    Then its value is 6.5 within 0.001
    And it does not report completion yet

  Scenario: The live frame loop drives a subscribed tween to completion
    Given an active tween behavior from 0.0 to 3.0 over 1.0 second subscribed to the behavior updater
    When I wait for its completion within 300 frames
    Then it reports completion exactly once
    And its value is 3.0 within 0.001
