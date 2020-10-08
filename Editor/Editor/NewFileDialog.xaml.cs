using CaveGame.Core.FileUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Editor
{
	/// <summary>
	/// Interaction logic for NewFileDialog.xaml
	/// </summary>
	/// 
	public interface IMainWindow {
		void GetNewStructure(StructureMetadata meta);
	}

	public partial class NewFileDialog : Window
	{
		IMainWindow _window;
		public NewFileDialog(IMainWindow window)
		{
			_window = window;
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{

			_window.GetNewStructure(new StructureMetadata { 
				Author = this.authorTextBox.Text,
				Name = this.authorTextBox.Text,
				//Notes = this.authorTextBox.Notes,
				Width = Int32.Parse(this.widthTextBox.Text),
				Height = Int32.Parse(this.heightTextBox.Text),
			});
			this.Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
