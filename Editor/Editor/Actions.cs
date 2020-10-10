﻿using CaveGame.Core.FileUtil;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
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
}
