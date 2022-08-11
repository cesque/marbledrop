﻿using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;

namespace MarbleDrop.Puzzles
{
	public abstract class PuzzleComponentWithPosition : PuzzleComponent
	{
		private Vector2 position;

		public Vector2 Position
		{
			get => position;
			set
			{
				if(position == value) return;
				var old = position;
				position = value;
				PositionChanged(old, position);
			}
		}

		public PuzzleComponentWithPosition(Puzzle puzzle) : this(puzzle, Vector2.Zero) { }

		public PuzzleComponentWithPosition(Puzzle puzzle, string id) : this(puzzle, id, Vector2.Zero) { }

		public PuzzleComponentWithPosition(Puzzle puzzle, Vector2 position) : this(puzzle, new Guid().ToString(), position) { }

		public PuzzleComponentWithPosition(Puzzle puzzle, string id, Vector2 position) : base(puzzle, id)
		{
			Vector2 Position = position;
		}

		public Rectangle GetBounds()
		{
			int maxX = 0, maxY = 0;
			int minX = int.MaxValue, minY = int.MaxValue;

			foreach (var character in GetCharacters())
			{
				if (character.Position.X < minX) minX = (int)character.Position.X;
				if (character.Position.Y < minY) minY = (int)character.Position.Y;
				if (character.Position.X > maxX) maxX = (int)character.Position.X;
				if (character.Position.Y > maxY) maxY = (int)character.Position.Y;
			}

			return new Rectangle(
				minX,
				minY,
				maxX - minX + 1,
				maxY - minY + 1
			);
		}

		public Rectangle GetScreenBounds()
		{
			var offset = (puzzle.display.ScreenBounds.Location.ToVector2() - puzzle.display.CameraPosition) * game.screenScale;
			var bounds = GetBounds();

			return new Rectangle(
				(int)((bounds.X * grid.CharacterWidth * game.screenScale) + offset.X),
				(int)((bounds.Y * grid.CharacterWidth * game.screenScale) + offset.Y),
				(int)(bounds.Width * grid.CharacterWidth * game.screenScale),
				(int)(bounds.Height * grid.CharacterWidth * game.screenScale)
			);
		}

		internal virtual void PositionChanged(Vector2 oldPosition, Vector2 newPosition)
		{
			// todo: update this to reroute connected wires
			foreach(var port in Ports)
			{
				port.Disconnect();
			}
		}

		public override bool IsMouseOver()
		{
			var bounds = GetScreenBounds();

			return bounds.Contains(game.inputManager.RawMousePosition);
		}

		public override void DrawEditor(SpriteBatch spritebatch)
		{
			base.DrawEditor(spritebatch);

			var screenBounds = GetScreenBounds();

			if (IsMouseOver() && !IsEditorSelected)
			{
				MonoGame.Primitives2D.DrawRectangle(spritebatch, screenBounds, Color.CornflowerBlue);
			}

			if (IsEditorSelected)
			{
				MonoGame.Primitives2D.DrawRectangle(spritebatch, screenBounds, Color.GreenYellow);

				var gridSize = new Vector2(grid.CharacterWidth, grid.CharacterHeight) * game.screenScale;

				for (var x = screenBounds.Left; x < screenBounds.Right; x += (int)gridSize.X)
				{
					MonoGame.Primitives2D.DrawLine(
						spritebatch,
						new Vector2(x, screenBounds.Top),
						new Vector2(x, screenBounds.Bottom),
						new Color(Color.Black, 0.4f)
					);
				}

				for (var y = screenBounds.Top; y < screenBounds.Bottom; y += (int)gridSize.Y)
				{
					MonoGame.Primitives2D.DrawLine(
						spritebatch,
						new Vector2(screenBounds.Left, y),
						new Vector2(screenBounds.Right, y),
						new Color(Color.Black, 0.4f)
					);
				}

				var circleSize = 10f;
				var circleVec = new Vector2(circleSize, circleSize);

				var min = circleSize * 0.7f;
				var max = circleSize;
				var step = 1f;

				foreach (var port in Inputs)
				{

					var color = Color.GreenYellow * (port.IsConnected ? 1f : 0.5f);

					for (var i = min; i <= max; i += step)
					{
						MonoGame.Primitives2D.DrawCircle(spritebatch, screenBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
					}
				}

				foreach (var port in Outputs)
				{
					var color = Color.CornflowerBlue * (port.IsConnected ? 1f : 0.5f);

					for (var i = min; i <= max; i+= step)
					{
						MonoGame.Primitives2D.DrawCircle(spritebatch, screenBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
					}
				}
			}
		}

		public void DrawEditorUI(PuzzleDisplay display, bool shouldCloseWindow)
		{
			base.DrawEditorUI(display);
			if (!IsEditorSelected) return;

			var bounds = GetBounds();
			var screenBounds = GetScreenBounds();

			var screen = game.GraphicsDevice.Viewport.Bounds;
			var width = 300;

			ImGui.SetNextWindowPos(new System.Numerics.Vector2(screen.Right - width, 0));
			ImGui.SetNextWindowSize(new System.Numerics.Vector2(width, screen.Height));
			ImGui.Begin(ID);

			ImGui.BeginTable($"{ID}-data", 2);
			ImGui.TableNextRow();
			ImGui.TableNextColumn();
			ImGui.Text("Type");
			ImGui.TableNextColumn();
			ImGui.Text(GetType().Name);

			ImGui.TableNextRow();
			ImGui.TableNextColumn();
			ImGui.Text("Position");
			ImGui.TableNextColumn();
			ImGui.Text($"X: {bounds.X}, Y: {bounds.Y}");
			ImGui.EndTable();

			ImGui.Dummy(new System.Numerics.Vector2(0f, 20f));

			var headerColor = Color.Gold;
			ImGui.TextColored(new System.Numerics.Vector4(headerColor.R / 255.0f, headerColor.G / 255.0f, headerColor.B / 255.0f, headerColor.A / 255.0f), "PORTS:");

			if (Ports.Count == 0)
			{
				var color = Color.Gray;
				ImGui.TextColored(new System.Numerics.Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f), "Component has no ports");
			}
			else
			{
				var inputColor = Color.GreenYellow;
				var outputColor = Color.CornflowerBlue;
				var marbleColor = Color.DeepPink;
				var sparkColor = Color.Gold;

				var inputColorVec = new System.Numerics.Vector4(inputColor.R / 255.0f, inputColor.G / 255.0f, inputColor.B / 255.0f, inputColor.A / 255.0f);
				var outputColorVec = new System.Numerics.Vector4(outputColor.R / 255.0f, outputColor.G / 255.0f, outputColor.B / 255.0f, outputColor.A / 255.0f);
				var marbleColorVec = new System.Numerics.Vector4(marbleColor.R / 255.0f, marbleColor.G / 255.0f, marbleColor.B / 255.0f, marbleColor.A / 255.0f);
				var sparkColorVec = new System.Numerics.Vector4(sparkColor.R / 255.0f, sparkColor.G / 255.0f, sparkColor.B / 255.0f, sparkColor.A / 255.0f);

				foreach (var port in Ports)
				{
					ImGui.PushStyleVar(ImGuiStyleVar.Alpha, port.IsConnected ? 1f : 0.3f);

					var flags = ImGuiTableFlags.BordersOuter;

					ImGui.BeginTable($"{ID}-{port.Name}-table", 2, flags);

					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.Text("Name");
					ImGui.TableNextColumn();
					ImGui.Text(port.Name);

					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.Text("Type");
					ImGui.TableNextColumn();
					ImGui.TextColored(port.Type == PortType.INPUT ? inputColorVec : outputColorVec, Enum.GetName(typeof(PortType), port.Type));

					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.Text("Resource Type");
					ImGui.TableNextColumn();
					ImGui.TextColored(port.ResourceType == Resources.ResourceType.MARBLE ? marbleColorVec : sparkColorVec, Enum.GetName(typeof(Resources.ResourceType), port.ResourceType));

					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.Text("Position");
					ImGui.TableNextColumn();
					var portPosition = port.Position;
					ImGui.Text($"X: {portPosition.X}, Y: {portPosition.Y}");

					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.Text("Is Connected?");
					ImGui.TableNextColumn();
					ImGui.BeginDisabled();
					var isConnected = port.IsConnected;
					ImGui.Checkbox($"##{ID}-{port.Name}-connected", ref isConnected);
					ImGui.EndDisabled();

					ImGui.EndTable();

					ImGui.PopStyleVar();
				}
			}

			if (shouldCloseWindow)
			{
				ImGui.End();
			}
		}

		public override void DrawEditorUI(PuzzleDisplay display) => DrawEditorUI(display, true);

		public override void UpdateEditor(GameTime gametime)
		{
			base.UpdateEditor(gametime);		
		}

		public void OnDrop()
		{
			foreach(var port in Outputs)
			{
				foreach(var component in puzzle.Components)
				{
					if (component == this) continue;
					foreach (var other in component.Inputs)
					{
						if (port.IsConnected || port.ResourceType != other.ResourceType) continue;
						if(port.GridPosition == other.GridPosition)
						{
							port.Connect(other);
						}
					}
				}
			}

			foreach (var port in Inputs)
			{
				foreach (var component in puzzle.Components)
				{
					if (component == this) continue;
					foreach (var other in component.Outputs)
					{
						if (port.IsConnected || port.ResourceType != other.ResourceType) continue;
						if (port.GridPosition == other.GridPosition)
						{
							port.Connect(other);
						}
					}
				}
			}
		}

		public static void PopulateFromJSON(PuzzleComponentWithPosition component, JsonElement element)
		{
			PuzzleComponent.PopulateFromJSON(component, element);

			var data = element.GetProperty("data");

			var position = data.GetProperty("position");

			var x = position.GetProperty("x").GetInt32();
			var y = position.GetProperty("y").GetInt32();

			component.Position = new Vector2(x, y);
		}
	}
}
