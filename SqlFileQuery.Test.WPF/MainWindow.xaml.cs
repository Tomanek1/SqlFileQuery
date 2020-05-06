using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SqlFileQueryLib.Test.WPF;

namespace SqlFileQueryLib.Test.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		SqlDirectDML sqlDirect = new SqlDirectDML();
		SqlDirectDemo sqlDirectDemo = new SqlDirectDemo();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Btn_SelectScalar_Click(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.SelectScalar();
		}

		private void Btn_SelectTable_Click(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.SelectTable();
		}

		private void Btb_SelectGrid_Click(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.SelectGrid();
		}

		private void Btn_InsertSingle_Click(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.InsertSingle();
		}

		private void Btn_InsertMultiple_Click(object sender, RoutedEventArgs e)
		{

			var n = sqlDirect.InsertMultiple();
		}

		private void Btn_Update_Click(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.Update();

		}

		private void Btn_Delete_Click(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.Delete();
		}

		private void Btn_DemoMotivation_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var n = sqlDirectDemo.GetDemo();
			}
			catch (Exception ex)
			{
				string exx = ex.ToString();
				throw;
			}

		}
	}
}
