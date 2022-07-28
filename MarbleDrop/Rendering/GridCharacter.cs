using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Rendering
{
	public class GridCharacter
	{
		Grid grid;

		public int CharacterIndex;
		public Vector2 Position;
		public int Priority;
		public Color ForegroundColor;
		public Color BackgroundColor;

		public GridCharacter(Grid grid) : this(grid, 0, Vector2.Zero) { }
		public GridCharacter(Grid grid, int index, Vector2 position) : this(grid, index, position, 0) { }

		public GridCharacter(Grid grid, int index, Vector2 position, Priority priority) : this(grid, index, position, (int)priority) { }
		public GridCharacter(Grid grid, int index, Vector2 position, Color foregroundColor, Priority priority) : this(grid, index, position, foregroundColor, (int)priority) { }
		public GridCharacter(Grid grid, int index, Vector2 position, Color foregroundColor, Color backgroundColor, Priority priority) : this(grid, index, position, foregroundColor, backgroundColor, (int)priority) { }


		public GridCharacter(Grid grid, int index, Vector2 position, int priority) : this(grid, index, position, grid.Palette.Get("white"), grid.Palette.Get("black"), priority) { }
		public GridCharacter(Grid grid, int index, Vector2 position, Color foregroundColor, int priority) : this(grid, index, position, foregroundColor, grid.Palette.Get("black"), priority) { }
		public GridCharacter(Grid grid, int index, Vector2 position, Color foregroundColor, Color backgroundColor, int priority)
		{
			this.grid = grid;

			CharacterIndex = index;
			Position = position;
			Priority = priority;
			ForegroundColor = foregroundColor;
			BackgroundColor = backgroundColor;
		}


		public void Draw(SpriteBatch spriteBatch)
		{
			grid.Font.DrawCharacter(spriteBatch, grid, CharacterIndex, Position, ForegroundColor, BackgroundColor);
		}
	}
}
