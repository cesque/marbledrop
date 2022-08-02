using MarbleDrop.Puzzles.Resources;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Components
{
	class TestComponent : PuzzleComponentWithPosition
	{
		public TestComponent(Puzzle puzzle, string id) : base(puzzle, id)
		{
			Position = Vector2.Zero;
		}

		public TestComponent(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }

		public override void Initialise()
		{
			throw new NotImplementedException();
		}

		public override void Input(ComponentPort port, Resource resource)
		{
			throw new NotImplementedException();
		}

		public override void Update(GameTime gameTime)
		{
			throw new NotImplementedException();
		}

		public override List<GridCharacter> GetCharacters()
		{
			throw new NotImplementedException();
		}

		public static PuzzleComponent FromJSON(Puzzle puzzle, JsonElement element)
		{
			var component = new TestComponent(puzzle, element.GetProperty("id").GetString());

			PuzzleComponentWithPosition.PopulateFromJSON(component, element);

			/* --- handle custom deserialization here --- */
			var data = element.GetProperty("data");

			return component;
		}
	}
}
