using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CADCore;
using Timer = System.Timers.Timer;

namespace CADView
{
    /// <summary>
    /// Interaction logic for LogoWindow.xaml
    /// </summary>
    public partial class LogoWindow : Window
    {
        public LogoWindow()
        {
            InitializeComponent();
        }

        private class CadTimer : Timer, ITick
        {
            public event CadManagementControl.SystemTickDelegate TickEvent;

            public CadTimer(): base(33)
            {
                Elapsed += (sender, args) => TickEvent?.Invoke((float) Interval);
            }
        }

        readonly ITick tickTimer = new CadTimer();

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            tickTimer.Start();

            Task initTask = new Task(() =>
            {
                CadManagementControl.CreateCADManagement(tickTimer);
                Task.Delay(1000).Wait();
            });

            initTask.Start();
            await initTask;

            new MainWindow().Show();
            Close();
        }
    }
}
