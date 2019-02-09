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
using SqlFileQuery.Test.WPF;

namespace SqlFileQuery.Test.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		SqlDirect sqlDirect = new SqlDirect();
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var n = sqlDirect.GetActivePublishers();
			}
			catch (Exception ex)
			{
				string exx = ex.ToString();
				throw;
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			var n = sqlDirect.Get2();
		}
	}
}
