using GitViz.Logic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitViz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowPlacement();
        }

        public void WindowPlacement()
        {
            Top = SystemParameters.WorkArea.Top + 100;
            Left = SystemParameters.WorkArea.Left + 100;

            Width = SystemParameters.WorkArea.Width * .75;
            Height = SystemParameters.WorkArea.Height * .75;
        }

        private void btnChooseRepoPath_Click(object sender, RoutedEventArgs e)
        {
            var configDefaultPath = ConfigurationManager.AppSettings.Get("defaultPath");
            var defaultPath = string.IsNullOrWhiteSpace(configDefaultPath) 
                                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                : configDefaultPath;
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = defaultPath; // Use current value for initial dir
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
            dialog.FileName = "select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                //Remove fake filename from resulting path
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // Our final value is in path
                txtRepoPath.Text = path;
            }
        }

        private void Graph_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var theGraph = (Graphing)DataContext;
            if (!theGraph.IsNewRepository)
                return;
            theGraph.IsNewRepository = false;

            
        }

        

        
    }
}
