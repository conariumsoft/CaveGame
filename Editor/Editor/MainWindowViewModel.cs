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
using System.Windows.Input;
using System.Windows.Controls;

namespace Editor
{


    public static class EditorTypeExtensions
	{

        public static Vector2 ToVector2(this System.Windows.Point point)
		{
            return new Vector2((float)point.X, (float)point.Y);
		}

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

            foreach (Layer layer in structure.Layers)
			{
                if (layer.Visible)
                {
                    for (int x = 0; x < structure.Metadata.Width; x++)
                    {
                        for (int y = 0; y < structure.Metadata.Height; y++)
                        {
                            Wall wall = layer.Walls[x, y];
                            Tile tile = layer.Tiles[x, y];


                            wall.Draw(MainWindowViewModel.TileSheet, sb, x, y, new Light3(16, 16, 16));
                            tile.Draw(MainWindowViewModel.TileSheet, sb, x, y, new Light3(16, 16, 16));

                        }
                    }
                }
			}
        }
    }

    public interface I_MGCCInput
	{
        void MGCC_KeyDown(object sender, KeyEventArgs e);
        void MGCC_KeyUp(object sender, KeyEventArgs e);
        void MGCC_MouseWheel(object sender, MouseWheelEventArgs e);
        void MGCC_MouseDoubleClick(object sender, MouseButtonEventArgs e);
        void MGCC_MouseDown(object sender, MouseButtonEventArgs e);
        void MGCC_MouseMove(object sender, MouseEventArgs e);
        void MGCC_MouseLeave(object sender, MouseEventArgs e);
        void MGCC_MouseEnter(object sender, MouseEventArgs e);
        void MGCC_MouseUp(object sender, MouseButtonEventArgs e);
	}


    public class MainWindowViewModel : MonoGameViewModel, I_MGCCInput
    {
        public bool ShiftDown { get; set; }
        public bool MousePanning { get; set; }
        public bool EditorFocused { get; set; }

        public Color BackgroundColor => new Color(10, 10, 10);
        public Color UnfocusedBackground => new Color(25, 25, 25);
        public Color GridLineColor => new Color(45, 45, 45);

        public static Texture2D TileSheet;
        public static Texture2D Pixel;

        public StructureFile LoadedStructure { get; set; }

        public Camera2D Camera { get; private set; }

        float _cameraZoom = 1.0f;

        public MainWindowViewModel() : base()
        {

        }

        private SpriteBatch _spriteBatch;
        private Texture2D _texture;
        private Vector2 _position;
        private float _rotation;
        private Vector2 _origin;
        private Vector2 _scale;


		public override void Initialize()
		{
            base.Initialize();


            Camera = new Camera2D(GraphicsDevice.Viewport) { Zoom = 0.5f };
            
		}



		public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _texture = Content.Load<Texture2D>("monogame-logo");
            TileSheet = Content.Load<Texture2D>("Textures/tilesheet");
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData<Color>(new Color[] { Color.White });
        }

        public override void Update(GameTime gameTime)
        {

            Camera.Zoom = Camera.Zoom.Lerp(_cameraZoom, 0.3f);
           
            _position = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
            _rotation = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) / 4f;
            _origin = _texture.Bounds.Center.ToVector2();
            _scale = Vector2.One;
        }

        private void DrawGridLines(SpriteBatch spriteBatch)
        {


            for (int x = 0; x < LoadedStructure.Metadata.Width; x++)
            {
                spriteBatch.Line(GridLineColor, new Vector2(
                    x * (Globals.TileSize),
                    0
                ), new Vector2(
                    x * ( Globals.TileSize),
                    LoadedStructure.Metadata.Height * (Globals.TileSize)
                ), 0.5f);
            }

            for (int y = 0; y < LoadedStructure.Metadata.Height; y++)
            {
                spriteBatch.Line(GridLineColor, new Vector2(
                    0,
                    y *  Globals.TileSize
                ), new Vector2(
                    LoadedStructure.Metadata.Width * ( Globals.TileSize),
                    y *  Globals.TileSize
                ), 0.5f);
            }

        }

        public override void Draw(GameTime gameTime)
        {
            if (EditorFocused)
                GraphicsDevice.Clear(BackgroundColor);
            else
                GraphicsDevice.Clear(UnfocusedBackground);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
            if (LoadedStructure != null)
			{
                DrawGridLines(_spriteBatch);
                LoadedStructure.Draw(_spriteBatch);
                DrawGridLines(_spriteBatch);
            }

            
            _spriteBatch.Draw(TileSheet, new Vector2(100, 100), TileMap.Brick, Color.Red);
            _spriteBatch.End();
        }

		public void MGCC_KeyDown(object sender, KeyEventArgs e)
		{
		    if (e.Key == Key.LeftShift) { ShiftDown = true; }	
		}
        public void MGCC_KeyUp(object sender, KeyEventArgs e)
		{
            if (e.Key == Key.LeftShift) { ShiftDown = false; }
        }


        public void MGCC_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            if (ShiftDown || MousePanning)
			{
                _cameraZoom += (e.Delta / 1000.0f);
            }
            
		}

		public void MGCC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			
		}

		public void MGCC_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Right)
			{
                MousePanning = true;
			}
		}

        System.Windows.Point lastmousepoint = new System.Windows.Point(0, 0);
		public void MGCC_MouseMove(object sender, MouseEventArgs e)
		{
            var point = e.GetPosition((System.Windows.IInputElement)sender);
            if (MousePanning)
			{
                var lastWorld = Camera.ScreenToWorldCoordinates(lastmousepoint.ToVector2());

                var diff = Camera.ScreenToWorldCoordinates(point.ToVector2()) - lastWorld;

                Camera.Position -= new Vector2(diff.X, diff.Y);
			}
            lastmousepoint = point;

        }

		public void MGCC_MouseLeave(object sender, MouseEventArgs e)
		{
            EditorFocused = false;
        }

		public void MGCC_MouseEnter(object sender, MouseEventArgs e)
		{
            EditorFocused = true;

        }

		public void MGCC_MouseUp(object sender, MouseButtonEventArgs e)
		{
            if (e.ChangedButton == MouseButton.Right)
            {
                MousePanning = false;
            }
        }
       
	}
}