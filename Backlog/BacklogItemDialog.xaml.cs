using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Backlog
{
    /// <summary>
    /// Interaction logic for BacklogItemDialog.xaml
    /// </summary>
    public partial class BacklogItemDialog : Window
    {
        bool canceled = false;

        public static void EditBacklogItem(BacklogItem bi)
        {
            BacklogItemDialog bid = new BacklogItemDialog();
            bid.DateTextBox.Text = bi.Date.ToShortDateString();
            bid.TitleTextBox.Text = bi.TitleTextBlock.Text;
            bid.NotesTextBox.Text = bi.NotesTextBlock.Text;
            bid.FileLocationTextBox.Text = bi.FileNameTextBlock.Text;
            bid.EstimateTextBox.Text = bi.TimeEstimateBlock.Text;

            bid.ShowDialog();
            if (bid.canceled)
            {
                return;
            }

            if (bid.DateTextBox.SelectedDate.HasValue)
            {
                bi.DateTextBlock.Text = bid.DateTextBox.SelectedDate.Value.ToShortDateString();
            }
            else
            {
                System.Windows.MessageBox.Show("You must input a due date!", "Date Missing");
                EditBacklogItem(bi);
                return;
            }
            if (!bid.TitleTextBox.Text.Equals(""))
            {
                bi.TitleTextBlock.Text = bid.TitleTextBox.Text;
            }
            else
            {
                System.Windows.MessageBox.Show("You must input a title!", "Title Missing");
                EditBacklogItem(bi);
                return;
            }
            bi.FileNameTextBlock.Text = bid.FileLocationTextBox.Text;
            bi.NotesTextBlock.Text = bid.NotesTextBox.Text;
            bi.TimeEstimateBlock.Text = bid.EstimateTextBox.Text;
        }

        public static BacklogItem CreateNewBacklogItem()
        {
            BacklogItemDialog bid = new BacklogItemDialog();
            bid.ShowDialog();
            if (bid.canceled)
            {
                return null;
            }

            BacklogItem bi = new BacklogItem();
            if (bid.DateTextBox.SelectedDate.HasValue)
            {
                bi.DateTextBlock.Text = bid.DateTextBox.SelectedDate.Value.ToShortDateString();
            }
            else
            {
                System.Windows.MessageBox.Show("You must input a due date!", "Date Missing");
                return CreateNewBacklogItem();
            }
            if (!bid.TitleTextBox.Text.Equals(""))
            {
                bi.TitleTextBlock.Text = bid.TitleTextBox.Text;
            }
            else
            {
                System.Windows.MessageBox.Show("You must input a title!", "Title Missing");
                return CreateNewBacklogItem();
            }
            bi.FileNameTextBlock.Text = bid.FileLocationTextBox.Text;
            bi.NotesTextBlock.Text = bid.NotesTextBox.Text;
            bi.TimeEstimateBlock.Text = bid.EstimateTextBox.Text;
            return bi;
        }

        public BacklogItemDialog()
        {
            InitializeComponent();

            TitleTextBox.Focus();
        }

        //okay
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            canceled = false;
            this.Close();
        }

        //cancel
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            canceled = true;
            this.Close();
        }

        //browse
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            FileLocationTextBox.Text = ofd.FileName;
        }

        private void EstimateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            if (e.Text.Equals(".") && EstimateTextBox.Text.Count((c) => { return c == '.'; }) < 1)
            {
                e.Handled = false;
                return;
            }
            double a;
            e.Handled = !Double.TryParse(e.Text, out a);
        }
    }
}
