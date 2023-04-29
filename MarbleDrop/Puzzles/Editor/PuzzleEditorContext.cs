using ImGuiNET;
using MarbleDrop.Puzzles.Editor;
using MarbleDrop.Puzzles.Editor.Modes;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles
{
	public class PuzzleEditorContext
	{
		internal PuzzleDisplay display;
		internal Puzzle puzzle => display.puzzle;


		Dictionary<PuzzleEditorMode, PuzzleEditorModeStrategy> modes;
		PuzzleEditorModeStrategy currentMode;

		public PuzzleEditorMode Mode
		{
			get => mode;
			set
			{
				// changing to same mode, do nothing
				if (currentMode != null && value == mode) return;

				mode = value;

				// if changing from another mode, leave that one and enter new one
				if (currentMode != null) currentMode.Leave();
				currentMode = modes[value];
				currentMode.Enter();
			}
		}
		PuzzleEditorMode mode;

		public bool Enabled = true;

		public PuzzleEditorContext(PuzzleDisplay display)
		{
			this.display = display;

			modes = new Dictionary<PuzzleEditorMode, PuzzleEditorModeStrategy>
			{
				{ PuzzleEditorMode.SELECT, new SelectMode(this) },
				{ PuzzleEditorMode.EDITWIRE, new EditWireMode(this) },
			};

			Mode = PuzzleEditorMode.SELECT;
		}

		public void Update(GameTime gameTime)
		{
			currentMode.Update(gameTime);

			if (puzzle.game.inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.S))
			{
				Mode = PuzzleEditorMode.SELECT;
			}
			else if (puzzle.game.inputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.W))
			{
				Mode = PuzzleEditorMode.EDITWIRE;
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			currentMode.Draw(spriteBatch);
		}

		public void DrawCharacters(Grid grid)
		{
			var x = 1;
			var y = puzzle.grid.Height - 1;

			var modeString = Mode.ToString();

			var indices = BitmapFont.ConvertStringToIndices($" MODE: { modeString } ");

			var modeColors = new Dictionary<Editor.PuzzleEditorMode, string>()
			{
				{ Editor.PuzzleEditorMode.SELECT, "green" },
				{ Editor.PuzzleEditorMode.EDITWIRE, "yellow" },
			};

			var foregroundColor = grid.Palette.Get("black");
			var backgroundColor = grid.Palette.Get(modeColors[Mode]);

			foreach (var index in indices)
			{
				var character = new GridCharacter(grid, index, new Vector2(x, y), foregroundColor, backgroundColor, 1000000);
				var result = grid.TryAddCharacter(character);
				x++;
			}
		}

		public void DrawUI()
		{
			currentMode.DrawUI();
			//foreach (var component in puzzle.Components)
			//{
			//	component.DrawEditorUI(puzzle.display);
			//}
		}
	}
}
