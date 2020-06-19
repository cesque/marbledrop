using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Resources
{
    class Marble : Resource
    {
        static Dictionary<MarbleColor, Color> colorMap;
        Vector2 lastFramePosition;
        Vector2 lastGridPosition;

        public MarbleColor Color;
        bool hideTrail;
        public Marble(Puzzle puzzle, MarbleColor color) : base(puzzle, ResourceType.MARBLE)
        {
            if (colorMap == null)
            {
                colorMap = new Dictionary<MarbleColor, Color>
                {
                    { MarbleColor.RED, puzzle.grid.Palette.Get("red") },
                    { MarbleColor.GREEN, puzzle.grid.Palette.Get("green") },
                    { MarbleColor.BLUE, puzzle.grid.Palette.Get("blue") },
                    { MarbleColor.WHITE, puzzle.grid.Palette.Get("white") },
                };
            }

            Color = color;
        }

        public Color GetColor()
        {
            return colorMap[Color];
        }

        public List<GridCharacter> GetCharacters(Vector2 position, int characterIndex)
        {
            var list = new List<GridCharacter>();

            list.Add(new GridCharacter(
                puzzle.grid,
                characterIndex,
                position,
                colorMap[Color],
                Priority.Marble

            ));

            if (!hideTrail)
            {
                if (Math.Round(position.X) != Math.Round(lastFramePosition.X) || Math.Round(position.Y) != Math.Round(lastFramePosition.Y))
                {
                    lastGridPosition = new Vector2(
                        (float)Math.Round(lastFramePosition.X),
                        (float)Math.Round(lastFramePosition.Y)
                    );
                }

                if (lastGridPosition != null)
                {
                    list.Add(new GridCharacter(
                        puzzle.grid,
                        characterIndex,
                        lastGridPosition,
                        colorMap[Color],
                        Priority.Marble
                    ));
                }
            }

            lastFramePosition = position;

            if (hideTrail)
            {
                lastGridPosition = position;
            }

            return list;
        }

        public override List<GridCharacter> GetCharacters(Vector2 position)
        {
            return GetCharacters(position, 188);
        }

        public void EnableTrail()
        {
            hideTrail = false;
        }
        public void DisableTrail()
        {
            hideTrail = true;
        }
    }

    enum MarbleColor
    {
        RED,
        GREEN,
        BLUE,
        WHITE,
    }
}
