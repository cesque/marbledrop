using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace MarbleDrop.Puzzles.Editor.Modes
{
	internal class EditPuzzleInfoMode : PuzzleEditorModeStrategy
	{
		public EditPuzzleInfoMode(PuzzleEditorContext context) : base(context) { }

		public override void Enter()
		{
			base.Enter();
		}

		public override void Leave()
		{
			base.Leave();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
		}

		public override void DrawUI()
		{
			base.DrawUI();

			var screen = puzzle.game.GraphicsDevice.Viewport.Bounds;
			var width = 300;
			ImGui.SetNextWindowPos(new System.Numerics.Vector2(screen.Right - width, 0));
			ImGui.SetNextWindowSize(new System.Numerics.Vector2(width, screen.Height));
			ImGui.Begin("PUZZLE INFO MODE");

			ImGui.InputText("Name", ref puzzle.Name, 32);

			ImGui.Dummy(new System.Numerics.Vector2(0, 40f));

			var windowContentRegion = ImGui.GetWindowContentRegionMax();
			var buttonText = "SAVE";
			var buttonSize = ImGui.CalcTextSize(buttonText);
			var buttonPadding = new Vector2(windowContentRegion.X - buttonSize.X - 8f, 20f);
			ImGui.PushStyleColor(ImGuiCol.Button, puzzle.grid.Palette.Get("darkgreen").ToVector4().ToNumerics());

			if (ImGui.Button(buttonText, new System.Numerics.Vector2(buttonSize.X + buttonPadding.X, buttonSize.Y + buttonPadding.Y)))
			{
				puzzle.Save();
			}

			ImGui.PopStyleColor();
			ImGui.End();
		}

		
	}
}
