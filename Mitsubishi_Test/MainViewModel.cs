
using Melfa.Robot;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Windows.Input;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace Mitsubishi_Test
{
    public class MainViewModel : BaseViewModel
    {
        MainWindow mainWindow;
        BackgroundWorker DataCheck;
        MelfaRobot robot;
        internal void Loaded(MainWindow mWindow)
        {
            this.mainWindow = mWindow;
            robot = new MelfaRobot("");
            ConnectionStatus = "No controller";
            ConnectButtonContext = "Connect";
            DataCheck = new BackgroundWorker();
            DataCheck.DoWork += DataCheck_DoWork;
            DataCheck.RunWorkerCompleted += DataCheck_RunWorkerCompleted;
            mainWindow.OverrideSlider.PreviewMouseDown += OverrideSlider_MouseDown;
            mainWindow.OverrideSlider.PreviewMouseUp += OverrideSlider_MouseUp;
        }
        bool overrideHold = false;
        private void OverrideSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (robot.IsConnected)
            { 
                var orfk = (byte)Override;
                try
                {
                    robot.Override = orfk;
                }
                catch (Exception ex)
                {

                }
            }
            overrideHold = false;
        }

        private void OverrideSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            overrideHold = true;
        }
        private void DataCheck_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if(robot.IsConnected)
                DataCheck.RunWorkerAsync();
        }
        private void DataCheck_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (robot.IsConnected)
            {
                PositionJ = new PositionJHelper(robot.CurrentPositionJ);
                PositionP = new PositionPHelper(robot.CurrentPositionP);
                if (!overrideHold)
                    Override = robot.Override;
            }
        }
        public int Override { get; set; }
        public string OperationStatus { get; set; }
        public string ConnectionStatus { get; set; }
        public string ConnectButtonContext { get; set; }
        public string ActiveTask { get; set; }
        public string Modules { get; set; }
        public string ProgramPosition { get; set; }
        public string IP { get; set; } = "192.168.0.118";
        public PositionJHelper PositionJ { get; set; }
        public PositionPHelper PositionP { get; set; }
        public ICommand ConnectButtonClick
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (ConnectButtonContext == "Connect" && IsValidIpAddress(IP))
                    {
                        robot = new MelfaRobot(IP);
                        robot.Connect(); var info = robot.Open("ClientName");
                        if (robot.IsConnected)
                        {
                            ConnectionStatus = "CONNECTED";
                            ConnectButtonContext = "Disconnect";
                            //RDnum1 = new RDItem();
                            //RDnum2 = new RDItem();
                            //RDnum3 = new RDItem();
                            //RDnum4 = new RDItem();
                            //RDnum5 = new RDItem();

                            //RDrobtarget1 = new RDItem();
                            //RDrobtarget2 = new RDItem();
                            //RDrobtarget3 = new RDItem();
                            //RDrobtarget4 = new RDItem();
                            //RDrobtarget5 = new RDItem();

                            //RDIO1 = new RDItem();
                            //RDIO2 = new RDItem();
                            //RDIO3 = new RDItem();
                            //RDIO4 = new RDItem();

                            DataCheck.RunWorkerAsync();
                        }
                    }
                    else
                    {
                        robot.Disconnect();
                        robot.Dispose();
                        ConnectionStatus = "No controller";
                        ConnectButtonContext = "Connect";
                    }
                }, o => true);
            }
        }
        public ICommand Start
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (robot.IsConnected)
                        robot.RunProgram("TEST");
                }, o => true);
            }
        }
        public ICommand Stop
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (robot.IsConnected)
                        robot.StopProgram();
                }, o => true);
            }
        }
        public ICommand Abort
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (robot.IsConnected)
                        robot.CycleStopProgram();
                }, o => true);
            }
        }
        public ICommand Reset
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (robot.IsConnected)
                    {
                        robot.ResetAlarm();
                    }
                }, o => true);
            }
        }
        static bool IsValidIpAddress(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }
    }
    public class PositionJHelper
    {
        private readonly PositionJ _position;

        public PositionJHelper(PositionJ position)
        {
            _position = position;
        }

        public string J1 => (_position.J1 / 100).ToString("F3");
        public string J2 => (_position.J2 / 100).ToString("F3");
        public string J3 => (_position.J3 / 100).ToString("F3");
        public string J4 => (_position.J4 / 100).ToString("F3");
        public string J5 => (_position.J5 / 100).ToString("F3");
        public string J6 => (_position.J6 / 100).ToString("F3");
        public string J7 => (_position.J7 / 100).ToString("F3");
        public string J8 => (_position.J8 / 100).ToString("F3");
    }
    public class PositionPHelper
    {
        private readonly PositionP _position;

        public PositionPHelper(PositionP position)
        {
            _position = position;
        }

        public string X => (_position.X / 100).ToString("F3");
        public string Y => (_position.Y / 100).ToString("F3");
        public string Z => (_position.Z / 100).ToString("F3");
        public string A => (_position.A / 100).ToString("F3");
        public string B => (_position.B / 100).ToString("F3");
        public string C => (_position.C / 100).ToString("F3");
    }

}
