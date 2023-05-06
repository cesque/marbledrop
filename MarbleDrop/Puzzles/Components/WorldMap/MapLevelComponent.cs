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
using System.IO;
using ImGuiNET;

namespace MarbleDrop.Puzzles.Components.WorldMap
{
	class MapLevelComponent : PuzzleComponentWithPosition
	{
		public new const string TypeName = "maplevel";

		List<string> files = new List<string>();

		string _fileName;
		string FileName
		{
			get { return _fileName; }
			set
			{
				_fileName = value;
				Console.WriteLine(value);
				Console.WriteLine(File.Exists(value));
				IsValid = value != null && File.Exists(value);
			}
		}

		bool _isValid = false;
		bool IsValid
		{
			get { return _isValid; }
			set {
				_isValid = value;
				var color = value ? "white" : "red";
				UpdateLayout();
			}
		}

		List<GridCharacter> layout;

		public MapLevelComponent(Puzzle puzzle, string id) : base(puzzle, id)
		{
			Position = Vector2.Zero;
		}

		public MapLevelComponent(Puzzle puzzle) : this(puzzle, new Guid().ToString()) { }

		public override void Initialise()
		{
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.SPARK, new Vector2(-1, 1), "spark/left"));
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.SPARK, new Vector2(1, -1), "spark/top"));
			Outputs.Add(new ComponentPort(this, PortType.OUTPUT, ResourceType.SPARK, new Vector2(3, 1), "spark/right"));

			Inputs.Add(new ComponentPort(this, PortType.INPUT, ResourceType.SPARK, new Vector2(1, 3), "spark/input"));

			FileName = null;
		}

		void UpdateLayout()
		{
			var backgroundColor = IsValid ? "white" : "red";
			layout = new List<GridCharacter>()
			{
				new GridCharacter(grid, 16, Position + new Vector2(2, 1), grid.Palette.Get(backgroundColor), grid.Palette.Get("black"), Priority.Component),
				new GridCharacter(grid, 17, Position + new Vector2(0, 1), grid.Palette.Get(backgroundColor), grid.Palette.Get("black"), Priority.Component),
				new GridCharacter(grid, 30, Position + new Vector2(1, 0), grid.Palette.Get(backgroundColor), grid.Palette.Get("black"), Priority.Component),
				new GridCharacter(grid, 31, Position + new Vector2(1, 2), grid.Palette.Get(backgroundColor), grid.Palette.Get("black"), Priority.Component),

				new GridCharacter(grid, 3, Position + new Vector2(1, 1), grid.Palette.Get("magenta"), grid.Palette.Get("black"), Priority.Component),
			};
		}

		internal override void PositionChanged(Vector2 oldPosition, Vector2 newPosition)
		{
			base.PositionChanged(oldPosition, newPosition);
			UpdateLayout();
		}

		public override void Update(GameTime gameTime)
		{

		}

		public override List<GridCharacter> GetCharacters()
		{
			return layout;
		}

		public override void Input(ComponentPort port, Resource resource)
		{

		}

		public static PuzzleComponent FromJSON(Puzzle puzzle, JsonElement element)
		{
			var component = new SwitchComponent(puzzle, element.GetProperty("id").GetString());

			PuzzleComponentWithPosition.PopulateFromJSON(component, element);

			return component;
		}

		public override JsonObject ToJSON()
		{
			var json = base.ToJSON();
			json["type"] = TypeName;
			return json;
		}

		void LoadPuzzle()
		{
			var newPuzzle = Puzzle.Load(game, FileName);
			puzzle.display.Mount(newPuzzle);
		}

		public override void DrawEditorUI(PuzzleDisplay display)
		{
			base.DrawEditorUI(display, false);

			var screen = puzzle.game.GraphicsDevice.Viewport.Bounds;

			ImGui.Dummy(new System.Numerics.Vector2(0, 20f));
			var popupId = "maplevelselectpopup-" + ID;

			ImGui.Text("Current level: ");
			ImGui.TextColored(puzzle.grid.Palette.Get(FileName == null ? "red" : "yellow").ToVector4().ToNumerics(), FileName ?? "???");

			if (ImGui.Button("Select level"))
			{
				ImGui.OpenPopup(popupId);

				files = Directory.EnumerateFiles("./Content/Levels/").ToList();
				files.ForEach(Console.WriteLine);
				Console.WriteLine();
			}

			if (ImGui.BeginPopupModal(popupId))
			{
				ImGui.BeginChildFrame(1, new System.Numerics.Vector2(screen.Width * 0.75f, screen.Height * 0.75f));

				ImGui.BeginListBox("Files");
				foreach (var file in files)
				{
					if(ImGui.Selectable(file))
					{
						FileName = file;
						ImGui.CloseCurrentPopup();
					}
				}
				ImGui.EndListBox();

				ImGui.EndChildFrame();
				ImGui.EndPopup();
			}

			if (ImGui.Button("Edit..."))
			{
				LoadPuzzle();
			}
		}
	}
}
