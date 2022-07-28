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
		int characterWidth;
		int characterHeight;
		Texture2D map;

		public BitmapFont(Game1 game, string assetName, int characterWidth, int characterHeight)
		{
			this.game = game;
			this.assetName = assetName;
			this.characterWidth = characterWidth;
			this.characterHeight = characterHeight;

			map = game.Content.Load<Texture2D>(assetName);
		}

		public Rectangle GetCharacterBounds(int i)
		{
			var offset = i * characterWidth;
			var y = (int)Math.Floor(offset / (float)map.Width) * characterWidth;
			var x = offset % map.Width;
			return new Rectangle(x, y, characterWidth, characterHeight);
		}

		public void DrawCharacter(SpriteBatch spriteBatch, Grid grid, int i, Vector2 position, Color foregroundColor, Color backgroundColor)
		{
			var x = (int)Math.Round(position.X);
			var y = (int)Math.Round(position.Y);

			var rectangle = GetCharacterBounds(i);

			spriteBatch.Draw(Globals.DebugTexture, new Rectangle(x * characterWidth, y * characterHeight, characterWidth, characterHeight), backgroundColor);
			spriteBatch.Draw(map, new Rectangle(x * characterWidth, y * characterHeight, characterWidth, characterHeight), rectangle, foregroundColor);
		}
	}
}
