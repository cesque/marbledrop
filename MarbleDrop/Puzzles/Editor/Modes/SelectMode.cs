using ImGuiNET;
using MarbleDrop.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDrop.Puzzles.Editor.Modes
{
	internal class SelectMode : PuzzleEditorModeStrategy
	{
		public PuzzleComponent SelectedComponent;

		// properties for dragging a component
		public PuzzleComponentWithPosition DraggedComponent;
		public bool IsDragging => DraggedComponent != null;
		Vector2? mouseDownPosition;
		Vector2? grabOffset;
		Vector2? grabOffsetGrid;
		RenderTarget2D dragPreview;

		Type newComponentType;
		RenderTarget2D newComponentPreview;


		public SelectMode(PuzzleEditorContext context) : base(context) { }

		public override void Enter()
		{
			base.Enter();
		}

		public override void Leave()
		{
			if (IsDragging)
			{
				DropDraggedComponent(false);
			}

			base.Leave();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (newComponentType != null)
			{
				if (puzzle.game.inputManager.IsLeftMouseButtonReleased())
				{
					var component = (PuzzleComponentWithPosition)Activator.CreateInstance(newComponentType, new object[] { puzzle });

					component.Position = puzzle.GetClampedMousePositionInGridSpace();
					puzzle.AddComponent(component);
					component.Initialise();
					component.AutomaticallyConnectPorts();

					newComponentType = null;
				}
				else if (puzzle.game.inputManager.IsRightMouseButtonReleased())
				{
					newComponentType = null;
				}
			}
			else
			{
				HandleSelectionAndDragging();
			}
		}

		void HandleSelectionAndDragging()
		{
			// drop dragged component
			if (IsDragging && puzzle.game.inputManager.IsLeftMouseButtonReleased())
			{
				DropDraggedComponent(true);
			}

			var didSelectComponentThisFrame = false;
			// select component on click
			foreach (var component in puzzle.Components)
			{
				if (component is PuzzleComponentWithPosition)
				{
					var componentWithPosition = (PuzzleComponentWithPosition)component;

					// this variable helps us not check hover state on every frame for every component, which i think would not be great perf wise
					var shouldCheckHover = puzzle.game.inputManager.IsLeftMouseButtonPressed() || puzzle.game.inputManager.IsLeftMouseButtonReleased() || puzzle.game.inputManager.IsRightMouseButtonReleased();
					if (shouldCheckHover && !ImGui.GetIO().WantCaptureMouse && componentWithPosition.IsMouseOver())
					{
						if (puzzle.game.inputManager.IsLeftMouseButtonPressed())
						{
							if (puzzle.game.inputManager.IsKeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftControl))
							{
								component.Delete();
							}
							else
							{
								SelectedComponent = component;
								mouseDownPosition = puzzle.GetMousePositionInPuzzleSpace();
								didSelectComponentThisFrame = true;
							}
						}
						else if (puzzle.game.inputManager.IsLeftMouseButtonReleased())
						{
							mouseDownPosition = null;
						}
					}
				}
			}

			// if clicking empty space, deselect
			if (puzzle.game.inputManager.IsLeftMouseButtonPressed() && !ImGui.GetIO().WantCaptureMouse && !didSelectComponentThisFrame) SelectedComponent = null;

			// start dragging component if dragged further than a certain distance
			if (mouseDownPosition != null && (puzzle.GetMousePositionInPuzzleSpace() - mouseDownPosition).Value.Length() > 6f)
			{
				if (SelectedComponent is PuzzleComponentWithPosition)
				{
					// start dragging component
					DraggedComponent = SelectedComponent as PuzzleComponentWithPosition;

					grabOffset = (DraggedComponent.GetBounds().Location.ToVector2() * new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight)) - mouseDownPosition;
					var tempGrabOffsetGrid = grabOffset / new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight);
					grabOffsetGrid = new Vector2((float)Math.Floor(tempGrabOffsetGrid.Value.X), (float)Math.Floor(tempGrabOffsetGrid.Value.Y));

					var graphics = puzzle.game.GraphicsDevice;
					dragPreview = new RenderTarget2D(graphics, puzzle.grid.Width * puzzle.grid.CharacterWidth, puzzle.Height * puzzle.grid.CharacterHeight);

					var grid = new Grid(puzzle.game, puzzle.grid.Width, puzzle.grid.Height);

					foreach (var character in DraggedComponent.GetCharacters())
					{
						grid.TryAddCharacter(character);
					}

					graphics.SetRenderTarget(dragPreview);
					graphics.Clear(Color.HotPink);
					var spriteBatch = new SpriteBatch(graphics);
					spriteBatch.Begin();

					grid.Draw(spriteBatch);

					spriteBatch.End();
					graphics.SetRenderTarget(null);

					//DraggedComponent.Delete();
				}

				mouseDownPosition = null;
			}
		}

		private void DropDraggedComponent(bool updatePosition)
		{
			//puzzle.AddComponent(DraggedComponent);

			// todo: make drag+drop behaviour not horrible
			var oldPosition = DraggedComponent.Position;
			//var test = 
			var connectedPorts = DraggedComponent.Ports.Where(port => port.IsConnected);
			var connectedWires = connectedPorts.Select(port => port.ConnectedPort.Component as Components.Wire);
			var connectedComponentPorts = connectedWires.Select(wire => (Wire: wire, Input: wire.Inputs.First().ConnectedPort, Output: wire.Outputs.First().ConnectedPort)).ToList();

			if (updatePosition)
			{
				try
				{
					DraggedComponent.Position = puzzle.grid.ConvertPuzzleSpaceToGridSpace(puzzle.GetMousePositionInPuzzleSpace()) + grabOffsetGrid.Value + Vector2.One;
				}
				catch (ArgumentOutOfRangeException)
				{
					// todo: add error feedback for invalid drop location
				}
			}

			foreach (var (wire, input, output) in connectedComponentPorts)
			{
				wire.ConnectPortsByDefaultPath(input, output);
			}

			DraggedComponent.PositionChanged(oldPosition, DraggedComponent.Position);

			mouseDownPosition = null;
			dragPreview = null;
			DraggedComponent = null;
			grabOffset = null;
			grabOffsetGrid = null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			foreach (var component in puzzle.Components)
			{
				component.DrawEditor(spriteBatch);

				if (component is PuzzleComponentWithPosition)
				{
					var componentWithPosition = component as PuzzleComponentWithPosition;
					var screenBounds = componentWithPosition.GetScreenBounds();

					DrawComponentHoverIfApplicable(spriteBatch, componentWithPosition, screenBounds);
					DrawComponentSelectedIfApplicable(spriteBatch, componentWithPosition, screenBounds);
				}
			}

			if (mouseDownPosition != null)
			{
				// draw something different if user has started dragging a component but not more than the "stickiness"???

				//var distance = (mouseDownPosition.Value - puzzle.GetMousePositionWithin()).Length();
				//var from = display.ConvertPuzzleSpaceCoordsToScreenSpace(mouseDownPosition.Value);
				//var to = display.ConvertPuzzleSpaceCoordsToScreenSpace(puzzle.GetMousePositionWithin());
				//MonoGame.Primitives2D.DrawLine(spriteBatch, from, to, distance > 10f ? Color.HotPink : Color.Gold);
			}

			// draw the dragged component wherever the mouse is
			if (IsDragging)
			{
				var bounds = DraggedComponent.GetBounds();
				var grid = puzzle.grid;
				var screenBounds = new Rectangle(
					bounds.Left * grid.CharacterWidth,
					bounds.Top * grid.CharacterHeight,
					bounds.Width * grid.CharacterWidth,
					bounds.Height * grid.CharacterHeight
				);

				var mousePosition = puzzle.game.inputManager.MousePosition;
				var drawBounds = new Rectangle(
					(int)(mousePosition.X + grabOffset.Value.X),
					(int)(mousePosition.Y + grabOffset.Value.Y),
					screenBounds.Width,
					screenBounds.Height
				);

				spriteBatch.Draw(dragPreview, drawBounds, screenBounds, Color.White * 0.5f);

				var gridSize = new Vector2(grid.CharacterWidth, grid.CharacterHeight);
				var circleSize = 5f;
				var circleVec = new Vector2(circleSize, circleSize);

				var min = circleSize * 0.7f;
				var max = circleSize;
				var step = 1f;

				foreach (var port in DraggedComponent.Inputs)
				{

					var color = Color.GreenYellow * (port.IsConnected ? 1f : 0.5f);

					for (var i = min; i <= max; i += step)
					{
						MonoGame.Primitives2D.DrawCircle(spriteBatch, drawBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
					}
				}

				foreach (var port in DraggedComponent.Outputs)
				{
					var color = Color.CornflowerBlue * (port.IsConnected ? 1f : 0.5f);

					for (var i = min; i <= max; i += step)
					{
						MonoGame.Primitives2D.DrawCircle(spriteBatch, drawBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
					}
				}
			}

			if(newComponentType != null)
			{
				var grid = puzzle.grid;
				var screenBounds = newComponentPreview.Bounds;

				var mousePosition = puzzle.display.ConvertDisplaySpaceToScreenSpace(puzzle.ConvertPuzzleSpaceToDisplaySpace(puzzle.grid.ConvertGridSpaceToPuzzleSpace(puzzle.GetClampedMousePositionInGridSpace())));
				var drawBounds = new Rectangle(
					(int)(mousePosition.X),
					(int)(mousePosition.Y),
					screenBounds.Width,
					screenBounds.Height
				);

				spriteBatch.Draw(newComponentPreview, drawBounds, screenBounds, Color.White * 0.5f);
			}
		}

		void DrawComponentHoverIfApplicable(SpriteBatch spriteBatch, PuzzleComponentWithPosition component, Rectangle screenBounds)
		{
			if (component.IsMouseOver() && component != SelectedComponent)
			{
				MonoGame.Primitives2D.DrawRectangle(spriteBatch, screenBounds, Color.CornflowerBlue);
			}
		}

		void DrawComponentSelectedIfApplicable(SpriteBatch spriteBatch, PuzzleComponentWithPosition component, Rectangle screenBounds)
		{
			if (component == SelectedComponent)
			{
				MonoGame.Primitives2D.DrawRectangle(spriteBatch, screenBounds, Color.GreenYellow);

				var gridSize = new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight);

				for (var x = screenBounds.Left; x < screenBounds.Right; x += (int)gridSize.X)
				{
					MonoGame.Primitives2D.DrawLine(
						spriteBatch,
						new Vector2(x, screenBounds.Top),
						new Vector2(x, screenBounds.Bottom),
						new Color(Color.Black, 0.4f)
					);
				}

				for (var y = screenBounds.Top; y < screenBounds.Bottom; y += (int)gridSize.Y)
				{
					MonoGame.Primitives2D.DrawLine(
						spriteBatch,
						new Vector2(screenBounds.Left, y),
						new Vector2(screenBounds.Right, y),
						new Color(Color.Black, 0.4f)
					);
				}

				var circleSize = 5f;

				var min = circleSize * 0.7f;
				var max = circleSize;
				var step = 1f;

				foreach (var port in component.Inputs)
				{

					var color = Color.GreenYellow * (port.IsConnected ? 1f : 0.5f);

					for (var i = min; i <= max; i += step)
					{
						MonoGame.Primitives2D.DrawCircle(spriteBatch, screenBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
					}
				}

				foreach (var port in component.Outputs)
				{
					var color = Color.CornflowerBlue * (port.IsConnected ? 1f : 0.5f);

					for (var i = min; i <= max; i += step)
					{
						MonoGame.Primitives2D.DrawCircle(spriteBatch, screenBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
					}
				}
			}
		}

		public override void DrawUI()
		{
			base.DrawUI();

			var headerColor = Color.Gold;

			if (SelectedComponent != null)
			{
				SelectedComponent.DrawEditorUI(puzzle.display);
			}
			else
			{
				var screen = puzzle.game.GraphicsDevice.Viewport.Bounds;
				var width = 300;
				ImGui.SetNextWindowPos(new System.Numerics.Vector2(screen.Right - width, 0));
				ImGui.SetNextWindowSize(new System.Numerics.Vector2(width, screen.Height));
				ImGui.Begin("SELECT MODE");

				ImGui.TextColored(new System.Numerics.Vector4(headerColor.R / 255.0f, headerColor.G / 255.0f, headerColor.B / 255.0f, headerColor.A / 255.0f), "CONTROLS:");

				var flags = ImGuiTableFlags.BordersH;
				ImGui.BeginTable($"select-mode-controls-table", 2, flags);

				ImGui.TableNextRow();
				ImGui.TableNextColumn();
				ImGui.Text("Select/Drag");
				ImGui.TableNextColumn();
				ImGui.Text("LMB");

				ImGui.TableNextRow();
				ImGui.TableNextColumn();
				ImGui.Text("Delete");
				ImGui.TableNextColumn();
				ImGui.Text("RMB");

				ImGui.TableNextRow();
				ImGui.TableNextColumn();
				ImGui.Text("Switch to Wire mode");
				ImGui.TableNextColumn();
				ImGui.Text("W");

				ImGui.EndTable();

				ImGui.Dummy(new System.Numerics.Vector2(0f, 20f));

				ImGui.TextColored(new System.Numerics.Vector4(headerColor.R / 255.0f, headerColor.G / 255.0f, headerColor.B / 255.0f, headerColor.A / 255.0f), "CREATE COMPONENT:");

				// todo: maybe the name can go into the class as a static member?
				var componentsButtons = new List<(string Name, Type Type)>()
				{
					(Name: "Buffer", Type: typeof(Components.BufferComponent)),
					(Name: "Switch", Type: typeof(Components.SwitchComponent)),
					(Name: "Marble Spawner", Type: typeof(Components.PlayerMarbleSpawnerComponent)),
				};

				foreach (var button in componentsButtons)
				{
					if (ImGui.Button(button.Name))
					{
						if (!IsDragging)
						{
							newComponentType = button.Type;

							var graphics = puzzle.game.GraphicsDevice;
							var component = (PuzzleComponentWithPosition)Activator.CreateInstance(newComponentType, new object[] { puzzle });
							component.Position = Vector2.Zero;
							component.Initialise();
							var componentBounds = component.GetBounds();
							newComponentPreview = new RenderTarget2D(graphics, componentBounds.Width * puzzle.grid.CharacterWidth, componentBounds.Height * puzzle.grid.CharacterHeight);

							var grid = new Grid(puzzle.game, componentBounds.Width, componentBounds.Height);

							foreach (var character in component.GetCharacters())
							{
								grid.TryAddCharacter(character);
							}

							graphics.SetRenderTarget(newComponentPreview);
							graphics.Clear(Color.HotPink);
							var spriteBatch = new SpriteBatch(graphics);
							spriteBatch.Begin();

							grid.Draw(spriteBatch);

							spriteBatch.End();
							graphics.SetRenderTarget(null);

						}
					}
				}
			}
		}
	}
}
