﻿using MarbleDrop.Puzzles.Resources;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles
{
    public abstract class PuzzleComponent
    {
        internal Puzzle puzzle;
        internal Game1 game;
        internal Grid grid;
        internal Priority priority;

        public List<ComponentPort> Inputs;
        public List<ComponentPort> Outputs;

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

        abstract public void Initialise();

        abstract public void Update(GameTime gameTime);

        abstract public List<GridCharacter> GetCharacters();

        abstract public void Input(ComponentPort port, Resource resource);

        public void Output(ComponentPort output, Resource resource)
        {
            if (output.Type != PortType.Output)
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

        public static void PopulateFromJSON(PuzzleComponent component, JsonElement element)
        {
            var id = element.GetProperty("id").GetString();
            component.ID = id;

            var data = element.GetProperty("data");
        }
    }
}
