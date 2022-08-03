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
	class SwitchComponent : PuzzleComponentWithPosition
	{
		public List<(Marble marble, float age, int output)> Marbles;

		Vector2 marblePosition = new Vector2(1, 1);

		int activeOutput = 0;

		float timeToHoldMarble = 0.01f;

		List<List<GridCharacter>> layouts;

		public SwitchComponent(Puzzle puzzle, string id) : base(puzzle, id)
		{
			Position = Vector2.Zero;

			Marbles = new List<(Marble, float, int)>();
		}

		public SwitchComponent(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }

		public override void Initialise()
		{
			Inputs.Add(new ComponentPort(this, PortType.INPUT, ResourceType.MARBLE, new Vector2(-1, 1), "marble/input"));
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.MARBLE, new Vector2(0, -1), "marble/top"));
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.MARBLE, new Vector2(0, 3), "marble/bottom"));

			Inputs.Add(new ComponentPort(this, PortType.INPUT, ResourceType.SPARK, new Vector2(2, 1), "spark/input"));

			UpdateLayouts();
		}

		void UpdateLayouts()
		{
			layouts = new List<List<GridCharacter>>
			{
				new List<GridCharacter>
				{
					new GridCharacter(grid, 131, Position + new Vector2(0, 0), grid.Palette.Get("white"), grid.Palette.Get("darkgreen"), Priority.Component),
					new GridCharacter(grid, 24, Position + new Vector2(1, 0), grid.Palette.Get("green"), grid.Palette.Get("darkgreen"), Priority.Component),

					new GridCharacter(grid, 169, Position + new Vector2(0, 1), grid.Palette.Get("white"), grid.Palette.Get("darkgreen"), Priority.Component),
					new GridCharacter(grid, 0, Position + new Vector2(1, 1), grid.Palette.Get("white"), grid.Palette.Get("darkgreen"), Priority.Component),

					new GridCharacter(grid, 0, Position + new Vector2(0, 2), grid.Palette.Get("white"), grid.Palette.Get("darkgreen"), Priority.Component),
					new GridCharacter(grid, 0, Position + new Vector2(1, 2), grid.Palette.Get("white"), grid.Palette.Get("darkgreen"), Priority.Component),
				},

				new List<GridCharacter>
				{
					new GridCharacter(grid, 0, Position + new Vector2(0, 0), grid.Palette.Get("white"), grid.Palette.Get("darkblue"), Priority.Component),
					new GridCharacter(grid, 0, Position + new Vector2(1, 0), grid.Palette.Get("white"), grid.Palette.Get("darkblue"), Priority.Component),

					new GridCharacter(grid, 143, Position + new Vector2(0, 1), grid.Palette.Get("white"), grid.Palette.Get("darkblue"), Priority.Component),
					new GridCharacter(grid, 0, Position + new Vector2(1, 1), grid.Palette.Get("white"), grid.Palette.Get("darkblue"), Priority.Component),

					new GridCharacter(grid, 131, Position + new Vector2(0, 2), grid.Palette.Get("white"), grid.Palette.Get("darkblue"), Priority.Component),
					new GridCharacter(grid, 25, Position + new Vector2(1, 2), grid.Palette.Get("blue"), grid.Palette.Get("darkblue"), Priority.Component),
				}
			};
		}

		internal override void PositionChanged(Vector2 oldPosition, Vector2 newPosition)
		{
			base.PositionChanged(oldPosition, newPosition);
			UpdateLayouts();
		}

		public override void Update(GameTime gameTime)
		{
			for (var i = 0; i < Marbles.Count; i++)
			{
				// there's gotta be a better way of doing this
				var tuple = Marbles[i];

				var newTuple = (marble: tuple.marble, age: tuple.age + (float)gameTime.ElapsedGameTime.TotalSeconds, output: tuple.output);
				Marbles[i] = newTuple;

				if (newTuple.age >= timeToHoldMarble)
				{
					newTuple.marble.DisableTrail();
					Output(Outputs[newTuple.output], newTuple.marble);

				}
			}

			Marbles = Marbles.Where(tuple => tuple.age < timeToHoldMarble).ToList();
		}

		public override List<GridCharacter> GetCharacters()
		{
			return layouts[activeOutput];
		}

		public override void Input(ComponentPort port, Resource resource)
		{
			Console.WriteLine(resource);
			if (resource.Type == ResourceType.MARBLE)
			{
				var marble = (Marble)resource;
				Marbles.Add((marble, 0f, activeOutput));

				Toggle();
			}
			else if (resource.Type == ResourceType.SPARK)
			{
				Toggle();
			}
		}

		public void Toggle()
		{
			activeOutput += 1;
			activeOutput = activeOutput % 2;
		}

		public static PuzzleComponent FromJSON(Puzzle puzzle, JsonElement element)
		{
			var component = new SwitchComponent(puzzle, element.GetProperty("id").GetString());

			PuzzleComponentWithPosition.PopulateFromJSON(component, element);

			return component;
		}
	}
}
