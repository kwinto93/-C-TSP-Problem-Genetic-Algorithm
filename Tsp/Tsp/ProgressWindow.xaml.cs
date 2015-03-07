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
using System.Windows.Threading;
using Tsp.Controllers;

namespace Tsp
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void ListenChangesFrom(LoadTspFileController loadTspFile)
        {
            loadTspFile.OnLoadingProgressChangedEvent += loadTspFile_OnLoadingProgressChangedEvent;
            loadTspFile.OnLoadingStateChangedEvent += loadTspFile_OnLoadingStateChangedEvent;
        }


        void loadTspFile_OnLoadingStateChangedEvent(string change)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => Status.Text = change));
        }

        void loadTspFile_OnLoadingProgressChangedEvent(int progress)
        {
             this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => LoadProgress.Value = progress));
        }
    }
}
