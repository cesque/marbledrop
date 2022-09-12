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
			base.Leave();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			wireHoverTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			wireHoverTimer %= wireHoverTimerMax;

			hoveredWire = null;
			hoveredSegment = null;

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

						if(!segment.IsTerminal && puzzle.game.inputManager.IsLeftMouseButtonPressed())
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
					var start = new Vector2(mousePosition.X, draggedSegment.Start.Y);
					var end = new Vector2(mousePosition.X, draggedSegment.End.Y);

					newDraggedSegmentEndpoints = (start, end);
				}
			}
		}

		void StartDraggingSegment(WireSegment segment)
		{
			draggedSegment = segment;
			newDraggedSegmentEndpoints = (segment.Start, segment.End);
		}

		void StopDraggingSegment()
		{
			draggedSegment.PreviousSegment.End = newDraggedSegmentEndpoints.Value.Start;
			draggedSegment.Start = newDraggedSegmentEndpoints.Value.Start;

			draggedSegment.End = newDraggedSegmentEndpoints.Value.End;
			draggedSegment.NextSegment.Start = newDraggedSegmentEndpoints.Value.End;

			draggedSegment = null;
			newDraggedSegmentEndpoints = null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if(draggedSegment == null && hoveredWire != null)
			{
				foreach (var segment in hoveredWire.Segments)
				{
					// highlight wire in grey, unless segment is movable in which case we'll highlight in yellow later
					if(!(segment == hoveredSegment && !segment.IsTerminal)) {
						DrawSegmentHighlight(spriteBatch, segment, Color.White * 0.2f);
					}
				}
			}

			if (draggedSegment == null && hoveredSegment != null && !hoveredSegment.IsTerminal)
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

				MonoGame.Primitives2D.DrawCircle(spriteBatch, start + puzzle.grid.GridCenterOffset, 3, 16, Color.Magenta);
				MonoGame.Primitives2D.DrawCircle(spriteBatch, end + puzzle.grid.GridCenterOffset, 3, 16, Color.Magenta);
			}

			if(draggedSegment != null)
			{
				DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, draggedSegment.PreviousSegment.End, newDraggedSegmentEndpoints.Value.Start, Color.Cyan * 0.2f);
				DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, newDraggedSegmentEndpoints.Value.Start, newDraggedSegmentEndpoints.Value.End, Color.Gold * 0.6f);
				DrawGridAlignedRectangleGivenTopLeftAndBottomRight(spriteBatch, newDraggedSegmentEndpoints.Value.End, draggedSegment.NextSegment.Start, Color.Magenta * 0.2f);
			}
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
