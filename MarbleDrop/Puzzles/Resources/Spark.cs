using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Resources
{
	class Spark : Resource
	{
		public Spark(Puzzle puzzle) : base(puzzle, ResourceType.SPARK)
		{

		}

		public override List<GridCharacter> GetCharacters(Vector2 position)
		{
			var list = new List<GridCharacter>();

			list.Add(new GridCharacter(
				puzzle.grid,
				42,
				position,
				puzzle.grid.Palette.Get("yellow"),
				Priority.Spark
			));

			return list;
		}
	}
}
