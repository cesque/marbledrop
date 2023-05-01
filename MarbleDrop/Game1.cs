using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text.Json;

using MonoGame.ImGui;

using MarbleDrop.Rendering;
using MarbleDrop.Puzzles;
using System.IO;

namespace MarbleDrop
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		RenderTarget2D renderTarget;
		internal float screenScale = 1f;

		ImGuiRenderer imguiRenderer;

		Grid grid;

		bool isFullScreen = false;

		internal InputManager inputManager;

		Puzzle puzzle;
		PuzzleDisplay puzzleDisplay;
		float timer;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			imguiRenderer = new ImGuiRenderer(this).Initialize().RebuildFontAtlas();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			inputManager = new InputManager(this);

			Globals.DebugTexture = new Texture2D(GraphicsDevice, 1, 1);
			Globals.DebugTexture.SetData<Color>(new Color[] { Color.White });

			// TODO: use this.Content to load your game content here

			grid = new Grid(this, 80, 40);
			var gridSize = grid.GetScreenBounds();
			screenScale = grid.GetMaxScreenScaleToFitOnScreen();

			var displayWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			var displayHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

			graphics.PreferredBackBufferWidth = (int)(gridSize.Width * screenScale);
			graphics.PreferredBackBufferHeight = (int)(gridSize.Height * screenScale);

			if (isFullScreen)
			{
				graphics.IsFullScreen = isFullScreen;

				var scaleX = (float)displayWidth / gridSize.X;
				var scaleY = (float)displayHeight / gridSize.Y;

				graphics.PreferredBackBufferWidth = displayWidth;
				graphics.PreferredBackBufferHeight = displayHeight;

				screenScale = Math.Min(scaleX, scaleY);
			}

			graphics.ApplyChanges();

			renderTarget = new RenderTarget2D(GraphicsDevice, (int)gridSize.Width, (int)gridSize.Height);


			/* --- */

			var file = File.ReadAllText("./Content/level1.json");
			var document = JsonDocument.Parse(file);

			puzzle = Puzzle.FromJSON(this,  document.RootElement);
			puzzleDisplay = new PuzzleDisplay(this, grid, new Rectangle(
				2,
				2,
				(int)(grid.Width * 0.75f),
				(int)(grid.Height - 4)
			));


			puzzleDisplay.Mount(puzzle);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			inputManager.Update();

			timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

			puzzleDisplay.Update(gameTime);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// generate character grid for puzzle display separately
			puzzleDisplay.DrawCharacters();
			// draw puzzle display characters to their own render target (doesn't show anything on screen yet)
			puzzleDisplay.DrawRenderTarget(spriteBatch);

			// clear main render target
			GraphicsDevice.SetRenderTarget(renderTarget);
			GraphicsDevice.Clear(Color.Black);

			// draw base grid, then puzzle display on top of it
			// todo: allow some non-puzzledisplay things to render on TOP of the display somehow?
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			grid.Draw(spriteBatch);
			puzzleDisplay.Draw(spriteBatch);
			// draw editor textures for puzzle
			if (puzzleDisplay.Editor.Enabled)
			{
				puzzleDisplay.DrawEditor(spriteBatch);
			}

			spriteBatch.End();

			// reset render target so we draw to screen now
			GraphicsDevice.SetRenderTarget(null);

			/* draw render target to screen (scaled up by nearest neighbour by screenScale) */
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			spriteBatch.Draw(renderTarget, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
			spriteBatch.End();

			base.Draw(gameTime);

			// draw editor UI elements
			if (puzzleDisplay.Editor.Enabled)
			{
				imguiRenderer.BeginLayout(gameTime);
				ImGuiNET.ImGui.SetMouseCursor(ImGuiNET.ImGuiMouseCursor.Arrow);
				ImGuiNET.ImGui.GetIO().MouseDrawCursor = true;
				puzzleDisplay.DrawEditorUI();
				imguiRenderer.EndLayout();
			}
		}
	}
}
