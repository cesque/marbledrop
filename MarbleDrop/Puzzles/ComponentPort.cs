using MarbleDrop.Puzzles.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles
{
	public enum PortType
	{
		INPUT,
		OUTPUT,
	}

	public class ComponentPort
	{
		public Vector2 Position;
		public string Name;

		public PortType Type;
		public ResourceType ResourceType;
		public PuzzleComponent Component;
		public ComponentPort ConnectedPort; // or null

		public bool IsConnected => ConnectedPort != null;

		public Vector2 GridPosition
		{
			get
			{
				if (Component is PuzzleComponentWithPosition)
				{
					var componentWithPosition = (PuzzleComponentWithPosition)Component;
					return componentWithPosition.Position + Position;
				}

				return Position;
			}
		}

		public ComponentPort(PuzzleComponent component, PortType portType, ResourceType resourceType, Vector2 position, string name)
		{
			Component = component;
			Type = portType;
			ResourceType = resourceType;
			Position = position;
			Name = name;
		}

		public void Output(Resource resource)
		{
			if (Type != PortType.OUTPUT) throw new Exception("tried to output a marble from a non-output port!");

			if (IsConnected)
			{
				ConnectedPort.Input(resource);
			}
		}

		public void Input(Resource resource)
		{
			Component.Input(this, resource);
		}

		public void Connect(ComponentPort other)
		{
			if (ResourceType != other.ResourceType)
			{
				throw new Exception("couldn't connect two ports of different resource types!");
			}

			if (GridPosition != other.GridPosition)
			{
				Console.WriteLine("port positions don't match up between connections of " + Component.ID + " and  " + other.Component.ID + "!");
				Console.WriteLine(Component.ID + ": " + Position);
				Console.WriteLine(other.Component.ID + ": " + other.Position);
			}

			ConnectedPort = other;
			other.ConnectedPort = this;
		}

		public void Disconnect()
		{
			if (IsConnected)
			{
				ConnectedPort.ConnectedPort = null;
			}
			ConnectedPort = null;
		}
	}
}
