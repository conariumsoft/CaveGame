using CaveGame.Core.FileUtil;
using CaveGame.Core.Tiles;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Editor
{
	public delegate void NewStructureFileHandler(StructureMetadata md);

	public static class WPFEventBridge
	{
		public static event RoutedEventHandler OnMenuOpen;
		public static event RoutedEventHandler OnMenuNew;
		public static event RoutedEventHandler OnMenuSave;
		public static event RoutedEventHandler OnMenuSaveAs;
		public static event RoutedEventHandler OnMenuDies;
		public static event NewStructureFileHandler OnNewFile;
		public static event MouseEventHandler OnMouseMove;
		public static event MouseEventHandler OnMouseClick;
		public static event MouseWheelEventHandler OnMouseWheel;

		public static void NewFile(StructureMetadata meta)
		{
			OnNewFile?.Invoke(meta);
		}
	}

    public partial class MainWindow : Window
    {

		MainWindowViewModel viewModel = new MainWindowViewModel();

        public MainWindow()
        {

            InitializeComponent();
			Debug.WriteLine( this.DataContext.GetType() );
		}
		

		private void Menu_Open(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Structure files (*.structure)|*.structure|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
				
			}

		}

		private void Menu_New(object sender, RoutedEventArgs e)
		{
			NewFileDialog dialog = new NewFileDialog();

			dialog.Owner = this;
			dialog.ShowDialog();
		}

		private void Menu_About(object sender, RoutedEventArgs e) { }

		private void Menu_Save(object sender, RoutedEventArgs e) { }
		private void Menu_SaveAs(object sender, RoutedEventArgs e) { }
		private void Menu_Exit(object sender, RoutedEventArgs e) { }
		private void Menu_Undo(object sender, RoutedEventArgs e) { }
		private void Menu_Redo(object sender, RoutedEventArgs e) { }
		private void Menu_Copy(object sender, RoutedEventArgs e) { }
		private void Menu_Cut(object sender, RoutedEventArgs e) { }
		private void Menu_Delete(object sender, RoutedEventArgs e) { }
		private void Menu_Paste(object sender, RoutedEventArgs e) { }
		private void Menu_SelectAll(object sender, RoutedEventArgs e) { }

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{

		}


		private void OnMouseEnter(object sender, MouseEventArgs e)
		{
			
		}

		private void OnMouseLeave(object sender, MouseEventArgs e)
		{

			
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			
		}
		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			// If the mouse wheel delta is positive, move the box up.
			if (e.Delta > 0)
			{
				if (Canvas.GetTop(box) >= 1)
				{
					Canvas.SetTop(box, Canvas.GetTop(box) - 1);
				}
			}

			// If the mouse wheel delta is negative, move the box down.
			if (e.Delta < 0)
			{
				if ((Canvas.GetTop(box) + box.Height) <= (MainCanvas.Height))
				{
					Canvas.SetTop(box, Canvas.GetTop(box) + 1);
				}
			}
		}
	}
}
