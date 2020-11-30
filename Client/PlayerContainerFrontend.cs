using CaveGame.Client.Game.Entities;
using CaveGame.Core;
using CaveGame.Core.Game.Inventory;
using CaveGame.Core.Generic;
using CaveGame.Core.Inventory;
using DataManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Client
{
	public class PlayerContainerFrontend
	{
		public float UIScale { get; set; }
		protected int ItemTextureSize = 16;

		public LocalPlayer Player { get; set; }
		public Vector2 InventoryDrawPosition => new Vector2(400, 30);
		public Vector2 HotbarDrawPosition => new Vector2(400, 30);

		public Container Container => Player?.Inventory;
		public Container MouseHoldContainer = new Container(1, 1);
		public ItemStack MouseHeldItem => MouseHoldContainer.GetItemStack(0, 0);

		public bool InventoryOpen { get; set; }



		public int HotbarIndex { get; set; }

		public bool IsHighlighted { get; set; }
		public Point HighlightedSlot { get; set; }

		public ItemStack EquippedItem { get; set; }

		public PlayerContainerFrontend()
		{
			UIScale = 3;
			EquippedItem = ItemStack.Empty;
		}

		private Vector2 GetSlotIndexPosition(Vector2 containerOffset, Point slotIndex, float uiScale)
        {
			float xpadding = 2*uiScale;
			float ypadding = 2*uiScale;
			float yExtraPadding = 0;

			if (slotIndex.Y >= 1)
				yExtraPadding = 4*uiScale;
			return containerOffset + (slotIndex.ToVector2() * ItemTextureSize * uiScale) + new Vector2(slotIndex.X * xpadding, (slotIndex.Y * ypadding) + yExtraPadding);

		}

		private string GetDisplayStringForItemQuantity(int quantity) => quantity.ToString() + "x";


		private void DrawTextForStack(GraphicsEngine gfx, Vector2 slotPos, float uiScale, int quantity, Color color)
        {
			string displayStr = GetDisplayStringForItemQuantity(quantity);
			Vector2 dimensions = gfx.Fonts.Arial16.MeasureString(displayStr);

			Vector2 offset = new Vector2(ItemTextureSize * uiScale, ItemTextureSize * uiScale) - dimensions;

			//sb.Print(GameFonts.Arial16, Color.Black, slotPos + offset + new Vector2(4, 4), displayStr);
			gfx.Text(gfx.Fonts.Arial16, displayStr, slotPos + offset, color);
			
		}

		private void DrawItemSlot(GraphicsEngine gfx, Vector2 containerOffset, Point position, float uiScale, ItemStack stack, bool bgOverride = false)
		{
			Vector2 slotPos = GetSlotIndexPosition(containerOffset, position, uiScale);

			if (!bgOverride)
				gfx.Sprite(gfx.Slot, slotPos - new Vector2(1, 1), null, Color.Gray, Rotation.Zero, Vector2.Zero, uiScale, SpriteEffects.None, 0);

			if (!stack.Equals(ItemStack.Empty))
			{
				stack.Item?.Draw(gfx, slotPos, uiScale);
				if (stack.Quantity > 1)
					DrawTextForStack(gfx, slotPos, uiScale, stack.Quantity, Color.White);
			}
		}

		private void DrawHighlightedItemSlot(GraphicsEngine gfx, Vector2 containerOffset, Point position, float uiScale, ItemStack stack)
		{
			Vector2 slotPos = GetSlotIndexPosition(containerOffset, position, uiScale);

			gfx.Sprite(gfx.Slot, slotPos-new Vector2(1, 1), null, Color.White, Rotation.Zero, Vector2.Zero, uiScale, SpriteEffects.None, 0);
			if (!stack.Equals(ItemStack.Empty))
			{
				stack.Item?.Draw(gfx, slotPos, UIScale);
				if (stack.Quantity > 1)
					DrawTextForStack(gfx, slotPos, uiScale, stack.Quantity, Color.Black);
			}

		}

		private void DrawOpenInventory(GraphicsEngine gfx)
		{
			// the player's primary inventory

			for (int x = 0; x < Container.Width; x++)
			{
				for (int y = 0; y < Container.Height; y++)
				{
					ItemStack stack = Container.GetItemStack(x, y);

					if (IsHighlighted && HighlightedSlot.X == x && HighlightedSlot.Y == y)
                    {
						DrawHighlightedItemSlot(gfx, InventoryDrawPosition, new Point(x, y), UIScale, stack);
						if (!stack.Equals(ItemStack.Empty))
                        {
							string text = stack.Item.DisplayName;
							Vector2 offset = gfx.Fonts.Arial16.MeasureString(text);
							gfx.Text(gfx.Fonts.Arial16, text, InventoryDrawPosition + new Vector2(-(offset.X+10) , 20));
                        }
					}
					
					else
						DrawItemSlot(gfx, InventoryDrawPosition, new Point(x, y), UIScale, stack);
				}
			}


		}

		private void DrawHotbar(GraphicsEngine gfx)
		{
			// draw top of hotbar
			for (int x = 0; x < Container.Width; x++)
			{
				ItemStack stack = Container.GetItemStack(x, 0);

				if (IsHighlighted && HighlightedSlot.X == x)
					DrawHighlightedItemSlot(gfx, HotbarDrawPosition, new Point(x, 0), UIScale, stack);
				else
					DrawItemSlot(gfx, HotbarDrawPosition, new Point(x, 0), UIScale, stack);

			}
		}

		private void DrawMouseHeldItem(GraphicsEngine gfx)
        {
			MouseState ms = Mouse.GetState();

			DrawItemSlot(gfx, ms.Position.ToVector2(), new Point(0, 0), UIScale, MouseHeldItem, true);
        }

		public void Draw(GraphicsEngine gfx)
		{
			if (Player == null)
				return;

			if (InventoryOpen)
				DrawOpenInventory(gfx);
			else
				DrawHotbar(gfx);


			if (MouseHeldItem!=null)
            {
				DrawMouseHeldItem(gfx);
            }

		}
		private int lastScroll = 0;
		private void HotbarUpdate(GameTime gt)
		{
			MouseState mouse = Mouse.GetState();
			var scroll = (mouse.ScrollWheelValue / 120) - (lastScroll / 120); // why is MouseScroll in 120 increments???
			lastScroll = mouse.ScrollWheelValue;

			HotbarIndex -= scroll;
			HotbarIndex = HotbarIndex.Mod(10);

			IsHighlighted = true;
			HighlightedSlot = new Point(HotbarIndex, 0);
			EquippedItem = Container.GetItemStack(HighlightedSlot.X, 0);
		}

		protected bool IsMouseInside(MouseState mouse, Vector2 position, float size)
		{
			return (mouse.X > position.X && mouse.Y > position.Y
				&& mouse.X < (position.X + size)
				&& mouse.Y < (position.Y + size));
		}
		protected bool IsMouseInside(MouseState mouse, Vector2 position, Vector2 size)
		{
			return (mouse.X > position.X && mouse.Y > position.Y
				&& mouse.X < (position.X + size.X)
				&& mouse.Y < (position.Y + size.Y));
		}



		MouseState previousMouse = Mouse.GetState();
		MouseState currentMouse = Mouse.GetState();
		private bool JustClickedLMB() => (previousMouse.LeftButton != ButtonState.Pressed && currentMouse.LeftButton == ButtonState.Pressed);
		private bool JustClickedRMB() => (previousMouse.RightButton != ButtonState.Pressed && currentMouse.RightButton == ButtonState.Pressed);
		private bool JustReleasedLMB() => (previousMouse.LeftButton == ButtonState.Pressed && currentMouse.LeftButton != ButtonState.Pressed);
		private bool JustReleasedRMB() => (previousMouse.RightButton == ButtonState.Pressed && currentMouse.RightButton != ButtonState.Pressed);
		private void OpenInventoryUpdate(GameTime gt)
		{
			currentMouse = Mouse.GetState();

			if (!IsMouseInside(currentMouse, InventoryDrawPosition, GetSlotIndexPosition(Vector2.Zero, new Point(Container.Width+1, Container.Height+1), UIScale)))
				EquippedItem = MouseHeldItem;

			

			IsHighlighted = false;
			//HighlightedSlot = null;

			for (int x = 0; x < Container.Width; x++)
			{
				for (int y = 0; y < Container.Height; y++)
				{
					Vector2 rectPos = GetSlotIndexPosition(InventoryDrawPosition, new Point(x, y), UIScale);
					if (IsMouseInside(currentMouse, rectPos, ItemTextureSize * UIScale))
                    {
						HighlightedSlot = new Point(x, y);
						IsHighlighted = true;
					}
				}
			}


			if (IsHighlighted)
            {
				
				if (JustClickedLMB())
                {

					ItemStack stackFromContainer = Container.GetItemStack(HighlightedSlot.X, HighlightedSlot.Y);
					ItemStack stackFromMouse = MouseHeldItem;
					

					if (MouseHeldItem.AreCombinable(stackFromContainer))
                    {
						Debug.WriteLine("YEEEEE");
						stackFromContainer.Quantity += stackFromMouse.Quantity;
						MouseHoldContainer.ForceSetSlot(0, 0, ItemStack.Empty);

					} else
                    {
						Debug.WriteLine(stackFromContainer.ToString());
						MouseHoldContainer.ForceSetSlot(0, 0, stackFromContainer);
						Container.ForceSetSlot(HighlightedSlot.X, HighlightedSlot.Y, stackFromMouse);
					}


				}
				if (JustClickedRMB())
                {
					ItemStack stackFromContainer = Container.GetItemStack(HighlightedSlot.X, HighlightedSlot.Y);

					if (!MouseHeldItem.Equals(ItemStack.Empty))
					{
						if (stackFromContainer.Equals(ItemStack.Empty))
                        {
							Container.ForceSetSlot(HighlightedSlot.X, HighlightedSlot.Y, new ItemStack { Item = MouseHeldItem.Item, Quantity = 1 });
							MouseHeldItem.Quantity--;
						} else if (MouseHeldItem.AreCombinable(stackFromContainer))
                        {
							MouseHeldItem.Quantity--;
							stackFromContainer.Quantity++;
						}
						
						
					}

					else if (MouseHeldItem.Equals(ItemStack.Empty))
                    {
						MouseHoldContainer.ForceSetSlot(0, 0, new ItemStack { Item = stackFromContainer.Item, Quantity = 1 });
						stackFromContainer.Quantity--;
					}
					else if (MouseHeldItem.AreCombinable(stackFromContainer))
					{
						MouseHeldItem.Quantity++;
						stackFromContainer.Quantity--;
					}
				}

			}


			previousMouse = currentMouse;

		}

		public void Update(GameTime gt)
		{
			if (Player == null)
				return;

			

			if (InventoryOpen)
				OpenInventoryUpdate(gt);
			else
				HotbarUpdate(gt);

		}
	}
}
