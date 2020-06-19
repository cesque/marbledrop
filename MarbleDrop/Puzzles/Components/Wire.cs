using MarbleDrop.Puzzles.Resources;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Components
{
    class Wire : PuzzleComponent
	{
		public float Speed;
		public List<WireSegment> Segments;
		public ResourceType ResourceType;

		static Vector2 up = new Vector2(0, -1);
		static Vector2 down = new Vector2(0, 1);
		static Vector2 left = new Vector2(-1, 0);
		static Vector2 right = new Vector2(1, 0);

		internal Color foregroundColor;
		internal Color backgroundColor;

		public static Dictionary<(Vector2, Vector2), int> CornerCharacters = new Dictionary<(Vector2, Vector2), int>
		{
			{ (up, up), 131 },
			{ (up, right), 170 },
			{ (up, down), 131 },
			{ (up, left), 143 },

			{ (right, up), 169 },
			{ (right, right), 148 },
			{ (right, down), 143 },
			{ (right, left), 148 },

			{ (down, up), 131 },
			{ (down, right), 144 },
			{ (down, down), 131 },
			{ (down, left), 169 },

			{ (left, up), 144 },
			{ (left, right), 148 },
			{ (left, down), 170 },
			{ (left, left), 148 },
		};
		
		public Wire(Puzzle puzzle, string id) : base(puzzle, id)
		{
			Speed = 15f;
			Segments = new List<WireSegment>();

		}

		public Wire(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }

		public override void Initialise()
		{
			if(ResourceType == ResourceType.MARBLE)
            {
				foregroundColor = grid.Palette.Get("grey");
				backgroundColor = grid.Palette.Get("black");
            } 
			else if(ResourceType == ResourceType.SPARK)
            {
				foregroundColor = grid.Palette.Get("darkgrey");
				backgroundColor = grid.Palette.Get("black");
			}
		}

		public void ConnectFrom(ComponentPort fromPort, Vector2 to)
		{
			Segments.Add(new WireSegment(this, fromPort.Position, to));

			Inputs.Clear();

			var resourceTypeName = ResourceType.ToString().ToLower();

			var newPort = new ComponentPort(this, PortType.Input, ResourceType, fromPort.Position, resourceTypeName + "/input");
			newPort.Connect(fromPort);

			Inputs.Add(newPort);
		}

		public void Extend(Vector2 to)
		{
			var last = Segments.Last();
			Segments.Add(new WireSegment(this, last.End, to));

			Outputs.Clear();

			var resourceTypeName = ResourceType.ToString().ToLower();

			Outputs.Add(new ComponentPort(this, PortType.Output, ResourceType, to, resourceTypeName + "/output"));
		}

		public void ConnectTo(ComponentPort toPort)
		{
			var last = Segments.Last();
			Segments.Add(new WireSegment(this, last.End, toPort.Position));

			Outputs.Clear();

			var resourceTypeName = ResourceType.ToString().ToLower();

			var newPort = new ComponentPort(this, PortType.Output, ResourceType, toPort.Position, resourceTypeName + "/output");
			newPort.Connect(toPort);

			Outputs.Add(newPort);
		}

		public override List<GridCharacter> GetCharacters()
		{
			var characters = new List<GridCharacter>();

			foreach (var segment in Segments)
			{
				characters.AddRange(segment.GetCharacters());
			}

			if(Segments.Count > 1)
			{
				for (var i = 0; i < Segments.Count - 1; i++) {
					var thisSegment = Segments[i];
					var nextSegment = Segments[i + 1];

					var intersectionLocation = thisSegment.End;

					var inDirection = thisSegment.Direction;
					var outDirection = nextSegment.Direction;

					var characterIndex = CornerCharacters[(inDirection, outDirection)];

					characters.Add(new GridCharacter(
						puzzle.grid,
						characterIndex,
						intersectionLocation,
						foregroundColor,
						backgroundColor,
						ResourceType == ResourceType.MARBLE ? Priority.WireCorners : Priority.SparkWireCorners
					));
				}
			}

			return characters;
		}

		public override void Input(ComponentPort port, Resource resource)
		{
			Segments.First().Input(resource);
		}

		public override void Update(GameTime gameTime)
		{
			for(var i = 0; i<Segments.Count; i++)
			{
				Segments[i].Update(gameTime);

				foreach (var resource in Segments[i].OutputQueue)
				{
					if(i + 1 < Segments.Count)
					{
						Segments[i + 1].Input(resource);
					}
					else
					{
						Output(Outputs.First(), resource);
					}
				}
			}
		}

		public static PuzzleComponent FromJSON(Puzzle puzzle, JsonElement element)
		{
			var component = new Wire(puzzle, element.GetProperty("id").GetString());

			PuzzleComponentWithPosition.PopulateFromJSON(component, element);

			/* --- handle custom deserialization here --- */
			var data = element.GetProperty("data");

			var hasResourceTypeDefinition = data.TryGetProperty("resourceType", out var resourceTypeJSON);
			if(hasResourceTypeDefinition)
            {
				var resourceType = resourceTypeJSON.GetString();
				switch(resourceType)
                {
					case "marble":
						component.ResourceType = ResourceType.MARBLE;
						break;
					case "spark":
						component.ResourceType = ResourceType.SPARK;
						break;
					default:
						throw new InvalidEnumArgumentException("unrecognised resource type on wire definition: " + resourceType);
                }
            }

			var pointsJSON = data.GetProperty("points").EnumerateArray();
			var points = new List<Vector2>();

			for(var i = 0; i<pointsJSON.Count() - 1; i++)
			{
				var point1 = pointsJSON.ElementAt(i);
				var p1x = point1.GetProperty("x").GetInt32();
				var p1y = point1.GetProperty("y").GetInt32();
				
				var point2 = pointsJSON.ElementAt(i + 1);
				var p2x = point2.GetProperty("x").GetInt32();
				var p2y = point2.GetProperty("y").GetInt32();

				var start = new Vector2(p1x, p1y);
				var end = new Vector2(p2x, p2y);

				var segment = new WireSegment(component, start, end);

				Console.WriteLine(start + " -> " + end);

				component.Segments.Add(segment);
			}

			if(component.Segments.Count == 0)
			{
				throw new Exception("can't deserialize a wire with 0 segments!");
			}

			var resourceTypeName = component.ResourceType.ToString().ToLower();

			component.Inputs = new List<ComponentPort>
			{
				new ComponentPort(component, PortType.Input, component.ResourceType, component.Segments.First().Start, resourceTypeName + "/input")
			};
			component.Outputs = new List<ComponentPort>
			{
				new ComponentPort(component, PortType.Output, component.ResourceType, component.Segments.Last().End, resourceTypeName + "/output")
			};


			return component;
		}
	}
}
