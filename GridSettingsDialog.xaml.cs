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
using System.Windows.Shapes;

namespace DataGridSerialization
{
    /// <summary>
    /// Interaction logic for GridSettingsDialog.xaml
    /// </summary>
    public partial class GridSettingsDialog : Window
    {
        public GridSettingsDialog()
        {
            InitializeComponent();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}
