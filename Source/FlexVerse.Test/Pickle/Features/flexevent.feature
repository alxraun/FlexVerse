# parents: @.frostline/graph/m-pickle.xml

Feature: FlexVerse FlexEvent semantics
  Event delivery facts (bubbling, stop-propagation, hover enter/leave)
  verified programmatically through the public WidgetTree/FlexEvent API.

  Scenario: Mouse down dispatches at the target and bubbles to ancestors
    Given a bubbling probe tree parent-child-grandchild
    When a mouse down is dispatched at the grandchild
    Then the probe recorded fact "click:grandchild"
    And the probe recorded fact "click:child"
    And the probe recorded fact "click:parent"

  Scenario: StopPropagation contains the event at the target
    Given a stop-propagation probe tree parent-child-grandchild
    When a mouse down is dispatched at the grandchild
    Then the probe recorded fact "stop:grandchild"
    And the probe did not record fact "stop:child"
    And the probe did not record fact "stop:parent"

  Scenario: Hover fires enter and leave in order
    Given a hover probe widget
    When the pointer enters the hover probe
    Then the probe recorded fact "hover:enter"
    When the pointer leaves the hover probe
    Then the probe recorded fact "hover:leave"
