// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public sealed class UnblockBlock(
        string id,
        int x,
        int y,
        int width,
        int height,
        int shadeIndex,
        params (int Beat, int X, int Y)[] changes
    )
    {
        public string Id { get; } = id;
        public Color Color { get; } = UnblockVisuals.Shade(id, shadeIndex);
        public int Width { get; } = width;
        public int Height { get; } = height;
        public bool IsHero => Id == "hero-red";
        public bool IsHorizontal => Width >= Height;

        public Vector2 At(int beat)
        {
            var position = new Vector2(x, y);
            foreach (var change in changes)
            {
                if (change.Beat > beat)
                    break;
                position = new Vector2(change.X, change.Y);
            }
            return position;
        }
    }

    internal static class UnblockChoreography
    {
        public const int Columns = 18;
        public const int Rows = 10;
        public const int Beats = 12;
        public const int ExitRow = 4;

        public static readonly IReadOnlyList<UnblockBlock> Blocks = new UnblockBlock[]
        {
            new(
                "hero-red",
                1,
                4,
                2,
                1,
                0,
                (2, 4, 4),
                (5, 7, 4),
                (7, 10, 4),
                (9, 13, 4),
                (10, 16, 4),
                (11, 19, 4)
            ),
            new("v_col0_top", 0, 0, 1, 3, 1),
            new("v_col0_mid", 0, 3, 1, 3, 2),
            new("v_col0_bot", 0, 6, 1, 4, 3),
            new("h_l0", 1, 0, 2, 1, 4),
            new("h_l1", 1, 1, 2, 1, 5),
            new("v_l3", 1, 2, 1, 2, 6, (3, 1, 3), (11, 1, 2)),
            new("v_l4", 2, 2, 1, 2, 7, (3, 2, 3), (11, 2, 2)),
            new("h_l2", 1, 5, 2, 1, 8),
            new("v_l5", 1, 6, 1, 3, 9),
            new("v_l6", 2, 6, 1, 3, 10),
            new("h_l3", 1, 9, 2, 1, 11),
            new("h_z1_0", 3, 0, 3, 1, 12),
            new("h_z1_1", 4, 1, 2, 1, 13),
            new("v_z1_2", 5, 2, 1, 2, 14),
            new("v_z1_m", 4, 2, 1, 2, 15, (5, 4, 3), (11, 4, 2)),
            new("v_b1", 3, 3, 1, 3, 16, (1, 3, 1)),
            new("h_z1_b0", 4, 5, 2, 1, 17),
            new("v_z1_b1", 3, 6, 1, 3, 18),
            new("v_z1_b2", 4, 6, 1, 3, 19),
            new("v_z1_b3", 5, 6, 1, 3, 20),
            new("h_z1_b4", 3, 9, 3, 1, 21),
            new("h_z2_0", 6, 0, 3, 1, 22),
            new("v_z2_1", 6, 1, 1, 2, 23),
            new("h_z2_2", 7, 1, 2, 1, 24),
            new("v_z2_3", 8, 2, 1, 2, 25),
            new("h_z2_4", 7, 3, 1, 1, 26),
            new("v_b2", 6, 3, 1, 3, 27, (4, 6, 5)),
            new("h_z2_b0", 7, 5, 2, 1, 28),
            new("v_z2_b1", 7, 6, 1, 3, 29),
            new("v_z2_b2", 8, 6, 1, 3, 30),
            new("h_z2_b3", 6, 8, 1, 1, 31),
            new("h_z2_b4", 6, 9, 3, 1, 32),
            new("h_z3_0", 9, 0, 3, 1, 33),
            new("h_z3_1", 10, 1, 2, 1, 34),
            new("v_z3_2", 11, 2, 1, 2, 35),
            new("v_z3_m", 10, 2, 1, 2, 36),
            new("v_b3", 9, 3, 1, 3, 37, (6, 9, 1)),
            new("h_z3_b0", 10, 5, 2, 1, 38),
            new("v_z3_b1", 9, 6, 1, 3, 39),
            new("v_z3_b2", 10, 6, 1, 3, 40),
            new("v_z3_b3", 11, 6, 1, 3, 41),
            new("h_z3_b4", 9, 9, 3, 1, 42),
            new("h_z4_0", 12, 0, 3, 1, 43),
            new("v_z4_1", 12, 1, 1, 2, 44),
            new("h_z4_2", 13, 1, 2, 1, 45),
            new("v_z4_3", 14, 2, 1, 2, 46),
            new("h_z4_4", 13, 3, 1, 1, 47),
            new("v_b4", 12, 3, 1, 3, 48, (8, 12, 5)),
            new("h_z4_b0", 13, 5, 2, 1, 49),
            new("v_z4_b1", 13, 6, 1, 3, 50),
            new("v_z4_b2", 14, 6, 1, 3, 51),
            new("h_z4_b3", 12, 8, 1, 1, 52),
            new("h_z4_b4", 12, 9, 3, 1, 53),
            new("h_z5_0", 15, 0, 3, 1, 54),
            new("h_z5_1", 16, 1, 2, 1, 55),
            new("v_z5_2", 16, 2, 1, 2, 56),
            new("v_b5", 15, 3, 1, 3, 57, (9, 15, 1)),
            new("v_b6", 17, 4, 1, 2, 58, (10, 17, 2)),
            new("h_z5_b0", 15, 6, 2, 1, 59),
            new("v_z5_b1", 17, 6, 1, 3, 60),
            new("v_z5_b2", 15, 7, 1, 2, 61),
            new("v_z5_b3", 16, 7, 1, 2, 62),
            new("h_z5_b4", 15, 9, 3, 1, 63),
        };
    }
}
