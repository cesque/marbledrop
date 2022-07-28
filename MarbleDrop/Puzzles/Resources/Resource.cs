using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Resources
{
	public enum ResourceType
	{
		MARBLE,
		SPARK,
	}

	public abstract class Resource
	{
		internal Puzzle puzzle;
		public ResourceType Type;

		public Resource(Puzzle puzzle, ResourceType type)
		{
			this.puzzle = puzzle;
			Type = type;
		}

		abstract public List<GridCharacter> GetCharacters(Vector2 position);
	}
}
