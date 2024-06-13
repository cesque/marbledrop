using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
	public class BitmapFont
	{
		Game1 game;
		string assetName;
		Texture2D map;

		public int CharacterWidth;
		public int CharacterHeight;

		private static Dictionary<int, int> rotationMap = new()
		{
			{ 131, 148 }, // │ -> ─
			{ 148, 131 }, // ─ -> │

			{ 170, 143 }, // ┌ -> ┐
			{ 143, 169 }, // ┐ -> ┘
			{ 169, 144 }, // ┘ -> └
			{ 144, 170 }, // └ -> ┌
		};

		public BitmapFont(Game1 game, string assetName, int characterWidth, int characterHeight)
		{
			this.game = game;
			this.assetName = assetName;
			CharacterWidth = characterWidth;
			CharacterHeight = characterHeight;

			map = game.Content.Load<Texture2D>(assetName);
		}

		public Rectangle GetCharacterBounds(int i)
		{
			var offset = i * CharacterWidth;
			var y = (int)Math.Floor(offset / (float)map.Width) * CharacterWidth;
			var x = offset % map.Width;
			return new Rectangle(x, y, CharacterWidth, CharacterHeight);
		}

		public void DrawCharacter(SpriteBatch spriteBatch, Grid grid, int i, Vector2 position, Color foregroundColor, Color backgroundColor)
		{
			var x = (int)Math.Round(position.X);
			var y = (int)Math.Round(position.Y);

			var rectangle = GetCharacterBounds(i);

			spriteBatch.Draw(Globals.DebugTexture, new Rectangle(x * CharacterWidth, y * CharacterHeight, CharacterWidth, CharacterHeight), backgroundColor);
			spriteBatch.Draw(map, new Rectangle(x * CharacterWidth, y * CharacterHeight, CharacterWidth, CharacterHeight), rectangle, foregroundColor);
		}

		public static List<int> ConvertStringToIndices(string s)
		{
			return s.ToCharArray().Select(c => (int)c).ToList();
		}

		public static int Rotate(int index, int quarterTurns)
		{
			var iterations = quarterTurns % 4;

			var value = index;
			for(var i = 0; i < iterations; i++)
			{
				if(rotationMap.TryGetValue(value, out int nextValue))
				{
					value = nextValue;
				} else
				{
					return value;
				}
			}

			return value;
		}
	}
}
