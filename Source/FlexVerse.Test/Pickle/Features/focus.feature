# parents: @.frostline/graph/m-pickle.xml

Feature: FlexVerse focus semantics
  Keyboard focus and dispatch into a FlexVerse TextField hosted in a real
  RimWorld window: a focus click reaches the field through the public
  OnMouseDown hook, synthesized key events flow through the engine's render
  loop into the focused widget, and unfocused keys dispatch nowhere.

  Scenario: Clicking a text field focuses it and Enter reaches the widget
    Given a focus probe window with a text field
    When I click the text field
    And I press Enter
    Then the focus probe recorded fact "key:submit"
    And the focus probe window is still open

  Scenario: Escape reaches the focused widget without closing the window
    Given a focus probe window with a text field
    When I click the text field
    And I press Escape
    Then the focus probe recorded fact "key:cancel"
    And the focus probe window is still open

  Scenario: Keyboard input without focus is not dispatched to the widget
    Given a focus probe window with a text field
    When I press Enter
    Then the focus probe did not record fact "key:submit"
    And the focus probe window is still open
