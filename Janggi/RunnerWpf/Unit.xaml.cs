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

namespace RunnerWpf
{
	/// <summary>
	/// Interaction logic for Unit.xaml
	/// </summary>
	public partial class Unit : UserControl
	{
		public Unit()
		{
			InitializeComponent();
			SizeChanged += Unit_SizeChanged;
		}

		private void Unit_SizeChanged(object sender, SizeChangedEventArgs e)
		{

		}
	}
}
