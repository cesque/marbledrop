using ImGuiNET;
using MarbleDrop.Puzzles.Components;
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
	internal class EditWireMode : PuzzleEditorModeStrategy
	{
		Wire hoveredWire;
		WireSegment hoveredSegment;
		float wireHoverTimer = 0.0f;
		float wireHoverTimerMax = 400.0f;

		WireSegment draggedSegment;
		(Vector2 Start, Vector2 End)? newDraggedSegmentEndpoints;

		ComponentPort hoveredPort;
		ComponentPort selectedPort;

		public EditWireMode(PuzzleEditorContext context) : base(context) { }

		public override void Enter()
		{
			base.Enter();
		}

		public override void Leave()
		{
			hoveredWire = null;
			hoveredSegment = null;
			StopDraggingSegment(true);

			base.Leave();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			wireHoverTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			wireHoverTimer %= wireHoverTimerMax;

			hoveredWire = null;
			hoveredSegment = null;
			hoveredPort = null;

			if (draggedSegment != null && puzzle.game.inputManager.IsRightMouseButtonReleased())
			{
				StopDraggingSegment(true);
			}

			if (draggedSegment != null && puzzle.game.inputManager.IsLeftMouseButtonReleased())
			{
				StopDraggingSegment();
			}

			var mousePosition = puzzle.GetClampedMousePositionInGridSpace();

			if (selectedPort == null)
			{
				// show hovered wire/segment and handle dragging start
				foreach (var wire in puzzle.Wires)
				{
					foreach (var segment in wire.Segments)
					{
						var characters = segment.GetCharacters();

						if (characters.Any(character => mousePosition.X == character.Position.X && mousePosition.Y == character.Position.Y))
						{
							// mouse is over wire
							hoveredWire = wire;
							hoveredSegment = segment;

							if (puzzle.game.inputManager.IsLeftMouseButtonPressed())
							{
								if (puzzle.game.inputManager.IsKeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftControl))
								{
									segment.Wire.Delete();
								}
								else
								{
									StartDraggingSegment(segment);
								}
							}
						}
					}
				}
			}

			// drag wire segment
			if (draggedSegment != null)
			{
				var isStartConnectedToPort = draggedSegment.IsFirst && draggedSegment.Wire.Inputs.Any(port => port.IsConnected);
				var isStartConnectedToAnotherSegment = !draggedSegment.IsFirst;

				var isEndConnectedToPort = draggedSegment.IsLast && draggedSegment.Wire.Outputs.Any(port => port.IsConnected);
				var isEndConnectedToAnotherSegment = !draggedSegment.IsLast;

				var (start, end) = (draggedSegment.Start, draggedSegment.End);

				var startX = start.X;
				var startY = start.Y;
				var endX = end.X;
				var endY = end.Y;

				var isVertical = draggedSegment.Orientation == WireSegmentOrientation.VERTICAL;

				// arcane boolean statements replacing a 4-depth nested if
				// discovered through drawing a decision tree, converting it to a truth table, solving with K-maps, and simplifying the resultant boolean expression
				// makes sure that the endpoints move in the most intuitive way
				var shouldUpdateStartX = !isStartConnectedToPort && (isVertical || !isStartConnectedToAnotherSegment) && !(isVertical && isEndConnectedToPort);
				var shouldUpdateStartY = !isStartConnectedToPort && !(isVertical && isStartConnectedToAnotherSegment) && (isVertical || !isEndConnectedToPort);
				var shouldUpdateEndX = !isEndConnectedToPort && (isVertical || !isEndConnectedToAnotherSegment) && !(isVertical && isStartConnectedToPort);
				var shouldUpdateEndY = !isEndConnectedToPort && !(isVertical && isEndConnectedToAnotherSegment) && (isVertical || !isStartConnectedToPort);

				if(shouldUpdateStartX) startX = mousePosition.X;
				if(shouldUpdateStartY) startY = mousePosition.Y;
				if(shouldUpdateEndX) endX = mousePosition.X;
				if(shouldUpdateEndY) endY = mousePosition.Y;

				newDraggedSegmentEndpoints = (new Vector2(startX, startY), new Vector2(endX, endY));
			}

			if (hoveredSegment != null && puzzle.game.inputManager.IsRightMouseButtonReleased())
			{
				WireSegment segment1, segment2;
				var index = hoveredWire.Segments.IndexOf(hoveredSegment);
				try
				{
					if (hoveredSegment.Orientation == WireSegmentOrientation.HORIZONTAL)
					{
						var splitPosition = mousePosition.X;

						segment1 = new WireSegment(hoveredWire, hoveredSegment.Start, new Vector2(splitPosition, hoveredSegment.End.Y));
						segment2 = new WireSegment(hoveredWire, new Vector2(splitPosition, hoveredSegment.End.Y), hoveredSegment.End);
					}
					else
					{
						var splitPosition = mousePosition.Y;

						segment1 = new WireSegment(hoveredWire, hoveredSegment.Start, new Vector2(hoveredSegment.End.X, splitPosition));
						segment2 = new WireSegment(hoveredWire, new Vector2(hoveredSegment.End.X, splitPosition), hoveredSegment.End);
					}

					hoveredWire.Segments.RemoveAt(index);
					hoveredWire.Segments.Insert(index, segment1);
					hoveredWire.Segments.Insert(index + 1, segment2);
				}
				catch (ArgumentException e)
				{
					Console.WriteLine(e.Message);
				}
			}

			if (draggedSegment == null)
			{
				foreach (var component in puzzle.Components)
				{
					foreach (var port in component.Ports)
					{
						var position = port.GridPosition;
						if (mousePosition == position)
						{
							hoveredPort = port;

							if (selectedPort == null && puzzle.game.inputManager.IsLeftMouseButtonPressed() && !port.IsConnected)
							{
								StartConnectingPorts(port);
							}
						}
					}
				}
			}

			if (selectedPort != null)
			{
				if (puzzle.game.inputManager.IsLeftMouseButtonReleased())
				{
					StopConnectingPorts();
				}
			}
		}

		void StartDraggingSegment(WireSegment segment)
		{
			draggedSegment = segment;

			newDraggedSegmentEndpoints = (segment.Start, segment.End);
		}

		void StopDraggingSegment(bool shouldReset = false)
		{
			// if released when not dragged, then we don't care, reset
			if (shouldReset || !newDraggedSegmentEndpoints.HasValue || (draggedSegment.Start == newDraggedSegmentEndpoints.Value.Start && draggedSegment.End == newDraggedSegmentEndpoints.Value.End))
			{
				draggedSegment = null;
				newDraggedSegmentEndpoints = null;
				return;
			}

			if (draggedSegment.PreviousSegment != null && draggedSegment.PreviousSegment.Orientation == draggedSegment.Orientation)
			{
				// add new segment between
				var newSegment = new WireSegment(draggedSegment.Wire, draggedSegment.PreviousSegment.End, newDraggedSegmentEndpoints.Value.Start);
				draggedSegment.Wire.Segments.Insert(draggedSegment.Wire.Segments.IndexOf(draggedSegment), newSegment);
				
			}

			if (draggedSegment.NextSegment != null && draggedSegment.NextSegment.Orientation == draggedSegment.Orientation)
			{
				// add new segment between
				var newSegment = new WireSegment(draggedSegment.Wire, newDraggedSegmentEndpoints.Value.End, draggedSegment.NextSegment.Start);
				draggedSegment.Wire.Segments.Insert(draggedSegment.Wire.Segments.IndexOf(draggedSegment) + 1, newSegment);				
			}

			if (draggedSegment.PreviousSegment != null) draggedSegment.PreviousSegment.End = newDraggedSegmentEndpoints.Value.Start;
			if (draggedSegment.NextSegment != null) draggedSegment.NextSegment.Start = newDraggedSegmentEndpoints.Value.End;

			draggedSegment.Start = newDraggedSegmentEndpoints.Value.Start;
			draggedSegment.End = newDraggedSegmentEndpoints.Value.End;

			// remove 0-length segments
			var wire = draggedSegment.Wire;
			if (draggedSegment.PreviousSegment != null && draggedSegment.PreviousSegment.Length == 0) wire.Segments.Remove(draggedSegment.PreviousSegment);
			if (draggedSegment.NextSegment != null && draggedSegment.NextSegment.Length == 0) wire.Segments.Remove(draggedSegment.NextSegment);

			// consolidate co-linear segments
			var newSegments = wire.Segments;
			var didConsolidateThisTurn = false;

			do
			{
				var tempSegments = new List<WireSegment>();
				didConsolidateThisTurn = false;

				foreach (var segment in newSegments)
				{
					if (segment.Length == 0)
					{
						didConsolidateThisTurn = true;
						continue;
					}

					var lastSegment = tempSegments.LastOrDefault();
					if (lastSegment != null && lastSegment.Orientation == segment.Orientation && lastSegment.End == segment.Start)
					{
						didConsolidateThisTurn = true;
						lastSegment.End = segment.End;
					}
					else
					{
						tempSegments.Add(segment);
					}
				}

				newSegments = tempSegments;
			} while (didConsolidateThisTurn);
			
			wire.Segments = newSegments;

			if (wire.Segments.Count == 0)
			{
				wire.Delete();
			}
			else
			{
				// relocate ComponentPorts to match the new locations
				foreach (var input in wire.Inputs) input.Position = wire.Segments.First().Start;
				foreach (var input in wire.Outputs) input.Position = wire.Segments.Last().End;
				// connect any ComponentPorts which are in the same location as others

				if (wire.Segments.Count == 0) wire.Delete();

				wire.AutomaticallyConnectPorts();
			}

			// reset dragged state
			draggedSegment = null;
			newDraggedSegmentEndpoints = null;
		}

		void StartConnectingPorts(ComponentPort port)
		{
			selectedPort = port;
		}

		void StopConnectingPorts()
		{
			var mousePosition = puzzle.GetClampedMousePositionInGridSpace();

			var allConnectablePorts = new List<ComponentPort>();
			foreach (var component in puzzle.Components)
			{
				foreach (var otherPort in component.Ports)
				{
					if (otherPort.ResourceType == selectedPort.ResourceType && otherPort.Type != selectedPort.Type)
					{
						allConnectablePorts.Add(otherPort);
					}
				}
			}

			foreach (var otherPort in allConnectablePorts)
			{
				if (otherPort.GridPosition == mousePosition)
				{
					AddWireBetweenPorts(selectedPort, otherPort);
				}
			}

			selectedPort = null;
		}

		void AddWireBetweenPorts(ComponentPort from, ComponentPort to)
		{
			var actualFrom = from.Type == PortType.OUTPUT ? from : to;
			var actualTo = to.Type == PortType.INPUT ? to : from;

			var wire = new Wire(puzzle, actualFrom, actualTo);

			puzzle.AddComponent(wire);

			wire.Initialise();
		}

		void DrawAllComponentPorts(SpriteBatch spriteBatch)
		{
			var gridSize = new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight);

			var circleSize = 5f;

			var min = circleSize * 0.7f;
			var max = circleSize;
			var step = 1f;

			foreach (var component in puzzle.Components)
			{
				component.DrawEditor(spriteBatch);

				if (component is PuzzleComponentWithPosition)
				{
					var componentWithPosition = component as PuzzleComponentWithPosition;
					var screenBounds = componentWithPosition.GetScreenBounds();

					if (selectedPort == null || selectedPort.Type == PortType.OUTPUT)
					{
						foreach (var port in component.Inputs)
						{
							if (selectedPort != null && (port.IsConnected || port.ResourceType != selectedPort.ResourceType)) continue;

							var color = Color.GreenYellow * (port.IsConnected ? 1f : 0.5f);
							if (port == hoveredPort) color = Color.Gold;

							for (var i = min; i <= max; i += step)
							{
								MonoGame.Primitives2D.DrawCircle(spriteBatch, screenBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
							}
						}
					}

					if (selectedPort == null || selectedPort.Type == PortType.INPUT)
					{
						foreach (var port in component.Outputs)
						{
							if (selectedPort != null && (port.IsConnected || port.ResourceType != selectedPort.ResourceType)) continue;

							var color = Color.CornflowerBlue * (port.IsConnected ? 1f : 0.5f);
							if (port == hoveredPort) color = Color.Gold;

							for (var i = min; i <= max; i += step)
							{
								MonoGame.Primitives2D.DrawCircle(spriteBatch, screenBounds.Location.ToVector2() + (port.Position * gridSize) + (gridSize / 2f), i, 16, color);
							}
						}
					}		
				}
			}

			if (selectedPort != null)
			{
				var screenBounds = ((PuzzleComponentWithPosition)selectedPort.Component).GetScreenBounds();

				var portCentre = screenBounds.Location.ToVector2() + (selectedPort.Position * gridSize) + (gridSize / 2f);

				for (var i = min; i <= max; i += step)
				{
					MonoGame.Primitives2D.DrawCircle(spriteBatch, portCentre, i, 16, Color.HotPink);
				}

				MonoGame.Primitives2D.DrawLine(spriteBatch, portCentre, puzzle.game.inputManager.MousePosition, Color.HotPink);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if (draggedSegment == null && hoveredWire != null)
			{
				foreach (var segment in hoveredWire.Segments)
				{
					// highlight wire in grey, unless segment is movable in which case we'll highlight in yellow later
					if (!(segment == hoveredSegment && !segment.IsTerminal)) {
						DrawSegmentHighlight(spriteBatch, segment, Color.White * 0.2f);
					}
				}
			}

			if (draggedSegment == null && hoveredSegment != null)
			{
				// draw box around the wire segment
				// we do some funky math here because drawing rects with negative width/height looked strange

				var sin = (float)Math.Sin((wireHoverTimer / wireHoverTimerMax) * Math.PI * 2);
				var normalised = (sin + 1) / 2f;
				var min = 0.2f;
				var max = 0.5f;

				var alpha = min + ((max - min) * normalised);

				DrawSegmentHighlight(spriteBatch, hoveredSegment, Color.Gold * alpha);

				var (start, end) = GetSegmentEndpointsInScreenSpace(hoveredSegment);
			}

			if (draggedSegment != null)
			{
				if (draggedSegment.PreviousSegment != null) DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, draggedSegment.PreviousSegment.End, newDraggedSegmentEndpoints.Value.Start, Color.Cyan * 0.2f);
				DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, newDraggedSegmentEndpoints.Value.Start, newDraggedSegmentEndpoints.Value.End, Color.Gold * 0.6f);
				if (draggedSegment.NextSegment != null) DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, newDraggedSegmentEndpoints.Value.End, draggedSegment.NextSegment.Start, Color.Magenta * 0.2f);
			}

			//foreach(var wire in this.puzzle.Wires)
			//{
			//	foreach(var segment in wire.Segments)
			//	{
			//		if (segment.IsFirst) continue;
			//		DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, segment.Start, segment.Start, Color.Cyan * 0.5f);
			//	}
			//}

			DrawAllComponentPorts(spriteBatch);
		}

		(Vector2 Start, Vector2 end) GetSegmentEndpointsInScreenSpace(WireSegment segment)
		{
			return (
				puzzle.display.ConvertPuzzleSpaceToScreenSpace(puzzle.grid.ConvertGridSpaceToPuzzleSpace(segment.Start)),
				puzzle.display.ConvertPuzzleSpaceToScreenSpace(puzzle.grid.ConvertGridSpaceToPuzzleSpace(segment.End))
			);
		}

		void DrawSegmentHighlight(SpriteBatch spriteBatch, WireSegment segment, Color color)
		{
			DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, segment.Start, segment.End, color);
		}

		void DrawGridAlignedRectangleGivenTopLeftAndBottomRight(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
		{
			var trueStart = new Vector2(
				Math.Min(start.X, end.X),
				Math.Min(start.Y, end.Y)
			);

			var trueEnd = new Vector2(
				Math.Max(start.X, end.X),
				Math.Max(start.Y, end.Y)
			);

			var puzzleStart = puzzle.grid.ConvertGridSpaceToPuzzleSpace(trueStart);
			var puzzleEnd = puzzle.grid.ConvertGridSpaceToPuzzleSpace(trueEnd);

			var adjustedPuzzleEnd = new Vector2(
				puzzleEnd.X + puzzle.grid.CharacterWidth,
				puzzleEnd.Y + puzzle.grid.CharacterHeight
			);

			var drawStart = puzzle.display.ConvertPuzzleSpaceToScreenSpace(puzzleStart);
			var drawEnd = puzzle.display.ConvertPuzzleSpaceToScreenSpace(adjustedPuzzleEnd);

			var rect = new Rectangle(
				(int)drawStart.X,
				(int)drawStart.Y,
				(int)(drawEnd.X - drawStart.X),
				(int)(drawEnd.Y - drawStart.Y)
			);

			MonoGame.Primitives2D.FillRectangle(spriteBatch, rect, color);
		}

		public override void DrawUI()
		{
			base.DrawUI();
		}
	}
}
