using PropertyChanged;
using System.ComponentModel;
using System.Net;
using System.Windows.Input;

namespace Mitsubishi_Test
{
    public class MainViewModel : BaseViewModel
    {
        MainWindow mainWindow;
        BackgroundWorker DataCheck;
        MelfaClient robot;
        internal void Loaded(MainWindow mWindow)
        {
            this.mainWindow = mWindow;
            robot = new MelfaClient();
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
                    robot.OperationEnabled = true;
                    robot.Override = orfk;
                    robot.OperationEnabled = false;
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
                try
                {
                    PositionJ = robot.CurrentPositionJ;
                    PositionP = robot.CurrentPositionP;
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                try
                {
                    State state = robot.GetState();
                    OperationStatus = state.RunStatus.ToString();
                    ActiveTask = "Program: " + state.ProgramName;
                    Modules = "Priority: " + state.TaskPriority.ToString();
                    ProgramPosition = "Line: " + state.LineNumber.ToString();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                try
                {
                    //PositionP p1 = robot.ProgramGetPosition("P_01");
                    //p1.X -= 0.01;
                    //robot.WriteVariable("", "P_01", p1.ToString());
                    //PositionP p2 = robot.ProgramGetPosition("P_01");
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                try
                {
                    if (SetNumValuesEnabled)
                    {
                        robot.WriteNumData(RDnum1);
                        robot.WriteNumData(RDnum2);
                        robot.WriteNumData(RDnum3);
                        robot.WriteNumData(RDnum4);
                        robot.WriteNumData(RDnum5);
                    }
                    else
                    {
                        robot.ReadNumData(RDnum1);
                        robot.ReadNumData(RDnum2);
                        robot.ReadNumData(RDnum3);
                        robot.ReadNumData(RDnum4);
                        robot.ReadNumData(RDnum5);
                    }
                    if (SetPosRegValuesEnabled)
                    {
                        robot.WritePosData(RDposReg1);
                        robot.WritePosData(RDposReg2);
                        robot.WritePosData(RDposReg3);
                        robot.WritePosData(RDposReg4);
                        robot.WritePosData(RDposReg5);
                    }
                    else
                    {
                        robot.ReadPosData(RDposReg1);
                        robot.ReadPosData(RDposReg2);
                        robot.ReadPosData(RDposReg3);
                        robot.ReadPosData(RDposReg4);
                        robot.ReadPosData(RDposReg5);
                    }
                    if (SetIOValuesEnabled)
                    {
                        robot.WriteIOData(RDIO1);
                        robot.WriteIOData(RDIO2);
                        robot.WriteIOData(RDIO3);
                        robot.WriteIOData(RDIO4);
                    }
                    else
                    {
                        robot.ReadIOData(RDIO1);
                        robot.ReadIOData(RDIO2);
                        robot.ReadIOData(RDIO3);
                        robot.ReadIOData(RDIO4);
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                try
                {

                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                    
                if (!overrideHold)
                {
                    Override = robot.Override;
                }
            }
        }
        public int Override { get; set; }
        public string OperationStatus { get; set; }
        public string ConnectionStatus { get; set; }
        public string ConnectButtonContext { get; set; }
        public string ActiveTask { get; set; }
        public string Modules { get; set; }
        public string ProgramPosition { get; set; }
        public string IP { get; set; } = "192.168.0.20";
        //public string IP { get; set; } = "127.0.0.1";

        public bool SetNumValuesEnabled { get; set; } = false;
        public bool SetNumNamesEnabled { get; set; } = true;
        public string SetNumButtonName { get; set; } = "SET";
        public bool SetPosRegValuesEnabled { get; set; } = false;
        public bool SetPosRegNamesEnabled { get; set; } = true;
        public string SetPosRegButtonName { get; set; } = "SET";
        public bool SetIOValuesEnabled { get; set; } = false;
        public bool SetIONamesEnabled { get; set; } = true;
        public string SetIOButtonName { get; set; } = "SET";

        public RDItem RDnum1 { get; set; }
        public RDItem RDnum2 { get; set; }
        public RDItem RDnum3 { get; set; }
        public RDItem RDnum4 { get; set; }
        public RDItem RDnum5 { get; set; }

        public RDItem RDposReg1 { get; set; }
        public RDItem RDposReg2 { get; set; }
        public RDItem RDposReg3 { get; set; }
        public RDItem RDposReg4 { get; set; }
        public RDItem RDposReg5 { get; set; }

        public RDItem RDIO1 { get; set; }
        public RDItem RDIO2 { get; set; }
        public RDItem RDIO3 { get; set; }
        public RDItem RDIO4 { get; set; }


        public PositionJ PositionJ { get; set; }
        public PositionP PositionP { get; set; }
        public ICommand ConnectButtonClick
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (ConnectButtonContext == "Connect" && IsValidIpAddress(IP))
                    {
                        robot.Connect(IP);
                        //var info = robot.Open("ClientName");
                        if (robot.IsConnected)
                        {
                            ConnectionStatus = "CONNECTED";
                            ConnectButtonContext = "Disconnect";
                            RDnum1 = new RDItem();
                            RDnum2 = new RDItem();
                            RDnum3 = new RDItem();
                            RDnum4 = new RDItem();
                            RDnum5 = new RDItem();

                            RDposReg1 = new RDItem();
                            RDposReg2 = new RDItem();
                            RDposReg3 = new RDItem();
                            RDposReg4 = new RDItem();
                            RDposReg5 = new RDItem();

                            RDIO1 = new RDItem();
                            RDIO2 = new RDItem();
                            RDIO3 = new RDItem();
                            RDIO4 = new RDItem();

                            DataCheck.RunWorkerAsync();
                        }
                    }
                    else
                    {
                        robot.Disconnect();
                        //robot.Dispose();
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
                    {
                        try
                        {
                            robot.OperationEnabled = true;
                            robot.RunProgram("test");
                            robot.OperationEnabled = false;
                        }
                        catch (Exception ex)
                        {
                            var shs = ex.Message;
                        }
                    }
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
                    {
                        try
                        {
                            robot.OperationEnabled = true;
                            robot.StopProgram();
                            while ((((byte)robot.GetState().RunStatus) & 0x40) != 0) ;
                            robot.CycleStopProgram();
                            robot.ResetProgram();
                            robot.OperationEnabled = false;
                        }
                        catch (Exception ex)
                        {
                            var shs = ex.Message;
                        }
                    }
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


        public ICommand SetNumValues
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (SetNumButtonName == "SET")
                    {
                        SetNumButtonName = "GET";
                        SetNumValuesEnabled = true;
                        SetNumNamesEnabled = false;
                    }
                    else
                    {
                        SetNumButtonName = "SET";
                        SetNumValuesEnabled = false;
                        SetNumNamesEnabled = true;
                    }

                }, o => true);
            }
        }
        public ICommand SetPosRegValues
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (SetPosRegButtonName == "SET")
                    {
                        SetPosRegButtonName = "GET";
                        SetPosRegValuesEnabled = true;
                        SetPosRegNamesEnabled = false;
                    }
                    else
                    {
                        SetPosRegButtonName = "SET";
                        SetPosRegValuesEnabled = false;
                        SetPosRegNamesEnabled = true;
                    }
                }, o => true);
            }
        }
        public ICommand SetIOValues
        {
            get
            {
                return new RelayCommand(o =>
                {
                    if (SetIOButtonName == "SET")
                    {
                        SetIOButtonName = "GET";
                        SetIOValuesEnabled = true;
                        SetIONamesEnabled = false;
                    }
                    else
                    {
                        SetIOButtonName = "SET";
                        SetIOValuesEnabled = false;
                        SetIONamesEnabled = true;
                    }
                }, o => true);
            }
        }

        static bool IsValidIpAddress(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }
    }
    
    [AddINotifyPropertyChangedInterface] // Fody automatically implements INotifyPropertyChanged
    public class RDItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsON { get; set; }
    }
}
