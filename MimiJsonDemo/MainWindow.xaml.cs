using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using System.Xml.Linq;
using MimiJson;

namespace MimiJsonDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Wrap_OnClick(object sender, RoutedEventArgs e)
        {
            var wrap = ((CheckBox)sender).IsChecked.Value;
            InputBox.TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            OutputBox.TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        private void Parse_OnClick(object sender, RoutedEventArgs e)
        {
            var text = InputBox.Text;
            File.WriteAllText("text.txt", text);
            var json = JsonValue.Parse(text);
            
            OutputBox.Text = json.ToString();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("text.txt"))
                InputBox.Text = File.ReadAllText("text.txt");
        }
    }
}
