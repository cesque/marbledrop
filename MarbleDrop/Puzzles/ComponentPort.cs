﻿using MarbleDrop.Puzzles.Resources;
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
        Input,
        Output,
    }

    public class ComponentPort
    {
        public Vector2 Position;
        public string Name;

        public PortType Type;
        public ResourceType ResourceType;
        public PuzzleComponent Component;
        public ComponentPort ConnectedPort; // or null

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
            if(Type != PortType.Output) throw new Exception("tried to output a marble from a non-output port!");

            if(ConnectedPort != null)
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
            if(ResourceType != other.ResourceType)
            {
                throw new Exception("couldn't connect two ports of different resource types!");
            }

            if(Position != other.Position)
            {
                Console.WriteLine("port positions don't match up between connections of " + Component.ID + " and  " + other.Component.ID + "!");
                Console.WriteLine(Component.ID + ": " + Position);
                Console.WriteLine(other.Component.ID + ": " + other.Position);
            }

            ConnectedPort = other;
            other.ConnectedPort = this;
        }
    }
}
