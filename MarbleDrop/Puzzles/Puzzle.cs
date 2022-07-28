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

		public List<PuzzleComponent> Components;
		public string Name;

		float debugTimer;

		public Puzzle(Game1 game, Grid grid)
		{
			this.game = game;
			this.grid = grid;

			Components = new List<PuzzleComponent>();

			//var spawnerComponent = new PlayerMarbleSpawnerComponent(this);
			//Components.Add(spawnerComponent);

			//var switchComponent = new SwitchComponent(this);
			//Components.Add(switchComponent);

			//var bufferComponent = new BufferComponent(this);
			//Components.Add(bufferComponent);

			//var wire = new Wire(this);

			//wire.ConnectFrom(spawnerComponent.Outputs.First(), new Vector2(20, 6));
			//wire.Extend(new Vector2(20, 16));
			//wire.ConnectTo(switchComponent.Inputs[0]);
			//Components.Add(wire);

			//var outputWire1 = new Wire(this);
			//outputWire1.ConnectFrom(switchComponent.Outputs[0], new Vector2(24, 4));
			//var outputWire2 = new Wire(this);
			//outputWire2.ConnectFrom(switchComponent.Outputs[1], new Vector2(24, 28));
			//outputWire2.ConnectTo(bufferComponent.Inputs[0]);

			//var outputWire3 = new Wire(this);
			//outputWire3.ConnectFrom(bufferComponent.Outputs[0], new Vector2(50, 28));


			//Components.Add(outputWire1);
			//Components.Add(outputWire2);
			//Components.Add(outputWire3);
		}

		public void Update(GameTime gameTime)
		{
			debugTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

			foreach (var component in Components)
			{
				component.Update(gameTime);
			}
		}

		public void Draw()
		{
			foreach (var component in Components)
			{
				var characters = component.GetCharacters();
				foreach (var character in characters)
				{
					grid.TryAddCharacter(character);
				}
			}
		}

		public void DrawEditor(SpriteBatch spriteBatch)
		{
			foreach (var component in Components)
			{
				component.DrawEditor(spriteBatch);
				//if(component.IsMouseOver())
				//{

				//    // debug ports
				//    var ports = new List<ComponentPort>();
				//    ports.AddRange(component.Inputs);
				//    ports.AddRange(component.Outputs);


				//    foreach (var port in ports)
				//    {
				//        var x = port.Position.X * grid.CharacterWidth * game.screenScale;
				//        var y = port.Position.Y * grid.CharacterHeight * game.screenScale;
				//        var width = grid.CharacterWidth * game.screenScale;
				//        var height = grid.CharacterHeight * game.screenScale;

				//        var color = port.Type == PortType.Input ? Color.HotPink : Color.GreenYellow;

				//        if (port.Type == PortType.Input && (debugTimer % 1 > 0) && (debugTimer % 1 < 0.33f))
				//        {
				//            //spriteBatch.Draw(Globals.DebugTexture, new Rectangle((int)x, (int)y, grid.CharacterWidth, grid.CharacterHeight), color);
				//            MonoGame.Primitives2D.DrawCircle(spriteBatch, new Vector2(x, y), width, 16, color);

				//        }

				//        if (port.Type == PortType.Output && (debugTimer % 1 > 0.33f) && (debugTimer % 1 < 0.66f))
				//        {
				//            MonoGame.Primitives2D.DrawCircle(spriteBatch, new Vector2(x, y), height, 16, color);

				//        }

				//    }
				//}
			}
		}

		public void DrawEditorUI()
		{
			foreach (var component in Components)
			{
				component.DrawEditorUI();
			}
		}

		public static Puzzle FromJSON(Game1 game, Grid grid, JsonElement element)
		{
			var puzzle = new Puzzle(game, grid);

			puzzle.Name = element.GetProperty("name").GetString();

			Console.WriteLine("loading: " + puzzle.Name);

			var components = element.GetProperty("components").EnumerateArray();

			//var componentTypes = new Dictionary<string, Type>
			//{
			//    { "buffer", typeof(BufferComponent) },
			//    { "switch", typeof(SwitchComponent) },
			//    { "playerspawner", typeof(PlayerMarbleSpawnerComponent) },
			//    { "test", typeof(TestComponent) },
			//};

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

				//if(componentTypes.ContainsKey(type))
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
							Console.WriteLine("error finding input and output from json file for components " + component.ID + " -> " + connectedComponentID);
							Console.WriteLine("maybe you need to be more specific about which ports are being used?");
						}

						output.Connect(input);
					}
				}
			}

			return puzzle;
		}
	}
}
