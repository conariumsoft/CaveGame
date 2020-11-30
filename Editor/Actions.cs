using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Editor.Actions
{
    // Interface and class definitions.
    
    public class ActionStack
    {
        private Stack<IAction> UndoStack;
        private Stack<IAction> RedoStack;

        public ActionStack()
        {
            UndoStack = new Stack<IAction>();
            RedoStack = new Stack<IAction>();
        }
        
        public void AddAction(IAction Action)
        {
            UndoStack.Push(Action);
            RedoStack.Clear();
        }

        public void Undo()
        {
            if (UndoStack.Count > 0)
            {
                IAction Action = UndoStack.Pop();
                RedoStack.Push(Action);
                Action.Undo();
            }
        }

        public void Redo()
        {
            if (RedoStack.Count > 0)
            {
                IAction Action = RedoStack.Pop();
                UndoStack.Push(Action);
                Action.Redo();
            }
        }
    }
    
    /// <summary>
    /// A interface for Actions.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Undoes the action. Self explanitory.
        /// </summary>
        void Undo();
        /// <summary>
        /// Redoes the action. Self explanitory.
        /// </summary>
        void Redo();
    }
    
    // Actions
    public class TileChangeAction : IAction
    {
        private Layer StructureLayer;
        private Point Position;
        private Tile tile;
        private Tile PreviousTile;
        
        public TileChangeAction(Layer _StructureLayer, Point _Position, Tile _tile)
        {
            StructureLayer = _StructureLayer;
            Position = _Position;
            tile = _tile;
            PreviousTile = _StructureLayer.Tiles[Position.X, Position.Y];
            _StructureLayer.Tiles[Position.X, Position.Y] = _tile;
        }

        public void Undo()
        {
            StructureLayer.Tiles[Position.X, Position.Y] = PreviousTile;
        }

        public void Redo()
        {
            StructureLayer.Tiles[Position.X, Position.Y] = tile;
        }
    }

	public class WallChangeAction : IAction
	{
        private Layer structureLayer;
        private Point position;
        private Wall wall;
        private Wall previousWall;
        public WallChangeAction(Layer _strLayer, Point pos, Wall donaldTrump)
		{
            wall = donaldTrump;
            structureLayer = _strLayer;
            position = pos;
            previousWall = structureLayer.Walls[pos.X, pos.Y];
            structureLayer.Walls[pos.X, pos.Y] = wall;
		}
		public void Redo()
		{
            structureLayer.Walls[position.X, position.Y] = wall;
		}

		public void Undo()
		{
            structureLayer.Walls[position.X, position.Y] = previousWall;
        }
	}
    public class StructureResizeAction : IAction
    {
        Point oldSize;
        Point newSize;
        StructureFile structure;
        StructureMetadata oldMetadata;
        StructureMetadata newMetadata;
        Tile[,] oldTileGrid;
        Wall[,] oldWallGrid;
        Tile[,] newTileGrid;
        Wall[,] newWallGrid;

        public StructureResizeAction(StructureFile file, StructureMetadata _newMD, Point _newSize)
        {
            oldTileGrid = file.Layers[0].Tiles;
            oldWallGrid = file.Layers[0].Walls;

            oldSize = new Point(file.Metadata.Width, file.Metadata.Height);
            oldMetadata = file.Metadata;
            structure = file;

            newSize = _newSize;
            newMetadata = _newMD;
            structure.Metadata = newMetadata;
            structure.Layers[0].Tiles = new Tile[newSize.X, newSize.Y];
            structure.Layers[0].Walls = new Wall[newSize.X, newSize.Y];
            for (int x=0; x<newSize.X;x++)
            {
                for (int y = 0; y < newSize.Y; y++)
                {
                    structure.Layers[0].Tiles[x, y] = new CaveGame.Core.Game.Tiles.Air();
                    structure.Layers[0].Walls[x, y] = new CaveGame.Core.Game.Walls.Air();
                }
            }
            // initialzize new size

            int smallerX = Math.Min(oldSize.X, newSize.X);
            int smallerY = Math.Min(oldSize.Y, newSize.Y);

            for (int x = 0; x < smallerX; x++)
            {
                for (int y = 0; y < smallerY; y++)
                {
                    structure.Layers[0].Tiles[x, y] = oldTileGrid[x, y];
                    structure.Layers[0].Walls[x, y] = oldWallGrid[x, y];
                }
            }
            newTileGrid = structure.Layers[0].Tiles;
            newWallGrid = structure.Layers[0].Walls;

        }

        public void Redo()
        {
            structure.Metadata = newMetadata;
            structure.Layers[0].Tiles = newTileGrid;
            structure.Layers[0].Walls = newWallGrid;
        }

        public void Undo()
        {
            structure.Metadata = oldMetadata;
            structure.Layers[0].Tiles = oldTileGrid;
            structure.Layers[0].Walls = oldWallGrid;
            
        }
    }
}
