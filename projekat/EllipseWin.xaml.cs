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
    /// Interaction logic for EllipseWin.xaml
    /// </summary>
    public partial class EllipseWin : Window
    {
        public EllipseWin()
        {
            InitializeComponent();
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(radiusXtb.Text.Trim()) || string.IsNullOrEmpty(radiusYtb.Text.Trim()) || string.IsNullOrEmpty(thicknessTb.Text.Trim()) || cmbColors.SelectedColor == null || borderColors.SelectedColor == null)
            {
                MessageBox.Show("Fields cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (!double.TryParse(radiusXtb.Text.ToString(), out MainWindow.width) || !double.TryParse(radiusYtb.Text.ToString(), out MainWindow.height) || !double.TryParse(thicknessTb.Text.ToString(), out MainWindow.borderThickness))
                {
                    MessageBox.Show("Radius X, Radius Y and Border Thickness must be numbers!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
