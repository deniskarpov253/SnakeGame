using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows;

namespace SnakeGame
{
    public class InputDialog : Window
    {
        private TextBox inputBox;
        public string ResponseText { get; private set; }
        public InputDialog(string question)
        {
            Title = "Имя игрока";
            Width = 300;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            StackPanel panel = new StackPanel();
            TextBlock text = new TextBlock { Text = question, Margin = new Thickness(10) };
            inputBox = new TextBox { Margin = new Thickness(10) };
            Button okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(10) };

            okButton.Click += (s, e) => { ResponseText = inputBox.Text; DialogResult = true; };
            panel.Children.Add(text);
            panel.Children.Add(inputBox);
            panel.Children.Add(okButton);
            Content = panel;
        }
    }

}
