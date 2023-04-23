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

			if (draggedSegment != null && puzzle.game.inputManager.IsRightMouseButtonReleased())
			{
				StopDraggingSegment(true);
			}

			if (draggedSegment != null && puzzle.game.inputManager.IsLeftMouseButtonReleased())
			{
				StopDraggingSegment();
			}

			var mousePosition = puzzle.GetClampedMousePositionInGridSpace();

			foreach (var wire in puzzle.Wires)
			{
				foreach(var segment in wire.Segments)
				{
					var characters = segment.GetCharacters();

					if (characters.Any(character => mousePosition.X == character.Position.X && mousePosition.Y == character.Position.Y))
					{
						// mouse is over wire
						hoveredWire = wire;
						hoveredSegment = segment;

						if (puzzle.game.inputManager.IsLeftMouseButtonPressed() && wire.Segments.Count > 1)
						{
							StartDraggingSegment(segment);
						}
					}
				}
			}

			if (draggedSegment != null)
			{
				if (draggedSegment.Orientation == WireSegmentOrientation.VERTICAL)
				{
					var start = new Vector2(mousePosition.X, draggedSegment.IsFirst ? mousePosition.Y : draggedSegment.Start.Y);
					var end = new Vector2(mousePosition.X, draggedSegment.IsLast ? mousePosition.Y : draggedSegment.End.Y);

					newDraggedSegmentEndpoints = (start, end);
				}
				else if (draggedSegment.Orientation == WireSegmentOrientation.HORIZONTAL)
				{
					var start = new Vector2(draggedSegment.IsFirst ? mousePosition.X : draggedSegment.Start.X, mousePosition.Y);
					var end = new Vector2(draggedSegment.IsLast ? mousePosition.X : draggedSegment.End.X, mousePosition.Y);

					newDraggedSegmentEndpoints = (start, end);
				}
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
		}

		void StartDraggingSegment(WireSegment segment)
		{
			draggedSegment = segment;

			newDraggedSegmentEndpoints = (segment.Start, segment.End);

			foreach (var port in segment.Wire.Ports)
			{
				port.Disconnect();
			}
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
			var tempSegments = new List<WireSegment>();
			var didConsolidateThisTurn = false;

			do
			{
				tempSegments = new List<WireSegment>();
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

		void DrawAllComponentPorts(SpriteBatch spriteBatch)
		{
			foreach (var component in puzzle.Components)
			{
				component.DrawEditor(spriteBatch);

				if (component is PuzzleComponentWithPosition)
				{
					var componentWithPosition = component as PuzzleComponentWithPosition;
					var screenBounds = componentWithPosition.GetScreenBounds();

					var gridSize = new Vector2(puzzle.grid.CharacterWidth, puzzle.grid.CharacterHeight);
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
