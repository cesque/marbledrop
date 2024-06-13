using MarbleDrop.Puzzles.Resources;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MarbleDrop.Puzzles
{
	public abstract class PuzzleComponent
	{
		public const string TypeName = "unknowncomponent";

		internal Puzzle puzzle;
		internal Game1 game;
		internal Grid grid;
		internal Priority priority;

		public List<ComponentPort> Inputs;
		public List<ComponentPort> Outputs;

		public List<ComponentPort> Ports
		{
			get { return Inputs.Concat(Outputs).ToList(); }
		}

		public string ID;

		public PuzzleComponent(Puzzle puzzle, string id)
		{
			this.ID = id;
			this.puzzle = puzzle;
			this.game = puzzle.game;
			this.grid = puzzle.grid;

			this.priority = Priority.Component;

			this.Inputs = new List<ComponentPort>();
			this.Outputs = new List<ComponentPort>();
		}

		public PuzzleComponent(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }

		virtual public void Initialise()
		{
			foreach (ComponentPort port in Inputs)
			{
				port.Disconnect();
			}

			foreach (ComponentPort port in Outputs)
			{
				port.Disconnect();
			}
		}

		abstract public void Update(GameTime gameTime);

		virtual public void UpdateEditor(GameTime gametime) { }

		abstract public List<GridCharacter> GetCharacters();

		abstract public void Input(ComponentPort port, Resource resource);

		public virtual void DrawEditor(SpriteBatch spritebatch) { }

		public virtual void DrawEditorUI(PuzzleDisplay display) { }

		public void Output(ComponentPort output, Resource resource)
		{
			if (output.Type != PortType.OUTPUT)
			{
				throw new Exception("can't output marble from input port!");
			}

			output.Output(resource);
		}

		public void Output(string portName, Resource resource)
		{
			var output = Outputs.Find(port => port.Name == portName);

			if (output == null)
			{
				throw new Exception("couldn't find port with name " + portName);
			}

			Output(output, resource);
		}
		
		public void Delete()
		{
			foreach(var port in Ports)
			{
				port.Disconnect();
			}

			puzzle.RemoveComponent(this);
		}

		public void AutomaticallyConnectPorts()
		{
			// todo: update this to reroute connected wires
			foreach (var port in Ports)
			{
				port.Disconnect();
			}


			foreach (var port in Inputs)
			{
				foreach (var component in puzzle.Components)
				{
					if (component == this) continue;
					foreach (var other in component.Outputs)
					{
						if (port.IsConnected || port.ResourceType != other.ResourceType)
						{
							Console.WriteLine(port.IsConnected ? "couldn't automatically connect due to: port is already connected" : "couldn't automatically connect due to: port resource type mismatch");
							continue;
						}

						if (port.GridPosition == other.GridPosition)
						{
							port.Connect(other);
						}
					}
				}
			}

			foreach (var port in Outputs)
			{
				foreach (var component in puzzle.Components)
				{
					if (component == this) continue;
					foreach (var other in component.Inputs)
					{
						if (port.IsConnected || port.ResourceType != other.ResourceType)
						{
							Console.WriteLine(port.IsConnected ? "couldn't automatically connect due to: port is already connected" : "couldn't automatically connect due to: port resource type mismatch");
							continue;
						}

						if (port.GridPosition == other.GridPosition)
						{
							port.Connect(other);
						}
					}
				}
			}
		}


		abstract public bool IsMouseOver();

		public static void PopulateFromJSON(PuzzleComponent component, JsonElement element)
		{
			var id = element.GetProperty("id").GetString();
			component.ID = id;

			var data = element.GetProperty("data");
		}

		public virtual JsonObject ToJSON()
		{
			var json = new JsonObject();
			json.Add("type", TypeName);
			json.Add("id", ID);

			var connections = new JsonArray();

			var shouldSpecifyOutput = Outputs.Count > 1 || Outputs.First().ResourceType != ResourceType.MARBLE;

			foreach (var port in Outputs)
			{
				Console.WriteLine(port.Name);
				if (!port.IsConnected) continue;

				var connection = new JsonObject();

				var shouldSpecifyInput = port.ConnectedPort.Component.Inputs.Where(other => other.ResourceType == port.ResourceType).Count() > 1;

				if (shouldSpecifyOutput) connection.Add("output", port.Name);
				connection.Add("component", port.ConnectedPort.Component.ID);
				if (shouldSpecifyInput) connection.Add("input", port.ConnectedPort.Name);

				connections.Add(connection);
			}

			json.Add("connections", connections);

			json.Add("data", new JsonObject());

			return json;
		}
	}
}
