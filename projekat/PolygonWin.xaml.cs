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

namespace projekat
{
    /// <summary>
    /// Interaction logic for PolygonWin.xaml
    /// </summary>
    public partial class PolygonWin : Window
    {
        public PolygonWin()
        {
            InitializeComponent();
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(thicknessTb.Text.Trim()) || borderColors.SelectedColor == null || cmbColors.SelectedColor == null)
            {
                MessageBox.Show("Fields cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (!double.TryParse(thicknessTb.Text.ToString(), out MainWindow.borderThickness))
                {
                    MessageBox.Show("Border Thickness must be number!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MainWindow.drawOrCancel = "draw";

                    this.Close();
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.drawOrCancel = "cancel";

            this.Close();
        }
        private void CmbColors_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            MainWindow.fillColor = (Color)cmbColors.SelectedColor;
        }

        private void BorderColors_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            MainWindow.borderColor = (Color)borderColors.SelectedColor;
        }
    }
}
