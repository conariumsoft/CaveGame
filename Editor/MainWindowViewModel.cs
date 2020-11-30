using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.MonoGameControls;
using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Walls;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Microsoft.Xna.Framework.Input;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Editor.Actions;
using CaveGame.Core.Generic;
using DataManagement;
using System.IO;

namespace Editor
{

    public static class AssetLoader
    {
        private static Texture2D FromStream(GraphicsDevice GraphicsProcessor, Stream DataStream)
        {

            Texture2D Asset = Texture2D.FromStream(GraphicsProcessor, DataStream);
            // Fix Alpha Premultiply
            Color[] data = new Color[Asset.Width * Asset.Height];
            Asset.GetData(data);
            for (int i = 0; i != data.Length; ++i)
                data[i] = Color.FromNonPremultiplied(data[i].ToVector4());
            Asset.SetData(data);
            return Asset;
        }

        public static Texture2D LoadTexture(GraphicsDevice device, string filepath)
        {
            return FromStream(device, TitleContainer.OpenStream(filepath));
        }

    }
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

    public class ResizeAction // TODO: interface
	{
        public void Undo()
		{

		}
        public void Redo()
		{

		}
	}

    public class MainWindowViewModel : MonoGameViewModel, I_MGCCInput, INotifyPropertyChanged
    {

        public GraphicsEngine GFX { get; set; }
        public bool CreatingSelectionBox { get; set; }
        public Rectangle? Selection { get; set; }

        public Point? InitialShiftClickPosition { get; set; }


        private Tile[] internalTileList;
        private Wall[] internalWallList;

		#region BottomBar Bindings
		private string _tileDisplayInfo;
        public string TileDisplayInfo
        {
            get { return _tileDisplayInfo; }
            set { _tileDisplayInfo = value;
                OnPropertyChanged("TileDisplayInfo");
			}
        }

        private string _fpsDisplayInfo;
        public string FPSDisplayInfo
        {
            get { return _fpsDisplayInfo; }
            set { _fpsDisplayInfo = value;
                OnPropertyChanged("FPSDisplayInfo");    
            }
        }

        private string _structureDisplayInfo;
        public string StructureDisplayInfo
        {
            get { return _structureDisplayInfo; }
            set { _structureDisplayInfo = value;
                OnPropertyChanged("StructureDisplayInfo");    
            }
        }
        #endregion

        public System.Windows.Point MousePosition { get; set; }
        System.Windows.Point LastMousePosition = new System.Windows.Point(0, 0);
        public bool LeftMouseDown { get; set; }
        public bool ShiftDown { get; set; }
        public bool CtrlDown { get; set; }

        public bool ZDown { get; set; }
        public bool YDown { get; set; }
        public bool MousePanning { get; set; }
        public bool EditorFocused { get; set; }

		#region Sidebar Bindings
		public bool TilesVisible { get; set; }
        public bool WallsVisible { get; set; }
        public bool AirVisible { get; set; }
        public bool GridVisible { get; set; }
        #endregion

        public int SelectedTile { get; set; }
        public int SelectedWall { get; set; }

        public EditorActivity LayerActivity { get; set; }

        public Color BackgroundColor => new Color(10, 10, 10);
        public Color UnfocusedBackground => new Color(25, 25, 25);
        public Color GridLineColor => new Color(45, 45, 45);
        public int EditorVersion => 2;

        public static Texture2D TileSheet;
        public static Texture2D Pixel;
        public static SpriteFont Arial10;

        private StructureFile _struct;
        public StructureFile LoadedStructure
        {
            get { return _struct; }
            set
            {
                _struct = value;
                StructureDisplayInfo = String.Format(
                    "file: {0} w:{1} h:{2} author:{3} notes[{4}] layers: {5}",
                    _struct.Metadata.Name, _struct.Metadata.Width,_struct.Metadata.Height,
                    _struct.Metadata.Author, _struct.Metadata.Notes, _struct.Layers.Count
                );
            }
        }

        public Camera2D Camera { get; private set; }

        float _cameraZoom = 1.0f;

        public int TileCount { get; private set; }
        public int WallCount { get; private set; }

        DelayedTask updateBarTask;

        private void LoadTileInformation()
		{
            var basetype = typeof(Tile);
            var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));

            TileCount = 0;
            foreach (var type in types)
            {

               
                bool exists = Enum.TryParse(type.Name, out TileID id);
                if (exists && type.Name != "Void")
                {
                    Trace.WriteLine(type.Name + " exists: "+exists);
                    TileCount++;
                }

            }
            internalTileList = new Tile[TileCount];
            foreach (var type in types)
            {
                bool exists = Enum.TryParse(type.Name, out TileID id);
                if (exists && type.Name != "Void")
                {
                    Tile t = (Tile)type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    internalTileList[t.ID] = t;
                }
            }
        }

        private void LoadWallInformation()
		{
            var basetype = typeof(Wall);
            var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));

            WallCount = 0;
            
            foreach (var type in types)
            {
                Trace.WriteLine(type.Name);
                bool exists = Enum.TryParse(type.Name, out WallID id);
                if (exists && type.Name != "Void")
                {
                    WallCount++;
                }

            }
            internalWallList = new Wall[WallCount + 1];
            foreach (var type in types)
            {
                bool exists = Enum.TryParse(type.Name, out WallID id);
                if (exists && type.Name != "Void")
                {
                    Wall w = (Wall)type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    internalWallList[w.ID] = w;
                }
            }
        }

        public MainWindowViewModel() : base()
        {
            TilesVisible = true;
            WallsVisible = true;
            GridVisible = true;
            updateBarTask = new DelayedTask(UpdateBar, (1 / 10.0f));
            TileDisplayInfo = "Awaiting Data";
            FPSDisplayInfo = "Awaiting FPS";
            StructureDisplayInfo = "Awaiting Structure";

            GFX = new GraphicsEngine();

            LoadTileInformation();
            LoadWallInformation();
        }

        private void UpdateBar()
		{
            if (LayerActivity == EditorActivity.EditTile)
			{
                var t = internalTileList[GetSelectedTileID()];
                Point mouse = GetMouseGridPoint();
                TileDisplayInfo = String.Format("<tile>selected {0}:{1} {2}, lookat: {3}:{4} {5}, mouse: {6},{7}", t.Namespace, t.TileName, t.ID, 0, 0, 0, mouse.X, mouse.Y, 0);
            } else
			{
                var w = internalWallList[GetSelectedWallID()];
                TileDisplayInfo = String.Format("<wall>selected {0}:{1} {2}, lookat: {3}:{4} {5}, mouse: {6},{7}", w.Namespace, w.WallName, w.ID, 0, 0, 0, 0, 0);
            }
            
            FPSDisplayInfo = "...";
        }

        private SpriteBatch _spriteBatch;

        


        public override void Initialize()
		{
            base.Initialize();


            Camera = new Camera2D(GraphicsDevice.Viewport) { Zoom = 0.5f };
            
		}


		public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            GFX.ContentManager = Content;
            GFX.GraphicsDevice = GraphicsDevice;
            //GFX.GraphicsDeviceManager = GraphicsDev;

            TileSheet = AssetLoader.LoadTexture(GraphicsDevice, "tilesheet.png");
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData<Color>(new Color[] { Color.White });

            GFX.LoadFonts = false;
            GFX.Initialize();
            GFX.LoadAssets(GraphicsDevice);
        }

        private int GetSelectedTileID(int index)
		{
            byte finalID = (byte)(index).Mod(TileCount); // TODO: temp hack
            return finalID;
        }

        private int GetSelectedTileID()
		{
            int tid = SelectedTile;

            byte finalID = (byte)(tid).Mod(TileCount); // TODO: temp hack
            return finalID;
        }

        private int GetSelectedWallID(int index)
		{
            byte finalID = (byte)(index).Mod(WallCount); // TODO: temp hack
            return finalID;
        }

        private int GetSelectedWallID()
		{
            int tid = SelectedWall;

            byte finalID = (byte)(tid).Mod(WallCount); // TODO: temp hack
            return finalID;
        }


        ActionStack Actions = new ActionStack();

        public void ActionUndo() => Actions.Undo(); 
        public void ActionCopySelection() { }
        public void ActionCutSelection() { }
        public void ActionPasteSelection() { }
        public void ActionRedo() => Actions.Redo();
        public void ActionResizeStructure(StructureMetadata newMetadata) => Actions.AddAction(new StructureResizeAction(LoadedStructure, newMetadata, new Point(newMetadata.Width, newMetadata.Height)));
        private Point GetMouseGridPoint()
		{
            var mp = Camera.ScreenToWorldCoordinates(MousePosition.ToVector2());

            int x = (int)Math.Floor(mp.X / 8);
            int y = (int)Math.Floor(mp.Y / 8);

            
            return new Point(x, y);
            
        }

        private bool IsMouseOnGrid()
		{
            Point mp = GetMouseGridPoint();
            if (LoadedStructure == null)
                return false;
            return (mp.X >= 0 && mp.Y >= 0 && mp.X < LoadedStructure.Metadata.Width && mp.Y < LoadedStructure.Metadata.Height);

        }


        public override void Update(GameTime gameTime)
        {
            GFX.Update(gameTime);

            if (!GFX.ContentLoaded)
                return;

            updateBarTask.Update(gameTime);
            Camera.Update(gameTime);
            var mouse = GetMouseGridPoint();


            if (LeftMouseDown && LoadedStructure!=null && IsMouseOnGrid())
            {
                if (ShiftDown)
                {
                    if (InitialShiftClickPosition == null)
                        InitialShiftClickPosition = mouse;


                    Selection = new Rectangle((Point)InitialShiftClickPosition, mouse- ( (Point)InitialShiftClickPosition - new Point(1, 1)) );
                } else
                {
                    if (Selection == null)
                    {
                        if (LayerActivity == EditorActivity.EditTile)
                        {

                            Tile newTile = Tile.FromID(internalTileList[GetSelectedTileID()].ID);


                            if (CtrlDown)
                                newTile = new CaveGame.Core.Game.Tiles.Void();

                            if (LoadedStructure.Layers[0].Tiles[mouse.X, mouse.Y].GetType() != newTile.GetType())
                                Actions.AddAction(new TileChangeAction(LoadedStructure.Layers[0], mouse, newTile));

                        }
                        else
                        {
                            Wall newWall = Wall.FromID(internalWallList[GetSelectedWallID()].ID);
                            if (CtrlDown)
                                newWall = new CaveGame.Core.Game.Walls.Void();

                            if (LoadedStructure.Layers[0].Walls[mouse.X, mouse.Y].GetType() != newWall.GetType())
                                Actions.AddAction(new WallChangeAction(LoadedStructure.Layers[0], mouse, newWall));

                        }
                    }

                    InitialShiftClickPosition = null;
                    Selection = null;
                }    
            }

            if (CtrlDown == true && ZDown == true && ShiftDown == false)
            {
                if (undoD == false)
                {
                    Actions.Undo();
                    undoD = true;
                }
               
            } else
            {
                undoD = false;
            }

            
           
            if (CtrlDown == true && YDown == true)
            {
                if (redoD == false)
                {
                    Actions.Redo();
                    redoD = true;
                }
            } else
            {
                redoD = false;
            }
         

            Camera.Zoom = Camera.Zoom.Lerp(_cameraZoom, 0.3f);
           
        }

        bool undoD;
        bool redoD;

        private void DrawGridLines(GraphicsEngine GFX)
        {


            for (int x = 0; x < LoadedStructure.Metadata.Width; x++)
            {
                GFX.Line(GridLineColor, new Vector2(
                    x * (Globals.TileSize),
                    0
                ), new Vector2(
                    x * ( Globals.TileSize),
                    LoadedStructure.Metadata.Height * (Globals.TileSize)
                ), 0.5f);
            }

            for (int y = 0; y < LoadedStructure.Metadata.Height; y++)
            {
                GFX.Line(GridLineColor, new Vector2(
                    0,
                    y *  Globals.TileSize
                ), new Vector2(
                    LoadedStructure.Metadata.Width * ( Globals.TileSize),
                    y *  Globals.TileSize
                ), 0.5f);
            }

        }

        public void DrawStructure(StructureFile structure, GraphicsEngine GFX)
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

                            if (WallsVisible && (wall.ID!=0||AirVisible))
                                wall.Draw(GFX, x, y, new Light3(16, 16, 16));
                            if (TilesVisible && (tile.ID != 0 || AirVisible))
                                tile.Draw(GFX, x, y, new Light3(16, 16, 16));
                        }
                    }
                }
            }
        }


        private void DrawTileSelectionSet(GraphicsEngine GFX)
		{
            GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            for (int i = -20; i<20; i++)
			{

                Tile t = internalTileList[GetSelectedTileID(SelectedTile+i)];

                Vector2 pos = new Vector2((20*24)+10+(i*25), 10);
                if (i == 0)
                {
                    GFX.Rect(Color.White * 0.5f, new Vector2(pos.X - 4, pos.Y - 4), new Vector2(24, 24));
                   
                }
                GFX.Sprite(GFX.TileSheet, pos, t.Quad, t.Color, Rotation.Zero, Vector2.Zero, 2, SpriteEffects.None, 0);
               
			}
            GFX.End();
        }

        private void DrawWallSelectionSet(GraphicsEngine GFX)
		{
            GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            for (int i = -20; i < 20; i++)
            {

                Wall w = internalWallList[GetSelectedWallID(SelectedWall + i)];

                Vector2 pos = new Vector2((20 * 24) + 10 + (i * 25), 10);
                if (i == 0)
                {
                    GFX.Rect(Color.White * 0.5f, new Vector2(pos.X - 4, pos.Y - 4), new Vector2(24, 24));
                }
                GFX.Sprite(TileSheet, pos, w.Quad, w.Color, Rotation.Zero, Vector2.Zero, 2, SpriteEffects.None, 0);

            }
            GFX.End();
        }

        public override void Draw(GameTime gameTime)
        {

            if (!GFX.ContentLoaded)
                return;

            if (EditorFocused)
                GFX.GraphicsDevice.Clear(BackgroundColor);
            else
                GFX.GraphicsDevice.Clear(UnfocusedBackground);

            GFX.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.View);
            if (LoadedStructure != null)
			{
                if (GridVisible)
                    DrawGridLines(GFX);
                DrawStructure(LoadedStructure, GFX);
                if (GridVisible)
                    DrawGridLines(GFX);


                if (Selection!=null)
                {
                    var selection = (Rectangle)Selection;
                    GFX.Rect(Color.Blue * 0.5f, new Vector2(selection.X * 8, selection.Y * 8), new Vector2(selection.Width * 8, selection.Height * 8));
                }
            }


            GFX.End();

            if (LayerActivity == EditorActivity.EditTile)
                DrawTileSelectionSet(GFX);
            else if (LayerActivity == EditorActivity.EditWall)
                DrawWallSelectionSet(GFX);
        }

		public void MGCC_KeyDown(object sender, KeyEventArgs e)
		{
            if (e.Key == Key.D1)
                LayerActivity = EditorActivity.EditTile;
            if (e.Key == Key.D2)
                LayerActivity = EditorActivity.EditWall;


            if (e.Key == Key.LeftCtrl)
                CtrlDown = true;


            if (e.Key == Key.LeftShift)
                ShiftDown = true;


            if (e.Key == Key.Z)
                ZDown = true;
            if (e.Key == Key.Y)
                YDown = true;

            if (e.Key == Key.Escape)
            {
                InitialShiftClickPosition = null;
                Selection = null;
            }
            


            //  if (e.Key == Key.LeftShift) { ShiftDown = true; }
            // if (e.Key == Key.LeftCtrl) { CtrlDown = true; }

        }
        public void MGCC_KeyUp(object sender, KeyEventArgs e)
		{
            if (e.Key == Key.LeftCtrl)
                CtrlDown = false;


            if (e.Key == Key.LeftShift)
                ShiftDown = false;

            // if (e.Key == Key.LeftShift) { ShiftDown = false; }
            // if (e.Key == Key.LeftCtrl) { CtrlDown = false; }

            if (e.Key == Key.Z)
            {
                ZDown = false;
            }
            if (e.Key == Key.Y)
                YDown = false;

        }


        public void MGCC_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            if (ShiftDown || MousePanning)
			{
                _cameraZoom += (e.Delta / 1000.0f);
            } else
			{
                if (LayerActivity == EditorActivity.EditTile)
                    SelectedTile += (e.Delta/120);
                else
                    SelectedWall += (e.Delta / 120);
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