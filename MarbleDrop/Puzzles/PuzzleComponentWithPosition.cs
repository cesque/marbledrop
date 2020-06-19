using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles
{
    public abstract class PuzzleComponentWithPosition : PuzzleComponent
    {
        public Vector2 Position;

        public PuzzleComponentWithPosition(Puzzle puzzle) : this(puzzle, Vector2.Zero) { }

        public PuzzleComponentWithPosition(Puzzle puzzle, string id) : this(puzzle, id, Vector2.Zero) { }

        public PuzzleComponentWithPosition(Puzzle puzzle, Vector2 position) : this(puzzle, new Guid().ToString(), position) { }
        
        public PuzzleComponentWithPosition(Puzzle puzzle, string id, Vector2 position) : base(puzzle, id)
        {
            Vector2 Position = position;
        }

        public static void PopulateFromJSON(PuzzleComponentWithPosition component, JsonElement element)
        {
            PuzzleComponent.PopulateFromJSON(component, element);

            var data = element.GetProperty("data");

            var position = data.GetProperty("position");

            var x = position.GetProperty("x").GetInt32();
            var y = position.GetProperty("y").GetInt32();

            component.Position = new Vector2(x, y);
        }
    }
}
