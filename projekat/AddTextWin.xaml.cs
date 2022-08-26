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
    /// Interaction logic for AddTextWin.xaml
    /// </summary>
    public partial class AddTextWin : Window
    {
        public AddTextWin()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.drawOrCancel = "cancel";

            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(fontSizeTb.Text.Trim()) || string.IsNullOrEmpty(textTb.Text.Trim()) || cmbColors.SelectedColor == null)
            {
                MessageBox.Show("Fields cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (!double.TryParse(fontSizeTb.Text.ToString(), out MainWindow.textFont))
                {
                    MessageBox.Show("Font size must be number!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MainWindow.drawOrCancel = "draw";

                    this.Close();
                }
            }
        }
        private void CmbColors_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            MainWindow.fillColor = (Color)cmbColors.SelectedColor;
        }

        private void textTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.textShow = textTb.Text;
        }

        private void fontSizeTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.textFont = Convert.ToDouble(fontSizeTb.Text);
        }
    }
}
