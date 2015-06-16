using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool SetWindowPlacement(IntPtr hWnd, uint wp);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        const UInt32 SW_SHOWNORMAL = 0x0001;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static void SetBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        public static void Restore(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPlacement(hWnd, SW_SHOWNORMAL);
        }

        public static void writeSettings()
        {
            System.IO.File.WriteAllText(settingsFile, taskFile);
        }

        public static void readSettings()
        {
            if (!System.IO.File.Exists(settingsFile))
            {
                System.IO.File.WriteAllText(settingsFile,taskFile);
            }
            taskFile = System.IO.File.ReadAllText(settingsFile);
        }

        public static void writeTaskFile(List<BacklogItem> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BacklogItem bi in items)
            {
                sb.AppendLine(bi.ToString());
            }
            System.IO.File.WriteAllText(taskFile, sb.ToString());
        }

        public static void readTaskFile(List<BacklogItem> list)
        {
            list.Clear();
            foreach (String line in System.IO.File.ReadLines(taskFile))
            {
                list.Add(new BacklogItem(line));
            }
            list.Sort();
        }

        List<BacklogItem> list = new List<BacklogItem>();
        Boolean quit = false;
        NotifyIcon icon = new NotifyIcon();
        System.Windows.Forms.ContextMenu iconCM = new System.Windows.Forms.ContextMenu();

        static String folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\Backlog";
        static String settingsFile = folder + @"\settings.txt";
        static String taskFile = folder + @"\taskfile.txt";

        public MainWindow()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;

            iconCM.MenuItems.Add("Choose Task File");
            iconCM.MenuItems.Add("Quit");
            iconCM.MenuItems[0].Click += TaskFile_Click;
            iconCM.MenuItems[1].Click += Quit_Click;

            icon.Visible = true;
            icon.ContextMenu = iconCM;
            icon.Icon = new System.Drawing.Icon(@"C:\Windows\System32\PerfCenterCpl.ico");

            readSettings();
            readTaskFile(list);
            BacklogList.ItemsSource = list;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BacklogItem bi = BacklogItemDialog.CreateNewBacklogItem();
            if (bi == null)
            {
                return;
            }
            if (bi.Date.ToUniversalTime() < DateTime.UtcNow)
            {
                //color red
                bi.PastDue = true;
            }
            list.Add(bi);
            list.Sort();
            writeTaskFile(list);

            BacklogList.Items.Refresh();
        }

        private void Window_SetBottom(object sender, EventArgs e)
        {
            SetBottom(this);
            foreach (BacklogItem bi in list)
            {
                if (bi.Date.ToUniversalTime() < DateTime.UtcNow)
                {
                    //color red
                    bi.PastDue = true;
                }
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

        private void Quit_Click(object sender, EventArgs e)
        {
            quit = true;
            icon.Visible = false;
            writeTaskFile(list);
            this.Close();
        }
        
    }
}
