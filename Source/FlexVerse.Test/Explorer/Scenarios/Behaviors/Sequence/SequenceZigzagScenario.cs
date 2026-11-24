// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>An ordinary local Sequence followed by a pure output mapping.</summary>
    public partial class SequenceZigzagScenario : Scenario
    {
        public static new string Description =>
            "An ordinary local Sequence feeds a helper function: rotation and uniform scale preserve the zigzag after every world retarget.";

        [State(ReconcileOnSet = true)]
        private Behavior Playback { get; set; } = NullBehavior.Instance;

        [State]
        private Behavior<SequenceScene> SceneModel { get; set; }

        private Vector2 _worldStart = new(2f, 4f);
        private Vector2 _worldTarget = new(14f, 8f);

        private SequenceBehavior<Vector2> LocalSequence => (SequenceBehavior<Vector2>)Playback;

        public override void OnMount()
        {
            base.OnMount();
            Playback = CreateLocalSequence();
            var scene = new SequenceScene();
            scene.Initialize(GetSequence, ProjectPoint, GetTarget, GetEntry, Retarget);
            SceneModel = scene;
        }

        private static SequenceBehavior<Vector2> CreateLocalSequence() =>
            new()
            {
                Value = Vector2.zero,
                Behaviors = new Behavior[]
                {
                    Animate.To(new Vector2(2f, 2f), 0.45f),
                    Animate.To(new Vector2(4f, -2f), 0.45f),
                    Animate.To(new Vector2(6f, 2f), 0.45f),
                    Animate.To(new Vector2(8f, -2f), 0.45f),
                    Animate.To(new Vector2(10f, 0f), 0.45f),
                },
            };

        private static Vector2 TransformPoint(Vector2 point, Vector2 start, Vector2 target)
        {
            if (point == Vector2.zero)
                return start;
            if (point == new Vector2(10f, 0f))
                return target;
            var axis = (target - start) / 10f;
            return start
                + new Vector2(
                    point.x * axis.x - point.y * axis.y,
                    point.x * axis.y + point.y * axis.x
                );
        }

        private Vector2 ProjectPoint(Vector2 point) =>
            TransformPoint(point, _worldStart, _worldTarget);

        private SequenceBehavior<Vector2> GetSequence() => LocalSequence;

        private Vector2 GetTarget() => _worldTarget;

        private Vector2 GetEntry() => _worldStart;

        private void Retarget(Vector2 target)
        {
            var current = ProjectPoint(LocalSequence.Value);
            _worldStart = current;
            _worldTarget = target;

            Playback = NullBehavior.Instance;
            Playback = CreateLocalSequence();
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            if (SceneModel.Value is null || Playback is not SequenceBehavior<Vector2>)
                return [];
            return
            [
                new SequencePanel
                {
                    SceneSource = SceneModel,
                    Kicker = "07  /  SEQUENCE → HELPER",
                    Title = "A zigzag stays a zigzag",
                    Instruction =
                        "Click the canvas. Local Sequence.Value passes through TransformPoint into world coordinates.",
                    Expected =
                        "The ordinary Sequence plays a fixed five-step zigzag. The helper rotates, uniformly scales and translates its output. A new run starts at the current world position without a jump.",
                    Failure =
                        "A straightened zigzag, a missed target, a position jump, or accumulated shape distortion. The local template and the existing Proportional strategy stay unchanged.",
                    Controls = new SequenceProgressBar { Scene = SceneModel },
                },
            ];
        }
    }
}
