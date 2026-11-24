// parents: @.frostline/graph/m-xunit.xml

using Xunit;

namespace FlexVerse.Test
{
    public class KeyframeBehaviorTests
    {
        [Fact]
        public void Target_ChangesOnlyTerminalValue_WhileActive()
        {
            var first = new Keyframe<float> { Time = 0.5f, Value = 10f };
            var last = new Keyframe<float>
            {
                Time = 1f,
                Value = 20f,
                Easing = Easing.Linear,
            };
            var behavior = new KeyframeBehavior<float>
            {
                Value = 3f,
                Keyframes = new[] { first, last },
            };
            behavior.Activate();

            behavior.Target = 40f;

            Assert.Equal(3f, behavior.Value);
            Assert.True(behavior.IsUpdating);
            Assert.Equal(first, behavior.Keyframes[0]);
            Assert.Equal(last.Time, behavior.Keyframes[1].Time);
            Assert.Equal(last.Easing, behavior.Keyframes[1].Easing);
            Assert.Equal(40f, behavior.Keyframes[1].Value);
            Assert.Equal(20f, last.Value);
        }

        [Fact]
        public void Target_BeforeFrames_IsStoredAndAppliedWhenConfigured()
        {
            var behavior = new KeyframeBehavior<float> { Target = 40f };
            Assert.Equal(40f, behavior.Target);
            Assert.Empty(behavior.Keyframes);
            Assert.False(behavior.IsUpdating);

            behavior.Keyframes = new[]
            {
                new Keyframe<float> { Time = 1f, Value = 20f },
            };

            Assert.Equal(40f, behavior.Keyframes[0].Value);
        }

        [Fact]
        public void Target_WithoutExplicitAssignment_FollowsLastFrame()
        {
            var behavior = new KeyframeBehavior<float>
            {
                Keyframes = new[]
                {
                    new Keyframe<float> { Time = 1f, Value = 20f },
                },
            };
            Assert.Equal(20f, behavior.Target);
            behavior.Target = 30f;
            Assert.Equal(30f, behavior.Target);
            Assert.False(behavior.IsUpdating);
        }
    }
}
