using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
	public class Grid
	{
		Game1 game;

		public int Width;
		public int Height;

		public int CharacterWidth => Font.CharacterWidth;
		public int CharacterHeight => Font.CharacterHeight;

		public BitmapFont Font;
		public ColorPalette Palette;

		public GridCharacter[,] Characters;

		public Grid(Game1 game, int width, int height)
		{
			this.game = game;

			Width = width;
			Height = height;

			Characters = new GridCharacter[Width, Height];

			for (var x = 0; x < Width; x++)
			{
				for (var y = 0; y < Height; y++)
				{
					Characters[x, y] = null;
				}
			}

			Font = new BitmapFont(game, "zxevolution", 8, 8);
			Palette = ColorPalette.Load("pico-8.txt");
		}

		public Rectangle GetScreenBounds()
		{
			return new Rectangle(
				0,
				0,
				CharacterWidth * Width,
				CharacterHeight * Height
			);
		}

		public Vector2 GetGridCoordinatesFromPosition(Vector2 position)
		{
			var x = (int)Math.Floor(position.X / CharacterWidth);
			if (x == Width) x--;

			var y = (int)Math.Floor(position.Y / CharacterHeight);
			if (y == Height) y--;

			if(x < 0 || x >= Width || y < 0 || y >= Height)
			{
				throw new ArgumentOutOfRangeException("position", $"co-ordinates {{{position.X}, {position.Y}}} -> {{{x}, {y}}} are outside of the bounds of the grid {{{Width - 1}, {Height - 1}}}. maybe you meant to call `GetClampedGridCoordinatesFromPosition()`?");
			}

			return new Vector2(x, y);
		}

		public Vector2 GetClampedGridCoordinatesFromPosition(Vector2 position)
		{
			var x = Math.Max(0, Math.Min(Width * CharacterWidth, position.X));
			var y = Math.Max(0, Math.Min(Height * CharacterHeight, position.Y));

			return GetGridCoordinatesFromPosition(new Vector2(x, y));
		}

		public bool Contains(Vector2 position)
		{
			return position.X >= 0 && position.X <= (Width * CharacterWidth) && position.Y >= 0 && position.Y <= (Height * CharacterHeight);
		}

		public float GetMaxScreenScaleToFitOnScreen()
		{
			var screenWidth = game.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
			var screenHeight = game.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

			var gridSize = GetScreenBounds();

			var scale = Math.Min(
				screenWidth / gridSize.Width,
				screenHeight / gridSize.Height
			);

			return (float)Math.Floor(scale * 0.95f);
		}

		public void Update(GameTime gameTime)
		{

			//for (var x = 0; x < Width; x++)
			//{
			//    for (var y = 0; y < Height; y++)
			//    {
			//        if(Globals.RNG.Next(100) > 90)
			//        {
			//            Characters[x, y] = new GridCharacter(
			//                this,
			//                Globals.RNG.Next(4),
			//                new Vector2(x, y)
			//            );
			//        }
			//    }
			//}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			var testGridBrightness = 0.1f;
			var testGridForegroundColor = new Color(0.0f, 0.0f, 0.0f);
			var testGridBackgroundColor = new Color(testGridBrightness, testGridBrightness, testGridBrightness);

			for (var x = 0; x < Width; x++)
			{
				for (var y = 0; y < Height; y++)
				{
					var character = Characters[x, y];

					MonoGame.Primitives2D.FillRectangle(spriteBatch, new Rectangle(
						x * CharacterWidth,
						y * CharacterHeight,
						CharacterWidth,
						CharacterHeight
					), (x + y) % 2 == 0 ? testGridForegroundColor : testGridBackgroundColor);

					if (character != null)
					{
						character.Position = new Vector2(x, y);
						character.Draw(spriteBatch);
					}
				}
			}

			Characters = new GridCharacter[Width, Height];
		}

		public bool TryAddCharacter(GridCharacter character)
		{

			var x = (int)Math.Round(character.Position.X);
			var y = (int)Math.Round(character.Position.Y);

			if (
				x < 0
				|| x >= Characters.GetLength(0)
				|| y < 0
				|| y >= Characters.GetLength(1)
			)
			{
				return false;
			}

			var current = Characters[x, y];

			if (current == null || current.Priority <= character.Priority)
			{
				Characters[x, y] = character;
				return true;
			}

			return false;
		}
	}
}
