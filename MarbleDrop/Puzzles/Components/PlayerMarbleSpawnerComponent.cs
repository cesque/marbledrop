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
	class PlayerMarbleSpawnerComponent : PuzzleComponentWithPosition
	{
		List<GridCharacter> layout = new List<GridCharacter>();

		public PlayerMarbleSpawnerComponent(Puzzle puzzle, string id) : base(puzzle, id)
		{
			Position = new Vector2(4, 4);

			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.MARBLE, this.Position + new Vector2(4, 2), "marble/output"));

		}

		public PlayerMarbleSpawnerComponent(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }

		public override void Initialise()
		{
			UpdateLayout();
		}

		public override void Update(GameTime gameTime)
		{
			if (game.inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Space))
			{
				var enumValues = Enum.GetValues(typeof(MarbleColor));
				var color = (MarbleColor)enumValues.GetValue(Globals.RNG.Next(enumValues.Length));
				var marble = new Marble(puzzle, color);
				Output("marble/output", marble);
			}
		}

		internal override void PositionChanged(Vector2 oldPosition, Vector2 newPosition)
		{
			base.PositionChanged(oldPosition, newPosition);
			UpdateLayout();
		}


		void UpdateLayout()
		{
			layout = new List<GridCharacter>()
			{
				new GridCharacter(grid, 170, Position + new Vector2(0, 0), Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(1, 0), Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(2, 0), Priority.Component),
				new GridCharacter(grid, 143, Position + new Vector2(3, 0), Priority.Component),

				new GridCharacter(grid, 131, Position + new Vector2(0, 1), Priority.Component),
				new GridCharacter(grid, 171, Position + new Vector2(1, 1), grid.Palette.Get("red"), Priority.Component),
				new GridCharacter(grid, 171, Position + new Vector2(2, 1), grid.Palette.Get("green"), Priority.Component),
				new GridCharacter(grid, 131, Position + new Vector2(3, 1), Priority.Component),

				new GridCharacter(grid, 131, Position + new Vector2(0, 2), Priority.Component),
				new GridCharacter(grid, 171, Position + new Vector2(1, 2), grid.Palette.Get("blue"), Priority.Component),
				new GridCharacter(grid, 171, Position + new Vector2(2, 2), grid.Palette.Get("white"), Priority.Component),
				new GridCharacter(grid, 131, Position + new Vector2(3, 2), Priority.Component),

				new GridCharacter(grid, 144, Position + new Vector2(0, 3), Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(1, 3), Priority.Component),
				new GridCharacter(grid, 148, Position + new Vector2(2, 3), Priority.Component),
				new GridCharacter(grid, 169, Position + new Vector2(3, 3), Priority.Component),
			};
		}

		public override List<GridCharacter> GetCharacters()
		{
			return layout;
		}

		public override void Input(ComponentPort port, Resource resource)
		{

		}

		public override void DrawEditorUI()
		{
			DrawEditorUI(false);
			if (!IsEditorSelected) return;

			ImGuiNET.ImGui.Text("Spawner");

			ImGuiNET.ImGui.End();
		}

		public static PuzzleComponent FromJSON(Puzzle puzzle, JsonElement element)
		{
			var component = new PlayerMarbleSpawnerComponent(puzzle, element.GetProperty("id").GetString());

			PuzzleComponentWithPosition.PopulateFromJSON(component, element);

			return component;
		}
	}
}
