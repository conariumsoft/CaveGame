using CaveGame.Core;
using CaveGame.Core.LuaInterop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Client.UI
{
	public class SliderChangedEventArgs : LuaEventArgs
    {

    }
	public class Scrubber
	{
		public int Width { get; set; }
	}

	public class IntSlider : Slider<SliderIndex<int>>
    {
		public IntSlider() : base()
        {

        }
    }
	public class FloatSlider : Slider<SliderIndex<float>>
	{
		public FloatSlider() : base()
		{

		}
	}

	public class Slider<T> : UIRect
	{


		public Slider() {
		
		}

		public LuaEvent<SliderChangedEventArgs> SliderChanged = new LuaEvent<SliderChangedEventArgs>();
		private int selectionIndex = 0;

		public T Current { 
			get
			{
				return DataSet[selectionIndex];
			}

		}
		public T[] DataSet { get; set; }
		public Scrubber Scrubber { get; set; }

		

		public int CurrentIndex {
			get { return selectionIndex; }
			set
			{
				if (value != selectionIndex)
				{
					OnValueChanged?.Invoke(this, DataSet[value], selectionIndex);
				}
				selectionIndex = value;

			}
		}

		public void SetIndex(int index)
		{
			selectionIndex = index;
			float absSizeX = AbsoluteSize.X - Scrubber.Width;

			float frac = index / (float)(DataSet.Length - 1);

			var vis2 = (float)Math.Clamp(0, (frac * absSizeX), absSizeX);

			scrubberVisualPos = vis2;
		}

		bool dragging;
		public Color UnselectedBGColor { get; set; }
		public Color SelectedBGColor { get; set; }

		public bool Selected { get; set; }

		private float scrubberVisualPos = 0;


		public delegate void SliderChangedHandler(Slider<T> slider, T value, int index);

		public event SliderChangedHandler OnValueChanged;

		private bool IsMouseInside(MouseState mouse)
		{
			return (mouse.X > AbsolutePosition.X && mouse.Y > AbsolutePosition.Y
				&& mouse.X < (AbsolutePosition.X + AbsoluteSize.X)
				&& mouse.Y < (AbsolutePosition.Y + AbsoluteSize.Y));
		}


		MouseState prevMouse = Mouse.GetState();

		public override void Update(GameTime gt)
		{
			

			MouseState mouse = Mouse.GetState();

			Selected = IsMouseInside(mouse);


			if (Selected && !IsMouseInside(prevMouse))
			{
				GameSounds.MenuBlip?.Play(1.0f, 1, 0.0f);
			}

			if (!Selected && IsMouseInside(prevMouse))
			{
				GameSounds.MenuBlip?.Play(0.8f, 1, 0.0f);
			}

			if (IsMouseInside(mouse) && mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
			{
				dragging = true;
			}
			if (dragging && mouse.LeftButton == ButtonState.Released)
			{
				dragging = false;
			}

			if (dragging) {
				float mouseXDiff = mouse.X - AbsolutePosition.X - (Scrubber.Width/2);


				float absSizeX = AbsoluteSize.X - Scrubber.Width;

				var frac = mouseXDiff / absSizeX;

				frac = Math.Clamp(frac, 0, 1);

				var visual = Math.Round(frac * (DataSet.Length-1) ) / (DataSet.Length-1);
				var vis2 = (float)Math.Clamp(0, (visual* absSizeX), absSizeX);

				scrubberVisualPos = vis2;

				var val = (int)Math.Round(frac * (DataSet.Length-1));
				CurrentIndex = val;
			}


			prevMouse = mouse;


			base.Update(gt);
		}

		public override void Draw(GraphicsEngine GFX)
		{
			base.Draw(GFX);

			Color sliderColor;
			if (Selected)
				sliderColor = SelectedBGColor;
			else
				sliderColor = UnselectedBGColor;
			GFX.Rect(sliderColor, new Vector2(AbsolutePosition.X+ scrubberVisualPos, AbsolutePosition.Y), new Vector2(Scrubber.Width, AbsoluteSize.Y));
			
			
		}
	}
}
