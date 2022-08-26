using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace projekat
{
    public class ClearCommand : ICommand
    {
        Canvas can;
        List<FrameworkElement> _elements;
        public ClearCommand(Canvas canvas, List<FrameworkElement> elements)
        {
            can = canvas;
            _elements = elements;
        }

        public void Execute()
        {
            can.Children.Clear();
        }


        public void Unexecute()
        {
            foreach (var item in _elements)
            {
                if (!can.Children.Contains(item))
                    can.Children.Add(item);
            }
        }
    }
}
