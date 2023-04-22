using MarbleDrop.Puzzles.Components;
using MarbleDrop.Puzzles.Resources;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles
{
	public class Puzzle
	{
		internal Game1 game;
		internal Grid grid;
		internal PuzzleDisplay display;

		public int Width => grid.Width;
		public int Height => grid.Height;

		public Rectangle Bounds => grid.GetScreenBounds();

		public List<PuzzleComponent> Components;
		public List<Wire> Wires => Components.Where(component => component is Wire).Select(component => component as Wire).ToList();

		public string Name;

		float debugTimer;
		List<PuzzleComponent> componentsReadyForDeletion;

		public Puzzle(Game1 game)
		{
			this.game = game;
			this.grid = new Grid(game, 80, 40);

			Components = new List<PuzzleComponent>();
			componentsReadyForDeletion = new List<PuzzleComponent>();

		}

		public void Update(GameTime gameTime)
		{
			debugTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

			foreach (var component in componentsReadyForDeletion)
			{
				Components.Remove(component);
			}
			componentsReadyForDeletion.Clear();

			foreach (var component in Components)
			{
				component.Update(gameTime);
			}

			grid.Update(gameTime);

		}

		// converts display space -> puzzle space
		public Vector2 ConvertDisplaySpaceToPuzzleSpace(Vector2 position)
		{
			return position + display.CameraPosition;
		}

		public Vector2 ConvertPuzzleSpaceToDisplaySpace(Vector2 position)
		{
			return position - display.CameraPosition;
		}

		public Vector2 GetMousePositionInPuzzleSpace()
		{
			return ConvertDisplaySpaceToPuzzleSpace(display.GetMousePositionInDisplaySpace());
		}

		public Vector2 GetClampedMousePositionInPuzzleSpace()
		{
			return ConvertDisplaySpaceToPuzzleSpace(display.GetClampedMousePositionInDisplaySpace());
		}

		public Vector2 GetMousePositionInGridSpace() => grid.ConvertPuzzleSpaceToGridSpace(GetMousePositionInPuzzleSpace());
		public Vector2 GetClampedMousePositionInGridSpace() => grid.ConvertPuzzleSpaceToGridSpaceClamped(GetClampedMousePositionInPuzzleSpace());

		public void DrawCharacters(SpriteBatch spriteBatch)
		{
			foreach (var component in Components)
			{
				var characters = component.GetCharacters();
				foreach (var character in characters)
				{
					grid.TryAddCharacter(character);
				}
			}


			grid.Draw(spriteBatch);
		}

		public void AddComponent(PuzzleComponent component)
		{
			Components.Add(component);
		}

		public void RemoveComponent(PuzzleComponent component)
		{
			componentsReadyForDeletion.Add(component);
		}


		public static Puzzle FromJSON(Game1 game, JsonElement element)
		{
			var puzzle = new Puzzle(game);

			puzzle.Name = element.GetProperty("name").GetString();

			Console.WriteLine("loading: " + puzzle.Name);

			var components = element.GetProperty("components").EnumerateArray();

			foreach (var componentJSON in components)
			{
				PuzzleComponent component = null;
				var type = componentJSON.GetProperty("type").GetString();
				switch (type)
				{
					case "wire":
						component = Wire.FromJSON(puzzle, componentJSON);
						break;
					case "buffer":
						component = BufferComponent.FromJSON(puzzle, componentJSON);
						break;
					case "switch":
						component = SwitchComponent.FromJSON(puzzle, componentJSON);
						break;
					case "playerspawner":
						component = PlayerMarbleSpawnerComponent.FromJSON(puzzle, componentJSON);
						break;
				}

				if (component == null)
				{
					Console.WriteLine("no component created for type: " + type + "!");
				}
				else
				{
					component.Initialise();
					puzzle.Components.Add(component);
				}

				//var type = componentJSON.GetProperty("type").GetString();

				//Console.WriteLine("checking " + type);

				//if (componentTypes.ContainsKey(type))
				//{
				//    var componentType = componentTypes[type];

				//    Console.WriteLine(componentType);
				//    var component = (PuzzleComponent)componentType.GetMethod("FromJSON").Invoke(null, new object[] { puzzle, componentJSON });

				//    Console.WriteLine(component.ID);

				//    puzzle.Components.Add(component);
				//}
			}

			// loop through again and connect stuff now everything exists
			foreach (var componentJSON in components)
			{
				var id = componentJSON.GetProperty("id").GetString();

				var component = puzzle.Components.Find(c => c.ID == id);

				if (componentJSON.TryGetProperty("connections", out var connections))
				{
					foreach (var connection in connections.EnumerateArray())
					{
						// either find output based on name given in json, or use the first (default one)
						var hasSpecifiedOutput = connection.TryGetProperty("output", out var outputName);
						var hasSpecifiedInput = connection.TryGetProperty("input", out var inputName);
						var connectedComponentID = connection.GetProperty("component").GetString();

						var connectedComponent = puzzle.Components.Find(c => c.ID == connectedComponentID);

						if (!hasSpecifiedOutput && component.Outputs.Where(o => o.ResourceType == ResourceType.MARBLE).Count() > 1)
						{
							throw new Exception("can't get default input port of a component with multiple (marble) input ports!");
						}

						if (!hasSpecifiedInput && connectedComponent.Inputs.Where(o => o.ResourceType == ResourceType.MARBLE).Count() > 1)
						{
							throw new Exception("can't get default input port of a component with multiple (marble) input ports!");
						}

						ComponentPort output = null;
						ComponentPort input = null;

						try
						{
							var defaultOutput = component.Outputs.Count == 1 ? component.Outputs.First() : component.Outputs.Find(c => c.ResourceType == ResourceType.MARBLE);
							var defaultInput = connectedComponent.Inputs.Count == 1 ? connectedComponent.Inputs.First() : connectedComponent.Inputs.Find(c => c.ResourceType == defaultOutput.ResourceType);

							output = hasSpecifiedOutput ?
								component.Outputs.Find(o => o.Name == outputName.GetString())
								: defaultOutput;
							input = hasSpecifiedInput ?
								connectedComponent.Inputs.Find(o => o.Name == inputName.GetString())
								: defaultInput;
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							Console.WriteLine("error finding input and output from json file for components " + component.ID + " -> " + connectedComponentID);
							Console.WriteLine("maybe you need to be more specific about which ports are being used?");
							Console.WriteLine();
						}

						output.Connect(input);
					}
				}
			}

			return puzzle;
		}
	}
}
