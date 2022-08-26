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
    /// Interaction logic for ChangeWin.xaml
    /// </summary>
    public partial class ChangeWin : Window
    {
        public ChangeWin()
        {
            InitializeComponent();
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (cmbColors.SelectedColor == null || borderColors.SelectedColor == null || string.IsNullOrEmpty(thicknessTb.Text.Trim()))
            {
                MessageBox.Show("Fields cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (!double.TryParse(thicknessTb.Text.ToString(), out MainWindow.borderThickness))
                {
                    MessageBox.Show("Border Thickness has to be numbers!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MainWindow.drawOrCancel = "change";

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
