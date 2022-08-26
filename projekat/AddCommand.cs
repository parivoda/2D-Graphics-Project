using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace projekat
{
    public class AddCommand : ICommand
    {
        Canvas myCanvas;
        FrameworkElement myElement;

        public AddCommand(Canvas canvas, FrameworkElement element)
        {
            myCanvas = canvas;
            myElement = element;
        }

        public void Execute()
        {
            myCanvas.Children.Add(myElement);
        }

        public void Unexecute()
        {
            myCanvas.Children.Remove(myElement);
        }
    }
}
