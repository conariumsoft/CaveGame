using CaveGame.Core.FileUtil;
using CaveGame.Core.Tiles;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor
{

    public partial class MainWindow : Window, IMainWindow
    {

		MainWindowViewModel _vm;


		public MainWindowViewModel ViewModel
		{
			get { if (_vm == null) { _vm = new MainWindowViewModel(); } return _vm; }
			set => _vm = value;
		}

        public MainWindow()
        {

            InitializeComponent();
			
			DataContext = ViewModel;
		}

		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}

		public void GetNewStructure(StructureMetadata md)
		{
			
			var LoadedStructure = new StructureFile(md);
			Layer brug = new Layer(LoadedStructure) { LayerID = "ZOG", Visible = true };
			for (int x = 0; x < md.Width; x++)
			{
				for (int y = 0; y < md.Height; y++)
				{
					brug.Tiles[x, y] = new CaveGame.Core.Tiles.Dirt();
				}
			}
			for (int x = 0; x < md.Width; x++)
			{
				for (int y = 0; y < md.Height; y++)
				{
					brug.Walls[x, y] = new CaveGame.Core.Walls.Stone();
				}
			}
			LoadedStructure.Layers.Add(brug);
			ViewModel.LoadedStructure = LoadedStructure;
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
			NewFileDialog dialog = new NewFileDialog(this);

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

		#region MonoGameContentControls Input->ViewModel
		private void MonoGameContentControl_MouseEnter(object sender, MouseEventArgs e)
		{
			ViewModel.MGCC_MouseEnter(sender, e);
		}

		private void MonoGameContentControl_MouseLeave(object sender, MouseEventArgs e)
		{
			ViewModel.MGCC_MouseLeave(sender, e);
		}

		private void MonoGameContentControl_MouseUp(object sender, MouseButtonEventArgs e)
		{
			ViewModel.MGCC_MouseUp(sender, e);
		}

		private void MonoGameContentControl_MouseMove(object sender, MouseEventArgs e)
		{
			ViewModel.MGCC_MouseMove(sender, e);
		}

		private void MonoGameContentControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ViewModel.MGCC_MouseDown(sender, e);
		}

		private void MonoGameContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ViewModel.MGCC_MouseDoubleClick(sender, e);
		}

		private void MonoGameContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ViewModel.MGCC_MouseWheel(sender, e);
		}

		private void MonoGameContentControl_KeyDown(object sender, KeyEventArgs e)
		{
			ViewModel.MGCC_KeyDown(sender, e);
		}

		private void MonoGameContentControl_KeyUp(object sender, KeyEventArgs e)
		{
			ViewModel.MGCC_KeyU
		}
		#endregion



	}
}
