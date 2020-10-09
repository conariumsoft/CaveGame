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
        public static void Rect(this SpriteBatch sb, Color color, int x, int y, int width, int height, float rotation = 0)
        {
            sb.Draw(
                MainWindowViewModel.Pixel,
                new Rectangle(x, y, width, height),
                null,
                color, rotation, new Vector2(0, 0), SpriteEffects.None, 0
            );
            // retardretardretardretardretardretard
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

    public enum EditorActivity
	{
        EditTile,
        EditWall
	}


    public class MainWindowViewModel : MonoGameViewModel, I_MGCCInput
    {
        // Data Bindings
        public string TileDisplayInfo { get; set; }
        public string FPSDisplayInfo { get; set; }
        public string StructureDisplayInfo { get; set; }

        public System.Windows.Point MousePosition { get; set; }
        System.Windows.Point LastMousePosition = new System.Windows.Point(0, 0);
        public bool LeftMouseDown { get; set; }
        public bool ShiftDown { get; set; }
        public bool CtrlDown { get; set; }
        public bool MousePanning { get; set; }
        public bool EditorFocused { get; set; }

        protected int _selectedtile;
        public int SelectedTile { 
            get { if (CtrlDown) { return 0; } return _selectedtile; }
            set { _selectedtile = value; }
        }
        public int SelectedWall { get; set; }

        public EditorActivity LayerActivity { get; set; }

        public Color BackgroundColor => new Color(10, 10, 10);
        public Color UnfocusedBackground => new Color(25, 25, 25);
        public Color GridLineColor => new Color(45, 45, 45);

        public static Texture2D TileSheet;
        public static Texture2D Pixel;
        public static SpriteFont Arial10;

        public StructureFile LoadedStructure { get; set; }

        public Camera2D Camera { get; private set; }

        float _cameraZoom = 1.0f;

        public MainWindowViewModel() : base()
        {
            TileDisplayInfo = "";
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
            int selectedTile = SelectedTile;
            if (CtrlDown)
                selectedTile = 0;


            var type = typeof(TileDefinitions);
            var members = type.GetFields();
            byte finalID = (byte)(selectedTile).Mod(members.Length); // TODO: temp hack

            TDef tdef = (TDef)members[finalID].GetValue(null);

            var mp = Camera.ScreenToWorldCoordinates(MousePosition.ToVector2());

            mp /= 8;
            var dp = new Vector2((int)mp.X, (int)mp.Y);
            if (LeftMouseDown && LoadedStructure!=null && dp.X <= LoadedStructure.Metadata.Width && dp.Y<=LoadedStructure.Metadata.Height)
            {
                
                


                LoadedStructure.Layers[0].Tiles[(int)dp.X, (int)dp.Y] = Tile.FromName(members[finalID].Name);
            }

            
            TileDisplayInfo = String.Format("tile {0} {1}", members[finalID].Name, tdef.ID);
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

        private void DrawTileSelectionSet(SpriteBatch sb)
		{
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            for (int i = -20; i<20; i++)
			{
                var type = typeof(TileDefinitions);
                var members = type.GetFields();
                byte finalID = (byte)(SelectedTile + i).Mod(members.Length); // TODO: temp hack

                TDef tdef = (TDef)members[finalID].GetValue(null);

                Vector2 pos = new Vector2((20*24)+10+(i*25), 10);
                if (i == 0)
                {
                    sb.Rect(Color.White * 0.5f, (int)pos.X - 4, (int)pos.Y - 4, 24, 24);
                   
                }
                sb.Draw(TileSheet, pos, tdef.Quad, tdef.Color, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
               
			}
            sb.End();
        }

        private void DrawWallSelectionSet(SpriteBatch sb)
		{
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            for (int i = -20; i < 20; i++)
            {

                var type = typeof(WallDefinitions);
                var members = type.GetFields();
                byte finalID = (byte)(SelectedWall + i).Mod(members.Length); // TODO: temp hack

                Wall tdef = (Wall)members[finalID].GetValue(null);

                Vector2 pos = new Vector2((20 * 24) + 10 + (i * 25), 10);
                if (i == 0)
                {
                    sb.Rect(Color.White * 0.5f, (int)pos.X - 4, (int)pos.Y - 4, 24, 24);
                }
                sb.Draw(TileSheet, pos, tdef.Quad, tdef.Color, 0, Vector2.Zero, 2, SpriteEffects.None, 0);

            }
            sb.End();
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

            if (LayerActivity == EditorActivity.EditTile)
                DrawTileSelectionSet(_spriteBatch);
            else if (LayerActivity == EditorActivity.EditWall)
                DrawWallSelectionSet(_spriteBatch);
        }

		public void MGCC_KeyDown(object sender, KeyEventArgs e)
		{
            CtrlDown = (e.KeyboardDevice.Modifiers == ModifierKeys.Control);
            ShiftDown = (e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
          //  if (e.Key == Key.LeftShift) { ShiftDown = true; }
           // if (e.Key == Key.LeftCtrl) { CtrlDown = true; }
        }
        public void MGCC_KeyUp(object sender, KeyEventArgs e)
		{
            CtrlDown = (e.KeyboardDevice.Modifiers == ModifierKeys.Control);
            ShiftDown = (e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
           // if (e.Key == Key.LeftShift) { ShiftDown = false; }
           // if (e.Key == Key.LeftCtrl) { CtrlDown = false; }
        }


        public void MGCC_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            if (ShiftDown || MousePanning)
			{
                _cameraZoom += (e.Delta / 1000.0f);
            } else
			{
                SelectedTile += (e.Delta/120);
			}
            
		}

		public void MGCC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			
		}

        public void MGCC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                LeftMouseDown = false;
            if (e.ChangedButton == MouseButton.Right)
            {
                MousePanning = false;
            }
        }

        public void MGCC_MouseDown(object sender, MouseButtonEventArgs e)
		{
            if (e.ChangedButton == MouseButton.Left)
                LeftMouseDown = true;
			if (e.ChangedButton == MouseButton.Right)
			{
                MousePanning = true;
			}
		}

        
		public void MGCC_MouseMove(object sender, MouseEventArgs e)
		{
            var point = e.GetPosition((System.Windows.IInputElement)sender);
            LastMousePosition = MousePosition;
            
            MousePosition = point;


            if (MousePanning)
            {
                var lastWorld = Camera.ScreenToWorldCoordinates(LastMousePosition.ToVector2());

                var diff = Camera.ScreenToWorldCoordinates(point.ToVector2()) - lastWorld;

                Camera.Position -= new Vector2(diff.X, diff.Y);
            }




        }

		public void MGCC_MouseLeave(object sender, MouseEventArgs e)
		{
            EditorFocused = false;
        }

		public void MGCC_MouseEnter(object sender, MouseEventArgs e)
		{
            EditorFocused = true;

        }

		
       
	}
}