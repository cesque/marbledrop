using ImGuiNET;
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
		PuzzleDisplay display;
		Puzzle puzzle => display.puzzle;

		public bool Enabled = true;
		public PuzzleComponent SelectedComponent;

		// properties for dragging a component
		public PuzzleComponentWithPosition DraggedComponent;
		public bool IsDragging => DraggedComponent != null;
		Vector2? mouseDownPosition;
		Vector2? grabOffset;
		Vector2? grabOffsetGrid;
		RenderTarget2D dragPreview;

		public PuzzleEditorContext(PuzzleDisplay display)
		{
			this.display = display;
		}

		public void Update(GameTime gameTime)
		{
			if(IsDragging && puzzle.game.inputManager.IsLeftMouseButtonReleased())
			{
				DraggedComponent.Position = puzzle.GetMousePositionOnGrid() + grabOffsetGrid.Value + Vector2.One;
				puzzle.AddComponent(DraggedComponent);
				DraggedComponent.OnDrop();

				mouseDownPosition = null;
				dragPreview = null;
				DraggedComponent = null;
				grabOffset = null;
				grabOffsetGrid = null;
			}

			foreach(var component in puzzle.Components)
			{
				if(component is PuzzleComponentWithPosition)
				{
					var componentWithPosition = (PuzzleComponentWithPosition)component;

					if (puzzle.game.inputManager.IsLeftMouseButtonPressed() && !ImGui.GetIO().WantCaptureMouse && componentWithPosition.IsMouseOver())
					{
						SelectedComponent = component;
						mouseDownPosition = puzzle.GetMousePositionWithin();
					}
				}
			}

			if(mouseDownPosition != null && (puzzle.game.inputManager.RawMousePosition - mouseDownPosition).Value.Length() > 10f)
			{
				if(SelectedComponent is PuzzleComponentWithPosition)
				{
					// start dragging component
					DraggedComponent = SelectedComponent as PuzzleComponentWithPosition;

					grabOffset = puzzle.game.screenScale * ((DraggedComponent.GetBounds().Location.ToVector2() * new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight)) - mouseDownPosition);
					var tempGrabOffsetGrid = (grabOffset / puzzle.game.screenScale) / new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight);
					grabOffsetGrid = new Vector2((float)Math.Floor(tempGrabOffsetGrid.Value.X), (float)Math.Floor(tempGrabOffsetGrid.Value.Y));
					Console.WriteLine(grabOffsetGrid);

					var graphics = puzzle.game.GraphicsDevice;
					var component = DraggedComponent;
					dragPreview = new RenderTarget2D(graphics, puzzle.grid.Width * puzzle.grid.CharacterWidth, puzzle.Height * puzzle.grid.CharacterHeight);

					var grid = new Grid(puzzle.game, puzzle.grid.Width, puzzle.grid.Height);
					
					foreach(var character in component.GetCharacters())
					{
						grid.TryAddCharacter(character);
					}

					graphics.SetRenderTarget(dragPreview);
					graphics.Clear(Color.HotPink);
					var spriteBatch = new SpriteBatch(graphics);
					spriteBatch.Begin();

					grid.Draw(spriteBatch);

					spriteBatch.End();
					graphics.SetRenderTarget(null);

					DraggedComponent.Delete();
				}

				mouseDownPosition = null;
			}

			//if (game.inputManager.IsLeftMouseButtonHeld() && IsMouseOver() && puzzle.grid.Contains(puzzle.GetMousePositionWithin()))
			//{
			//	if(grabbedOffset == null)
			//	{
			//		grabbedOffset = Position - puzzle.GetClampedMousePositionOnGrid();
			//	}			
			//}

			//if(game.inputManager.IsLeftMouseButtonReleased() && grabbedOffset != null)
			//{
			//	grabbedOffset = null;

			//	OnDrop();
			//}

			//if(grabbedOffset != null && puzzle.grid.Contains(puzzle.GetClampedMousePositionWithin()))
			//{
			//	var tempPosition = puzzle.GetClampedMousePositionOnGrid() + (grabbedOffset ?? Vector2.Zero);
			//	var bounds = GetBounds();
			//	var clampedPosition = new Vector2(
			//		Math.Max(0, Math.Min(puzzle.Width - bounds.Width, tempPosition.X)),
			//		Math.Max(0, Math.Min(puzzle.Height - bounds.Height, tempPosition.Y))
			//	);

			//	Position = clampedPosition;
			//}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (var component in puzzle.Components)
			{
				component.DrawEditor(spriteBatch);
			}

			if(mouseDownPosition != null)
			{
				var distance = (mouseDownPosition.Value - puzzle.GetMousePositionWithin()).Length();
				var from = display.ConvertPuzzleSpaceCoordsToScreenSpace(mouseDownPosition.Value);
				var to = display.ConvertPuzzleSpaceCoordsToScreenSpace(puzzle.GetMousePositionWithin());
				MonoGame.Primitives2D.DrawLine(spriteBatch, from, to, distance > 10f ? Color.HotPink : Color.Gold);
			}

			if(IsDragging)
			{
				var bounds = DraggedComponent.GetBounds();
				var grid = puzzle.grid;
				var screenBounds = new Rectangle(
					bounds.Left * grid.CharacterWidth,
					bounds.Top * grid.CharacterHeight,
					bounds.Width * grid.CharacterWidth,
					bounds.Height * grid.CharacterHeight
				);

				var mousePosition = puzzle.game.inputManager.RawMousePosition;

				spriteBatch.Draw(dragPreview, new Rectangle((int)(mousePosition.X + grabOffset.Value.X), (int)(mousePosition.Y + grabOffset.Value.Y), (int)(screenBounds.Width * puzzle.game.screenScale), (int)(screenBounds.Height * puzzle.game.screenScale)), screenBounds, Color.White * 0.5f);
			}
		}

		public void DrawUI()
		{
			foreach (var component in puzzle.Components)
			{
				component.DrawEditorUI(puzzle.display);
			}
		}
	}
}
