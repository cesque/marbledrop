using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Editor
{
	public abstract class PuzzleEditorModeStrategy
	{
		internal PuzzleEditorContext context;
		internal PuzzleDisplay display => context.display;
		internal Puzzle puzzle => context.puzzle;

		public PuzzleEditorModeStrategy(PuzzleEditorContext context)
		{
			this.context = context;
		}

		public virtual void Enter() { }
		public virtual void Leave() { }

		public virtual void Update(GameTime gameTime) { }
		public virtual void Draw(SpriteBatch spriteBatch) { }
		public virtual void DrawUI() { }
	}

	public enum PuzzleEditorMode
	{
		EDITPUZZLEINFO,
		SELECT,
		EDITWIRE,
	}
}
