using MarbleDrop.Puzzles.Resources;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Components
{
	class WireSegment
	{
		public Vector2 Start;
		public Vector2 End;

		public float Length;

		public Dictionary<Resource, float> Resources;
		public List<Resource> OutputQueue;

		public readonly Vector2 Direction;

		static Dictionary<Vector2, int> marbleIndexesPerDirection = new Dictionary<Vector2, int>
		{
			{ new Vector2(1, 0), 16 },
			{ new Vector2(-1, 0), 17 },
			{ new Vector2(0, 1), 31 },
			{ new Vector2(0, -1), 30 },
		};

		Wire wire;

		public WireSegment(Wire wire, Vector2 start, Vector2 end)
		{
			this.wire = wire;

			Start = start;
			End = end;

			if (!(start.X == end.X ^ start.Y == end.Y))
			{
				throw new Exception("wire segment is not orthogonal!");
			}

			Length = Vector2.Distance(Start, End);

			Resources = new Dictionary<Resource, float>();

			OutputQueue = new List<Resource>();

			var xDiff = End.X - Start.X;
			var yDiff = End.Y - Start.Y;

			if (Math.Abs(xDiff) > Math.Abs(yDiff))
			{
				yDiff = 0;
			}
			else
			{
				xDiff = 0;
			}

			Direction = new Vector2(
				Math.Sign(xDiff),
				Math.Sign(yDiff)
			);
		}

		public void Update(GameTime gameTime)
		{
			OutputQueue.Clear();

			foreach (var resource in Resources.Keys.ToList())
			{
				Resources[resource] += (float)gameTime.ElapsedGameTime.TotalSeconds * wire.Speed;

				if (Resources[resource] > Length + 0.5)
				{
					OutputQueue.Add(resource);
				}
			}

			foreach (var removedMarble in OutputQueue)
			{
				Resources.Remove(removedMarble);
			}
		}

		public void Input(Resource resource)
		{
			Resources.Add(resource, 0f);
		}

		public List<GridCharacter> GetCharacters()
		{
			var characters = new List<GridCharacter>();


			/* todo: rewrite so its not duplicated? */
			var current = new Vector2(Start.X, Start.Y);

			var priority = wire.ResourceType == ResourceType.MARBLE ? Priority.Wire : Priority.SparkWire;

			if (Direction.X == 1)
			{
				do
				{
					characters.Add(new GridCharacter(
						wire.puzzle.grid,
						148,
						current,
						wire.foregroundColor,
						wire.backgroundColor,
						priority
					));

					current += Direction;

				} while (current.X <= End.X);
			}
			else if (Direction.X == -1)
			{
				do
				{
					characters.Add(new GridCharacter(
						wire.puzzle.grid,
						148,
						current,
						wire.foregroundColor,
						wire.backgroundColor,
						priority
					));

					current += Direction;

				} while (current.X >= End.X);
			}
			else if (Direction.Y == 1)
			{
				do
				{
					characters.Add(new GridCharacter(
						wire.puzzle.grid,
						131,
						current,
						wire.foregroundColor,
						wire.backgroundColor,
						priority
					));

					current += Direction;

				} while (current.Y <= End.Y);
			}
			else if (Direction.Y == -1)
			{
				do
				{
					characters.Add(new GridCharacter(
						wire.puzzle.grid,
						131,
						current,
						wire.foregroundColor,
						wire.backgroundColor,
						priority
					));

					current += Direction;

				} while (current.Y >= End.Y);
			}

			// show marbles above wires
			foreach (var resource in Resources.Keys)
			{
				var distance = Resources[resource];
				var percentage = distance / Length;

				var position = Vector2.Lerp(Start, End, percentage);

				//var last = distance - 1;

				//if(last < 0)
				//{
				//    last = 0;
				//}

				//var lastPercentage = last / Length;
				//var lastPosition = Vector2.Lerp(Start, End, lastPercentage);

				////characters.Add(new GridCharacter(
				////    wire.puzzle.grid,
				////    128,
				////    lastPosition,
				////    Priority.WireMarbleTrail
				////));

				//// modify existing tile to add trail
				//var tile = characters.Find(character => {
				//    var isSameX = Math.Round(character.Position.X) == Math.Round(lastPosition.X);
				//    var isSameY = Math.Round(character.Position.Y) == Math.Round(lastPosition.Y);

				//    return isSameX && isSameY;
				//});

				//if(tile != null)
				//{
				//    //changing tiles charcter index to add trail
				//    //if (tile.CharacterIndex == 131)
				//    //{
				//    //    tile.CharacterIndex = 138;
				//    //}
				//    //else if (tile.CharacterIndex == 148)
				//    //{
				//    //    tile.CharacterIndex = 157;
				//    //}

				//    tile.CharacterIndex = 171;

				//    // changing tile color to add trail
				//    tile.ForegroundColor = marble.GetColor();
				//}            

				if (resource.Type == ResourceType.MARBLE)
				{
					var marble = (Marble)resource;
					characters.AddRange(marble.GetCharacters(position, 171));
					marble.EnableTrail();
				}
				else if (resource.Type == ResourceType.SPARK)
				{
					var spark = (Spark)resource;
					characters.AddRange(spark.GetCharacters(position));
				}
			}

			return characters;
		}
	}
}
