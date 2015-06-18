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

namespace Backlog
{
    /// <summary>
    /// Interaction logic for CompletionDialog.xaml
    /// </summary>
    public partial class CompletionDialog : Window
    {
        public CompletionDialog()
        {
            InitializeComponent();
        }

        private void TimeSpentTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Equals(".") && TimeSpentTextBox.Text.Count((c) => { return c == '.'; }) < 1)
            {
                e.Handled = false;
                return;
            }
            double a;
            e.Handled = !Double.TryParse(e.Text, out a);
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
