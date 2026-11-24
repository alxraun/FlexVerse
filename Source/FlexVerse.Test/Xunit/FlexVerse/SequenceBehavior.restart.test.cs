// parents: @.frostline/graph/m-xunit.xml

using Xunit;

namespace FlexVerse.Test
{
    public class SequenceRestartDiagnosticsTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Retarget_AfterCompletion_RedrivesFromLiveValue(bool restart)
        {
            var first = Animate.To(4f, 0.5f);
            var middle = Animate.To(6f, 0.5f);
            var last = Animate.To(13f, 0.5f);
            var sequence = new SequenceBehavior<float>(first, middle, last);
            int firstCompletions = 0;
            int lastActivations = 0;
            first.OnComplete += () => firstCompletions++;
            last.OnActivate += () => lastActivations++;
            sequence.Activate();
            for (int i = 0; i < 200 && sequence.IsUpdating; i++)
                sequence.Update(0.02f);
            Assert.False(sequence.IsUpdating);
            Assert.Equal(13f, sequence.Value);

            sequence.Strategy = new SequenceStrategy { Elasticity = 1f, Restart = restart };
            sequence.Target = 20f;
            Assert.Equal(13f, sequence.Value);
            Assert.True(sequence.IsUpdating);
            sequence.Update(0.02f);
            Assert.True(sequence.Value > 13f, $"Value={sequence.Value}; last.Target={last.Target}");
            for (int i = 0; i < 200 && sequence.IsUpdating; i++)
                sequence.Update(0.02f);
            Assert.Equal(20f, sequence.Value);
            Assert.Equal(restart ? 2 : 1, firstCompletions);
            Assert.Equal(1, lastActivations);

            sequence.Target = 25f;
            Assert.True(sequence.IsUpdating);
            for (int i = 0; i < 200 && sequence.IsUpdating; i++)
                sequence.Update(0.02f);
            Assert.Equal(25f, sequence.Value);
            Assert.Equal(restart ? 3 : 1, firstCompletions);
            Assert.Equal(1, lastActivations);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Retarget_InitiallyCollapsedChain_RedrivesFromLiveValue(bool restart)
        {
            var sequence = new SequenceBehavior<float>(
                Animate.To(4f, 0.5f),
                Animate.To(6f, 0.5f),
                Animate.To(13f, 0.5f)
            )
            {
                Target = 0f,
                Value = 0f,
            };
            sequence.Activate();
            Assert.False(sequence.IsUpdating);
            sequence.Strategy = new SequenceStrategy { Elasticity = 1f, Restart = restart };
            sequence.Target = 20f;
            Assert.True(sequence.IsUpdating);
            sequence.Update(0.02f);
            Assert.True(sequence.Value > 0f);
        }

        [Fact]
        public void Retarget_MidFlightWithoutRestart_MovesWithoutReplayingEarlierChildren()
        {
            var first = Animate.To(4f, 0.5f);
            var middle = Animate.To(6f, 0.5f);
            var last = Animate.To(13f, 0.5f);
            var sequence = new SequenceBehavior<float>(first, middle, last);
            int firstCompletions = 0;
            first.OnComplete += () => firstCompletions++;
            sequence.Activate();
            for (int i = 0; i < 35; i++)
                sequence.Update(0.02f);
            Assert.Equal(1, firstCompletions);
            float before = sequence.Value;
            sequence.Strategy = new SequenceStrategy { Elasticity = 1f, Restart = false };
            sequence.Target = 20f;
            Assert.Equal(before, sequence.Value);
            sequence.Update(0.02f);
            Assert.True(sequence.Value > before);
            for (int i = 0; i < 200 && sequence.IsUpdating; i++)
                sequence.Update(0.02f);
            Assert.Equal(20f, sequence.Value);
            Assert.Equal(1, firstCompletions);
        }
    }
}
