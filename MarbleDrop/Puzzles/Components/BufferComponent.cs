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
	class BufferComponent : PuzzleComponentWithPosition
	{
		public Queue<Marble> Marbles;
		public Queue<Marble> OutputQueue;

		float outputTimer = 0f;
		bool isOutputting = false;

		List<GridCharacter> layout;

		Color backgroundColor;
		Color foregroundColor;

		public BufferComponent(Puzzle puzzle, string id) : base(puzzle, id)
		{
			Position = Vector2.Zero;

			Marbles = new Queue<Marble>();
			OutputQueue = new Queue<Marble>();

			foregroundColor = grid.Palette.Get("white");
			backgroundColor = grid.Palette.Get("darkgrey");
		}

		public BufferComponent(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }


		public override void Initialise()
		{
			Inputs.Add(new ComponentPort(this, PortType.INPUT, ResourceType.MARBLE, this.Position + new Vector2(-1, 1), "marble/input"));
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.MARBLE, this.Position + new Vector2(5, 1), "marble/output"));

			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.SPARK, this.Position + new Vector2(2, -1), "spark/top"));
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.SPARK, this.Position + new Vector2(2, 3), "spark/bottom"));

			layout = new List<GridCharacter>()
			{
				new GridCharacter(grid, 170, Position + new Vector2(0, 0), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(1, 0), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(2, 0), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(3, 0), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 143, Position + new Vector2(4, 0), foregroundColor, backgroundColor, Priority.Component),

                //new GridCharacter(grid, 131, Position + new Vector2(0, 1), Priority.Component),
                //new GridCharacter(grid, 131, Position + new Vector2(4, 1), Priority.Component),
                new GridCharacter(grid, 132, Position + new Vector2(0, 1), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 128, Position + new Vector2(1, 1), grid.Palette.Get("grey"), backgroundColor, Priority.Component),
				new GridCharacter(grid, 128, Position + new Vector2(2, 1), grid.Palette.Get("grey"), backgroundColor, Priority.Component),
				new GridCharacter(grid, 128, Position + new Vector2(3, 1), grid.Palette.Get("grey"), backgroundColor, Priority.Component),
				new GridCharacter(grid, 147, Position + new Vector2(4, 1), foregroundColor, backgroundColor, Priority.Component),

				new GridCharacter(grid, 144, Position + new Vector2(0, 2), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(1, 2), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(2, 2), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(3, 2), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 169, Position + new Vector2(4, 2), foregroundColor, backgroundColor, Priority.Component),
			};
		}

		public override void Update(GameTime gameTime)
		{
			//Console.WriteLine(isOutputting + " - " + outputTimer);
			if (isOutputting)
			{
				outputTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

				if (outputTimer <= 0f)
				{
					outputTimer = 0.25f;

					var marble = OutputQueue.Dequeue();
					marble.DisableTrail();

					Output("marble/output", marble);

					if (OutputQueue.Count == 0)
					{
						isOutputting = false;
					}
				}
			}
		}

		public override List<GridCharacter> GetCharacters()
		{
			var characters = new List<GridCharacter>();

			characters.AddRange(layout);

			for (var i = 0; i < 3; i++)
			{
				if (isOutputting ? OutputQueue.Count > i : Marbles.Count > i)
				{
					var marble = isOutputting ? OutputQueue.ElementAt(i) : Marbles.ElementAt(i);
					marble.DisableTrail();
					var position = new Vector2(Position.X + 3 - i, Position.Y + 1);

					var marbleCharacters = marble.GetCharacters(position);

					marbleCharacters.First().BackgroundColor = backgroundColor;

					characters.AddRange(marbleCharacters);
				}
			}

			return characters;
		}

		public override void Input(ComponentPort port, Resource resource)
		{
			if (resource.Type == ResourceType.MARBLE)
			{
				var marble = (Marble)resource;
				if (!isOutputting)
				{
					Marbles.Enqueue(marble);

					if (Marbles.Count == 3)
					{
						while (Marbles.Count > 0) OutputQueue.Enqueue(Marbles.Dequeue());

						// start outputting
						isOutputting = true;
						outputTimer = 0.1f;

						Output("spark/bottom", new Spark(puzzle));
						Output("spark/top", new Spark(puzzle));
					}
				}
			}
		}

		public static PuzzleComponent FromJSON(Puzzle puzzle, JsonElement element)
		{
			var component = new BufferComponent(puzzle, element.GetProperty("id").GetString());

			PuzzleComponentWithPosition.PopulateFromJSON(component, element);

			return component;
		}
	}
}
