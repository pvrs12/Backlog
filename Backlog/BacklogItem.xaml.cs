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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backlog
{
    /// <summary>
    /// Interaction logic for BacklogItem.xaml
    /// </summary>
    public partial class BacklogItem : UserControl, IComparable<BacklogItem>
    {
        public BacklogItem()
        {
            InitializeComponent();
        }

        public BacklogItem(String s):this()
        {
            String[] ss = s.Replace("\u0002",System.Environment.NewLine).Split('\u0001');

            TitleTextBlock.Text = ss[0];
            NotesTextBlock.Text = ss[1];
            FileNameTextBlock.Text = ss[2];
            DateTextBlock.Text = ss[3];
            Completed = Boolean.Parse(ss[4]);

            CheckPastDue();
        }

        public override String ToString()
        {
            String[] output = {TitleTextBlock.Text,
                                  NotesTextBlock.Text,
                                  FileNameTextBlock.Text,
                                  DateTextBlock.Text,
                                  Completed.ToString()};
            
            return String.Join("\u0001",output).Replace("\n","\u0002").Replace("\r","");
        }

        private Boolean completed=false;

        public Boolean Completed
        {
            set
            {
                if (value)
                {
                    this.Visibility = System.Windows.Visibility.Collapsed;
                }
                completed = value;
            }
            get
            {
                return completed;
            }
        }

        public void CheckPastDue()
        {
            bool bo = Date.ToUniversalTime().AddDays(1) < DateTime.UtcNow;
            if (bo)
            {
                //update color
                Brush b = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                this.Background = b;
            }
        }

        public DateTime Date
        {
            get
            {
                return DateTime.Parse(this.DateTextBlock.Text);
            }
        }

        public int CompareTo(BacklogItem other)
        {
            DateTime d = DateTime.Parse(this.DateTextBlock.Text);
            DateTime od = DateTime.Parse(other.DateTextBlock.Text);

            return d.CompareTo(od);
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            Completed = true;
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(FileNameTextBlock.Text);
            }
            catch (Exception)
            {
            }
        }
    }
}
