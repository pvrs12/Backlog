using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backlog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IObserver<BacklogItem>
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static void SetBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        public void writeSettings()
        {
            System.IO.File.WriteAllText(settingsFile, taskFile + System.Environment.NewLine + actual + System.Environment.NewLine + estimated);
        }

        public void readSettings()
        {
            if (!System.IO.Directory.Exists(folder))
            {
                //if no folder, make it
                System.IO.Directory.CreateDirectory(folder);
            }
            if (!System.IO.File.Exists(settingsFile))
            {
                System.IO.File.WriteAllText(settingsFile, taskFile + System.Environment.NewLine + actual + System.Environment.NewLine + estimated);
            }
            string s = System.IO.File.ReadAllText(settingsFile);
            string[] spl = { System.Environment.NewLine };
            string[] ss = s.Split(spl, StringSplitOptions.RemoveEmptyEntries);
            taskFile = ss[0];
            actual = int.Parse(ss[1]);
            estimated = int.Parse(ss[2]);

            updateVelocity();
        }

        public void writeTaskFile(List<BacklogItem> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BacklogItem bi in items)
            {
                sb.AppendLine(bi.ToString());
            }
            System.IO.File.WriteAllText(taskFile, sb.ToString());
        }

        public void readTaskFile(List<BacklogItem> list)
        {
            list.Clear();
            //no file, make it
            if (!System.IO.File.Exists(taskFile))
            {
                writeTaskFile(list);
            }
            foreach (String line in System.IO.File.ReadLines(taskFile))
            {
                BacklogItem bi = new BacklogItem(line);
                if (!bi.Completed)
                {
                    bi.Subscribe(this);
                    list.Add(bi);
                }
            }
            list.Sort();
        }

        List<BacklogItem> list = new List<BacklogItem>();
        Boolean quit = false;
        NotifyIcon icon = new NotifyIcon();
        System.Windows.Forms.ContextMenu iconCM = new System.Windows.Forms.ContextMenu();

        static String folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar + @"Backlog";
        static String settingsFile = folder + System.IO.Path.DirectorySeparatorChar + @"settings.txt";
        static String taskFile = folder + System.IO.Path.DirectorySeparatorChar + @"taskfile.txt";

        static int actual = 0;
        static int estimated = 0;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (o, e) =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                this.Left = corner.X - this.ActualWidth;
                this.Top = workingArea.Top;

                this.Height = workingArea.Height;

                this.ShowInTaskbar = false;

                iconCM.MenuItems.Add("Choose Task File");
                iconCM.MenuItems.Add("Quit");
                iconCM.MenuItems[0].Click += TaskFile_Click;
                iconCM.MenuItems[1].Click += Quit_Click;

                icon.Visible = true;
                icon.ContextMenu = iconCM;
                Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Icon1.ico")).Stream;
                icon.Icon = new System.Drawing.Icon(iconStream);

                readSettings();
                readTaskFile(list);
                BacklogList.ItemsSource = list;

            };
        }

        //add item to list
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BacklogItem bi = BacklogItemDialog.CreateNewBacklogItem();
            if (bi == null)
            {
                return;
            }
            bi.CheckPastDue();
            list.Add(bi);
            bi.Subscribe(this);
            list.Sort();
            writeTaskFile(list);

            BacklogList.Items.Refresh();
        }

        private void Window_SetBottom(object sender, EventArgs e)
        {
            SetBottom(this);
            foreach (BacklogItem bi in list)
            {
                bi.CheckPastDue();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !quit;
        }

        private void TaskFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = folder;
            ofd.ShowDialog();
            if (ofd.FileName.Equals(""))
            {
                return;
            }
            taskFile = ofd.FileName;
            writeSettings();
            readTaskFile(list);

            BacklogList.Items.Refresh();
        }

        private void updateVelocity()
        {
            VelocityTextBlock.Text = actual + " : " + estimated;
            writeSettings();
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            quit = true;
            icon.Visible = false;
            writeTaskFile(list);
            writeSettings();
            this.Close();
        }

        //called when a task is edited
        public void OnCompleted()
        {
            //called after edit
            //forces refresh
            list.Sort();
            BacklogList.Items.Refresh();

            writeTaskFile(list);
        }

        public void OnError(Exception error)
        {
        }

        //called when a task is completed
        public void OnNext(BacklogItem value)
        {
            try
            {
                estimated += int.Parse(value.TimeEstimateBlock.Text);
                actual += value.ActualTime;

                //update the velocity
                updateVelocity();
            }
            catch (Exception)
            {
                //do nothing because no estimate was given
                //dont want to include actual either because
                //it would skew velocity
            }

            list.Sort();
            BacklogList.Items.Refresh();
            writeTaskFile(list);
        }

        private void BacklogList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BacklogList.SelectedIndex = -1;
            e.Handled = true;
        }
    }
}
