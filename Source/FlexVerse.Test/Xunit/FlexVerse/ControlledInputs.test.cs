// parents: @.frostline/graph/m-xunit.xml

using System;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    public class ControlledInputsTests
    {
        [Fact]
        public void PublicApi_UsesSingleDisabledAndValueChangeContract()
        {
            Assert.NotNull(typeof(Button).GetProperty("Disabled"));
            Assert.Null(typeof(Button).GetProperty("Active"));
            Assert.NotNull(typeof(Checkbox).GetProperty("Checked"));
            Assert.Null(typeof(Checkbox).GetProperty("IsChecked"));

            foreach (
                var control in new[]
                {
                    typeof(Checkbox),
                    typeof(TextField),
                    typeof(HorizontalSlider),
                    typeof(AreaSlider),
                }
            )
            {
                Assert.NotNull(control.GetProperty("OnValueChange"));
                Assert.NotNull(control.GetProperty("Disabled"));
                Assert.Null(control.GetProperty("OnChanged"));
            }
        }

        [Fact]
        public void Checkbox_NotifiesOnceWithoutOverwritingAuthoritativeChecked()
        {
            int calls = 0;
            bool requested = false;
            var checkbox = new Checkbox
            {
                Checked = false,
                OnValueChange =
                    (Action<bool>)(
                        v =>
                        {
                            calls++;
                            requested = v;
                        }
                    ),
            };
            checkbox.Checked.Activate();
            checkbox.Disabled.Activate();
            checkbox.OnValueChange.Activate();

            checkbox.ProposeChecked(true);
            checkbox.ProposeChecked(false);

            Assert.Equal(1, calls);
            Assert.True(requested);
            Assert.False(checkbox.Checked.Value);
        }

        [Fact]
        public void TextField_ValidatorVetoAndDisabledKeepExternalValue()
        {
            int calls = 0;
            string? requested = null;
            var field = new TextField
            {
                Value = "ok",
                Validator = (Func<string, bool>)(v => v.Length <= 3),
                OnValueChange =
                    (Action<string>)(
                        v =>
                        {
                            calls++;
                            requested = v;
                        }
                    ),
            };
            field.Value.Activate();
            field.Validator.Activate();
            field.OnValueChange.Activate();
            field.Disabled.Activate();

            field.ProposeValue("toolong");
            field.ProposeValue("ok");
            field.ProposeValue("new");

            Assert.Equal(1, calls);
            Assert.Equal("new", requested);
            Assert.Equal("ok", field.Value.Value);

            field.Disabled = true;
            field.Disabled.Activate();
            field.ProposeValue("yes");
            Assert.Equal(1, calls);
        }

        [Fact]
        public void HorizontalSlider_OnlyNotifiesChangedEnabledProposals()
        {
            int calls = 0;
            float requested = 0f;
            var slider = new HorizontalSlider
            {
                Disabled = false,
                Value = 0.25f,
                OnValueChange =
                    (Action<float>)(
                        v =>
                        {
                            calls++;
                            requested = v;
                        }
                    ),
            };
            slider.Value.Activate();
            slider.Disabled.Activate();
            slider.OnValueChange.Activate();

            slider.ProposeValue(0.25f);
            slider.ProposeValue(0.75f);
            Assert.Equal(1, calls);
            Assert.Equal(0.75f, requested);
            Assert.Equal(0.25f, slider.Value.Value);

            slider.Disabled = true;
            slider.Disabled.Activate();
            slider.ProposeValue(0.9f);
            Assert.Equal(1, calls);
        }

        [Fact]
        public void TwoDimensionalSlider_ChangesNeitherControlledValueNorWhenDisabled()
        {
            int calls = 0;
            Vector2 requested = Vector2.zero;
            var slider = new AreaSlider
            {
                Value = new Vector2(0.25f, 0.5f),
                OnValueChange =
                    (Action<Vector2>)(
                        v =>
                        {
                            calls++;
                            requested = v;
                        }
                    ),
            };
            slider.Value.Activate();
            slider.Disabled.Activate();
            slider.OnValueChange.Activate();

            slider.ProposeValue(new Vector2(0.25f, 0.5f));
            slider.ProposeValue(new Vector2(0.8f, 0.2f));
            Assert.Equal(1, calls);
            Assert.Equal(new Vector2(0.8f, 0.2f), requested);
            Assert.Equal(new Vector2(0.25f, 0.5f), slider.Value.Value);

            slider.Disabled = true;
            slider.Disabled.Activate();
            slider.ProposeValue(Vector2.one);
            Assert.Equal(1, calls);
        }

        [Fact]
        public void DisabledTextField_DoesNotSubmitOrCancel()
        {
            int calls = 0;
            var field = new TextField
            {
                Disabled = true,
                OnSubmit = (Action)(() => calls++),
                OnCancel = (Action)(() => calls++),
            };
            field.Disabled.Activate();
            field.OnSubmit.Activate();
            field.OnCancel.Activate();

            field.OnKeyDown(
                new FlexEvent(
                    new Event { type = EventType.KeyDown, keyCode = KeyCode.Return },
                    field
                )
            );
            field.OnKeyDown(
                new FlexEvent(
                    new Event { type = EventType.KeyDown, keyCode = KeyCode.Escape },
                    field
                )
            );
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Button_DisabledPreventsOnMouseUpAction()
        {
            int calls = 0;
            var button = new Button { Disabled = true, OnClick = (Action)(() => calls++) };
            button.Disabled.Activate();
            button.OnClick.Activate();

            button.OnMouseUp(
                new FlexEvent(new Event { type = EventType.MouseUp, button = 0 }, button)
            );
            Assert.Equal(0, calls);

            button.Disabled = false;
            button.Disabled.Activate();
            button.OnMouseUp(
                new FlexEvent(new Event { type = EventType.MouseUp, button = 0 }, button)
            );
            Assert.Equal(1, calls);
        }
    }
}
