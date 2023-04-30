using MarbleDrop.Puzzles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
	public class PuzzleDisplay
	{
		readonly List<float> zoomSteps = new List<float>()
		{
			0.5f,
			0.75f,
			1f,
			1.5f,
		};

		int zoomIndex = 1;

		public Vector2 CameraPosition;
		public float CameraZoom
		{
			get { return zoomSteps[zoomIndex]; }
		}

		public RenderTarget2D RenderTarget;
		public Rectangle Bounds;
		public PuzzleEditorContext Editor;

		public bool IsEdgePanEnabled = false;

		public Rectangle ScreenBounds {
			get
			{
				return new Rectangle(
					Bounds.X * grid.CharacterWidth,
					Bounds.Y * grid.CharacterHeight,
					Bounds.Width * grid.CharacterWidth,
					Bounds.Height * grid.CharacterHeight
				);
			}
		}

		Game1 game;
		Grid grid;
		internal Puzzle puzzle;

		List<GridCharacter> layout;

		public PuzzleDisplay(Game1 game, Grid grid, Rectangle bounds)
		{
			this.game = game;
			this.grid = grid;
			Bounds = bounds;
			CameraPosition = new Vector2(0, 0);
			RenderTarget = new RenderTarget2D(game.GraphicsDevice, ScreenBounds.Width, ScreenBounds.Height);
			Editor = new PuzzleEditorContext(this);

			SetLayout();

			ResetZoom();
		}

		void SetLayout()
		{
			var foregroundColor = grid.Palette.Get("white");
			var backgroundColor = grid.Palette.Get("black");

			layout = new List<GridCharacter>()
			{
				new GridCharacter(grid, 170, Bounds.Location.ToVector2() + new Vector2(-1, -1), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 143, Bounds.Location.ToVector2() + new Vector2(Bounds.Width, -1), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 144, Bounds.Location.ToVector2() + new Vector2(-1, Bounds.Height), foregroundColor, backgroundColor, Priority.Component),
				new GridCharacter(grid, 169, Bounds.Location.ToVector2() + new Vector2(Bounds.Width, Bounds.Height), foregroundColor, backgroundColor, Priority.Component),
			};

			for(var x = 0; x < Bounds.Width; x++)
			{
				layout.Add(new GridCharacter(grid, 148, Bounds.Location.ToVector2() + new Vector2(x, -1), foregroundColor, backgroundColor, Priority.Component));
				layout.Add(new GridCharacter(grid, 148, Bounds.Location.ToVector2() + new Vector2(x, Bounds.Height), foregroundColor, backgroundColor, Priority.Component));
			}
			
			for(var y = 0; y < Bounds.Height; y++)
			{
				layout.Add(new GridCharacter(grid, 131, Bounds.Location.ToVector2() + new Vector2(-1, y), foregroundColor, backgroundColor, Priority.Component));
				layout.Add(new GridCharacter(grid, 131, Bounds.Location.ToVector2() + new Vector2(Bounds.Width, y), foregroundColor, backgroundColor, Priority.Component));
			}
		}

		public void Mount(Puzzle puzzle)
		{
			this.puzzle = puzzle;
			puzzle.display = this;
		}

		public void ZoomIn()
		{
			var from = CameraZoom;

			zoomIndex++;
			if(zoomIndex >= zoomSteps.Count) zoomIndex = zoomSteps.Count - 1;

			var to = CameraZoom;

			if (from != to)
			{
				// update camera position to zoom around cursor
				var ratio = to / from;

				var mousePosition = GetClampedMousePositionInDisplaySpace();
				var difference = mousePosition - CameraPosition;
				CameraPosition += difference * ratio;
			}
		}

		public void ZoomOut()
		{
			var from = CameraZoom;

			zoomIndex--;
			if (zoomIndex < 0) zoomIndex = 0;

			var to = CameraZoom;

			if (from != to)
			{
				// update camera position to zoom around cursor
				var ratio = to / from;

				var mousePosition = GetClampedMousePositionInDisplaySpace();
				var difference = mousePosition - CameraPosition;
				CameraPosition += difference * ratio;
			}
		}

		public void ResetZoom()
		{
			zoomIndex = zoomSteps.IndexOf(1f);
		}
		
		// converts screen space -> display space
		public Vector2 GetMousePositionInDisplaySpace()
		{
			return game.inputManager.MousePosition - ScreenBounds.Location.ToVector2();
		}

		public Vector2 GetClampedMousePositionInDisplaySpace()
		{
			var mousePositionWithin = GetMousePositionInDisplaySpace();
			return new Vector2(
				Math.Max(0, Math.Min(mousePositionWithin.X, ScreenBounds.Width)),
				Math.Max(0, Math.Min(mousePositionWithin.Y, ScreenBounds.Height))
			);
		}

		public bool IsMouseWithin()
		{
			return ScreenBounds.Contains(game.inputManager.MousePosition);
		}

		public void Update(GameTime gameTime)
		{
			var cameraVelocity = new Vector2(0, 0);

			if (IsMouseWithin())
			{
				if (game.inputManager.IsMiddleMouseButtonHeld())
				{
					var delta = game.inputManager.MouseDelta;
					cameraVelocity = new Vector2(-delta.X, -delta.Y);
				}
				else if (IsEdgePanEnabled)
				{
					// edge pan code
					#region edge pan
					const float scrollTriggerRange = 70.0f;
					const float scrollTriggerSpeed = 5.0f;

					var clampedMousePosition = GetClampedMousePositionInDisplaySpace();

					var x = (ScreenBounds.Width / 2) - clampedMousePosition.X;
					var xDistanceFromEdge = (ScreenBounds.Width / 2) - Math.Abs(x);
					var xDistanceAdjusted = -Math.Min(0, xDistanceFromEdge - scrollTriggerRange);
					cameraVelocity.X = (xDistanceAdjusted / scrollTriggerRange) * scrollTriggerSpeed * Math.Sign(x) * -1.0f;

					var y = (ScreenBounds.Height / 2) - clampedMousePosition.Y;
					var yDistanceFromEdge = (ScreenBounds.Height / 2) - Math.Abs(y);
					var yDistanceAdjusted = -Math.Min(0, yDistanceFromEdge - scrollTriggerRange);
					cameraVelocity.Y = (yDistanceAdjusted / scrollTriggerRange) * scrollTriggerSpeed * Math.Sign(y) * -1.0f;
					#endregion edge pan
				}

				var scrollWheelDelta = game.inputManager.MouseScrollWheelDelta;
				if(scrollWheelDelta > 0)
				{
					ZoomIn();
				}
				else if(scrollWheelDelta < 0)
				{
					ZoomOut();
				}
			}

			cameraVelocity /= CameraZoom;

			CameraPosition = new Vector2(
				Math.Max(0, Math.Min(grid.GetScreenBounds().Width - ScreenBounds.Width, CameraPosition.X + cameraVelocity.X)),
				Math.Max(0, Math.Min(grid.GetScreenBounds().Height - ScreenBounds.Height, CameraPosition.Y + cameraVelocity.Y))
			);

			puzzle.Update(gameTime);

			if (Editor.Enabled)
			{
				Editor.Update(gameTime);
			}
		}

		public Vector2 ConvertDisplaySpaceToScreenSpace(Vector2 position)
		{
			return position + ScreenBounds.Location.ToVector2();
		}

		public Vector2 ConvertPuzzleSpaceToScreenSpace(Vector2 position)
		{
			return ConvertDisplaySpaceToScreenSpace(puzzle.ConvertPuzzleSpaceToDisplaySpace(position));
		}

		public void DrawCharacters()
		{
			foreach (var character in layout)
			{
				grid.TryAddCharacter(character);
			}

			Editor.DrawCharacters(grid);

			if(CameraZoom != 1f)
			{
				var text = $" {CameraZoom.ToString()}x ";
				var textIndices = BitmapFont.ConvertStringToIndices(text);
				for (var x = 0; x < text.Length; x++) {
					var position = new Vector2(Bounds.Right - text.Length + x, Bounds.Bottom);
					var character = new GridCharacter(grid, textIndices[x], position, grid.Palette.Get("white"), grid.Palette.Get("darkgrey"), Priority.Component);
					grid.TryAddCharacter(character);
				}
			}
		}

		public void DrawRenderTarget(SpriteBatch spriteBatch)
		{
			game.GraphicsDevice.SetRenderTarget(RenderTarget);
			game.GraphicsDevice.Clear(Color.Black);

			var matrix = Matrix.Multiply(
				Matrix.CreateTranslation(-CameraPosition.X, -CameraPosition.Y, 0),
				Matrix.CreateScale(CameraZoom)
			);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, matrix);

			puzzle.DrawCharacters(spriteBatch);
			//spriteBatch.Draw(Globals.DebugTexture, new Rectangle(0, 0, bounds.Width, bounds.Height), Color.Magenta);
			//puzzle.DrawDebug(spriteBatch);
			spriteBatch.End();
			game.GraphicsDevice.SetRenderTarget(null);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(RenderTarget, ScreenBounds, Color.White);
		}
		public void DrawEditor(SpriteBatch spriteBatch)
		{
			Editor.Draw(spriteBatch);
		}

		public void DrawEditorUI()
		{
			Editor.DrawUI();
		}
	}
}
