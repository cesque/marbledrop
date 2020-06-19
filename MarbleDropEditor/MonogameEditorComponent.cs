using Microsoft.Xna.Framework;
using MonoGame.Forms.Controls;
using MonoGame.Forms.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarbleDrop;
using MarbleDrop.Puzzles;
using MarbleDrop.Rendering;
using System.Text.Json;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace MarbleDropEditor
{
	class MonogameEditorComponent : MonoGameControl
	{
		public JsonElement puzzle;

		public GraphicsDevice graphics;
		public SpriteBatch spriteBatch;

		public Form1 Form;

		string levelPath = @"E:\Code\C#\MarbleDrop\MarbleDrop\Content\level1.json";

		List<ComponentDefinition> componentDefinitions = new List<ComponentDefinition>
		{
			new ComponentDefinition {
				Type = "wire",
				Ports = new List<ComponentPortDefinition>(),
			},  
		   
			new ComponentDefinition {
				Type = "playerspawner",
				Width = 4,
				Height = 4,
				Ports = new List<ComponentPortDefinition>
				{
					{ new ComponentPortDefinition {
						Name = "marble/output",
						ResourceType = "marble",
						Type = "output",
						X = 4,
						Y = 2,
					}},
				},
			},
			
			new ComponentDefinition {
				Type = "buffer",
				Width = 5,
				Height = 3,
				Ports = new List<ComponentPortDefinition>
				{
					{ new ComponentPortDefinition {
						Name = "marble/input",
						ResourceType = "marble",
						Type = "input",
						X = -1,
						Y = 1,
					}},
					{ new ComponentPortDefinition {
						Name = "marble/output",
						ResourceType = "marble",
						Type = "output",
						X = 5,
						Y = 1,
					}},
					{ new ComponentPortDefinition {
						Name = "spark/top",
						ResourceType = "spark",
						Type = "output",
						X = 2,
						Y = -1,
					}},
					{ new ComponentPortDefinition {
						Name = "spark/bottom",
						ResourceType = "spark",
						Type = "output",
						X = 2,
						Y = 3,
					}},
				},
			},

			new ComponentDefinition {
				Type = "switch",
				Width = 2,
				Height = 3,
				Ports = new List<ComponentPortDefinition>
				{
					{ new ComponentPortDefinition {
						Name = "marble/input",
						ResourceType = "marble",
						Type = "input",
						X = -1,
						Y = 1,
					}},
					{ new ComponentPortDefinition {
						Name = "marble/top",
						ResourceType = "marble",
						Type = "output",
						X = 0,
						Y = -1,
					}},
					{ new ComponentPortDefinition {
						Name = "marble/bottom",
						ResourceType = "marble",
						Type = "output",
						X = 0,
						Y = 3,
					}},
					{ new ComponentPortDefinition {
						Name = "spark/input",
						ResourceType = "spark",
						Type = "output",
						X = 2,
						Y = 1,
					}},
				},
			},

			new ComponentDefinition {
				Type = "switch",
				Width = 2,
				Height = 3,
				Ports = new List<ComponentPortDefinition>
				{
					{ new ComponentPortDefinition {
						Name = "marble/input",
						ResourceType = "marble",
						Type = "input",
						X = -1,
						Y = 1,
					}},
					{ new ComponentPortDefinition {
						Name = "marble/top",
						ResourceType = "marble",
						Type = "output",
						X = 0,
						Y = -1,
					}},
					{ new ComponentPortDefinition {
						Name = "marble/bottom",
						ResourceType = "marble",
						Type = "output",
						X = 0,
						Y = 3,
					}},
					{ new ComponentPortDefinition {
						Name = "spark/input",
						ResourceType = "spark",
						Type = "output",
						X = 2,
						Y = 1,
					}},
				},
			},
		};

		int characterSize = 16;

		public MonogameEditorComponent()
		{

		}


		protected override void Initialize()
		{
			base.Initialize();

			graphics = Editor.graphics;
			spriteBatch = Editor.spriteBatch;

			var file = File.ReadAllText(levelPath);

			puzzle = JsonDocument.Parse(file).RootElement;

			Form.Log(file);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}
		protected override void Draw()
		{
			base.Draw();

			spriteBatch.Begin();

			graphics.Clear(Color.Black);

			var gridColor = new Color(30, 39, 46);

			for(var x = 0; x < Editor.graphics.Viewport.Width; x += characterSize)
			{
				spriteBatch.Draw(Editor.Pixel, new Rectangle(x, 0, 1, graphics.Viewport.Height), gridColor);
			}
			
			for(var y = 0; y < Editor.graphics.Viewport.Height; y += characterSize)
			{
				spriteBatch.Draw(Editor.Pixel, new Rectangle(0, y, graphics.Viewport.Width, 1), gridColor);
			}

			var components = puzzle.GetProperty("components").EnumerateArray();

			foreach (var component in components)
			{
				var type = component.GetProperty("type").GetString();

				if (type == "wire")
				{
					DrawWire(component);
				}
				else
				{
					DrawComponent(component);
				}
			}

			Editor.spriteBatch.End();

		}

		// draw a normal component that has position and size
		public void DrawComponent(JsonElement element)
		{
			var definition = componentDefinitions.Find(c => c.Type == element.GetProperty("type").GetString());

			var data = element.GetProperty("data");

			var position = data.GetProperty("position");
			var x = position.GetProperty("x").GetInt32();
			var y = position.GetProperty("y").GetInt32();

			spriteBatch.Draw(Editor.Pixel, new Rectangle(
				x * characterSize,
				y * characterSize, 
				definition.Width.Value * characterSize, 
				definition.Height.Value * characterSize
			), Color.White);
		}

		public void DrawWire(JsonElement element)
		{

		}
	}
}
