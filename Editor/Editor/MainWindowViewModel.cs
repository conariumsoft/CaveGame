using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.MonoGameControls;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Walls;
using CaveGame.Core.Tiles;
using CaveGame.Core;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Microsoft.Xna.Framework.Input;

namespace Editor
{

    public static class EditorTypeExtensions
	{

        // TODO: Fix code duplication between this and Renderer.cs
        public static void Line(this SpriteBatch spriteBatch, Color color, Vector2 point1, Vector2 point2, float thickness = 1f)
        {
            float distance = Vector2.Distance(point1, point2);
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            float expanded = (float)Math.Floor(angle * Math.PI);
            float backDown = expanded / (float)Math.PI;

            Line(spriteBatch, color, point1, distance, angle, thickness);
        }
        public static void Line(this SpriteBatch sb, Color color, Vector2 point, float length, float angle, float thickness = 1f)
        {
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, thickness);
            sb.Draw(MainWindowViewModel.Pixel, point, null, color, angle, origin, scale, SpriteEffects.None, 0);
        }


        public static void Draw(this StructureFile structure, SpriteBatch sb)
        {
            foreach(Layer layer in structure.Layers)
			{
                if (layer.Visible)
				{
                    for (int x = 0; x < structure.Metadata.Width; x++)
                    {
                        for (int y = 0; y < structure.Metadata.Height; y++)
                        {
                            Wall wall = layer.Walls[x, y];
                            Tile tile = layer.Tiles[x, y];


                            wall.Draw(MainWindowViewModel.TileSheet, sb, x * Globals.TileSize, y * Globals.TileSize, new Light3(16, 16, 16));
                            tile.Draw(MainWindowViewModel.TileSheet, sb, x * Globals.TileSize, y * Globals.TileSize, new Light3(16, 16, 16));
                        }
                    }
                }
			}
        }
    }
    public class MainWindowViewModel : MonoGameViewModel
    {
        public Color BackgroundColor => new Color(15, 15, 15);
        public Color GridLineColor => new Color(45, 45, 45);

        public static Texture2D TileSheet;
        public static Texture2D Pixel;

        public StructureFile LoadedStructure { get; set; }

        public Camera2D Camera { get; private set; }

        public MainWindowViewModel() : base()
        {
            WPFEventBridge.OnNewFile += NewFile;
        }

        private SpriteBatch _spriteBatch;
        private Texture2D _texture;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _origin;
        private Vector2 _scale;


        protected void NewFile(StructureMetadata md)
		{
            LoadedStructure = new StructureFile(md);
            Layer brug = new Layer(LoadedStructure) { LayerID = "ZOG" };
            LoadedStructure.Layers.Add(brug);
            brug.Tiles = new CaveGame.Core.Tiles.Tile[md.Width, md.Height];
            for (int x = 0; x < md.Width; x++)
            {
                for (int y = 0; y < md.Height; y++)
                {
                    brug.Tiles[x, y] = new CaveGame.Core.Tiles.Dirt();
                }
            }
            brug.Walls = new CaveGame.Core.Walls.Wall[md.Width, md.Height];
            for (int x = 0; x < md.Width; x++)
            {
                for (int y = 0; y < md.Height; y++)
                {
                    brug.Walls[x, y] = new CaveGame.Core.Walls.Stone();
                }
            }
            brug.Tiles[5, 5] = new CaveGame.Core.Tiles.Stone();
            brug.Tiles[5, 5] = new CaveGame.Core.Tiles.Stone();
            brug.Tiles[5, 6] = new CaveGame.Core.Tiles.Stone();
            brug.Walls[1, 1] = new CaveGame.Core.Walls.Stone();
            LoadedStructure.Save();
            Debug.WriteLine("ASS!!");
        }


		public override void Initialize()
		{
            base.Initialize();


            Camera = new Camera2D(GraphicsDevice.Viewport) { Zoom = 2 };
            
		}



		public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _texture = Content.Load<Texture2D>("monogame-logo");
            TileSheet = Content.Load<Texture2D>("Textures/tilesheet");
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData<Color>(new Color[] { Color.White });
        }

        MouseState lastState;
        public override void Update(GameTime gameTime)
        {
            MouseState state = Mouse.GetState();

            if ((state.RightButton == ButtonState.Pressed) && lastState!=null)
			{
                Debug.WriteLine("YESS");
                int difx = state.X - lastState.X;
                int dify = state.Y - lastState.Y;
                var mp = Camera.ScreenToWorldCoordinates(state.Position.ToVector2());
                var mp2 = Camera.ScreenToWorldCoordinates(lastState.Position.ToVector2());


                Camera.Position = mp;
			}
            lastState = state;


            _position = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
            _rotation = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) / 4f;
            _origin = _texture.Bounds.Center.ToVector2();
            _scale = Vector2.One;
        }

        private void DrawGridLines(SpriteBatch spriteBatch)
        {
            int camChunkX = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.X / (Globals.TileSize));
            int camChunkY = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.Y / ( Globals.TileSize));

            for (int x = 0; x < LoadedStructure.Metadata.Width; x++)
            {
                spriteBatch.Line(GridLineColor, new Vector2(
                    (camChunkX + x) * (Globals.TileSize),
                    (camChunkY) * ( Globals.TileSize)
                ), new Vector2(
                    (camChunkX + x) * ( Globals.TileSize),
                    (camChunkY + LoadedStructure.Metadata.Height) * (Globals.TileSize)
                ), 1);
            }

            for (int y = 0; y < LoadedStructure.Metadata.Height; y++)
            {
                spriteBatch.Line(GridLineColor, new Vector2(
                    (camChunkX) * ( Globals.TileSize),
                    (camChunkY + y) * ( Globals.TileSize)
                ), new Vector2(
                    (camChunkX + LoadedStructure.Metadata.Width) * ( Globals.TileSize),
                    (camChunkY + y) * ( Globals.TileSize)
                ), 1);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
            if (LoadedStructure != null)
			{
                DrawGridLines(_spriteBatch);
                LoadedStructure.Draw(_spriteBatch);
            }

            //_spriteBatch.Draw(_texture, _position, null, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0f);

            
            _spriteBatch.Draw(TileSheet, new Vector2(100, 100), TileMap.Brick, Color.Red);
            _spriteBatch.End();
        }
    }
}