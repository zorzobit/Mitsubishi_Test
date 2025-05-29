using PropertyChanged;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

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
                PositionJ = robot.CurrentPositionJ;
                PositionP = robot.CurrentPositionP;
                State state = robot.GetState();
                OperationStatus = state.RunStatus.ToString();
                ActiveTask = "Program: " + state.ProgramName;
                Modules = "Priority: " + state.TaskPriority.ToString();
                ProgramPosition = "Line: " + state.LineNumber.ToString();
                try
                {
                    PositionP p1 = robot.ProgramGetPosition("P_01");
                    var post = robot.CurrentPositionP;

                    var sfs = post.ToString();
                    robot.WriteVariable("", "P_01", sfs);
                    PositionP p2 = robot.ProgramGetPosition("P_01");
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
        public string IP { get; set; } = "192.168.0.118";

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
        public RDItem RDposReg5{ get; set; }

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

    public class PositionJ
    {
        public double J1 { get; set; }
        public double J2 { get; set; }
        public double J3 { get; set; }
        public double J4 { get; set; }
        public double J5 { get; set; }
        public double J6 { get; set; }
        public double J7 { get; set; }
        public double J8 { get; set; }

        public PositionJ(string posj)
        {
            if (string.IsNullOrWhiteSpace(posj))
                throw new ArgumentException("Input string is null or empty.");

            if (!posj.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                throw new FormatException("String does not start with 'Qok'.");

            var parts = posj.Split(';', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                // Joint values are expected after "Qok" prefix
                for (int i = 1; i < parts.Length; i += 2)
                {
                    string jointName = parts[i - 1].Trim();
                    if (jointName.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                        jointName = jointName.Substring(3);
                    if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        continue; // skip if value is not parsable

                    switch (jointName.ToUpperInvariant())
                    {
                        case "J1": J1 = value; break;
                        case "J2": J2 = value; break;
                        case "J3": J3 = value; break;
                        case "J4": J4 = value; break;
                        case "J5": J5 = value; break;
                        case "J6": J6 = value; break;
                        case "J7": J7 = value; break;
                        case "J8": J8 = value; break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormatException("Failed to parse position string.", ex);
            }
        }
    }

    public class PositionP
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double L1 { get; set; }
        public double L2 { get; set; }
        public byte Flag1 { get; set; }
        public byte Flag2 { get; set; }
        public string Identifier { get; set; }

        private const string PositionXyzPattern =
            @"^\s*\(" +
            @"\s*(?'X'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'Y'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'Z'[+-]?\d+(?:\.\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'A'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'B'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'C'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'L1'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*," +
            @"\s*(?'L2'[+-]?\d+(?:\.\d+)?|[nN][aA][nN]|[+-]?[iI][nN][fF])\s*" +
            @"\)\s*\(" +
            @"\s*(?'FLG1'0[xX][0-9a-fA-F]+|\d+|[lLrR][bBaA][fFnN])\s*," +
            @"\s*(?'FLG2'0[xX][0-9a-fA-F]+|\d+)\s*" +
            @"\)\s*$";

        private const string SemicolonPattern =
            @"(?i)^Qok(?<id>[^\;]*)\;X;(?<X>-?\d+\.?\d*)\;Y;(?<Y>-?\d+\.?\d*)\;Z;(?<Z>-?\d+\.?\d*)\;" +
            @"A;(?<A>-?\d+\.?\d*)\;B;(?<B>-?\d+\.?\d*)\;C;(?<C>-?\d+\.?\d*)\;\;(?<FLG1>\d+),(?<FLG2>\d+);";

        public PositionP(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or empty.");

            input = input.Trim();

            if (!input.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                throw new FormatException("Input must start with 'Qok'.");

            // Extract identifier like QokX, QokP_01, etc.
            int prefixEnd = input.IndexOf('=') != -1 ? input.IndexOf('=') : input.IndexOf(';');
            Identifier = input.Substring(0, prefixEnd).Trim();

            // Try semicolon format
            var semicolonMatch = Regex.Match(input, SemicolonPattern);
            if (semicolonMatch.Success)
            {
                X = double.Parse(semicolonMatch.Groups["X"].Value, CultureInfo.InvariantCulture);
                Y = double.Parse(semicolonMatch.Groups["Y"].Value, CultureInfo.InvariantCulture);
                Z = double.Parse(semicolonMatch.Groups["Z"].Value, CultureInfo.InvariantCulture);
                A = double.Parse(semicolonMatch.Groups["A"].Value, CultureInfo.InvariantCulture);
                B = double.Parse(semicolonMatch.Groups["B"].Value, CultureInfo.InvariantCulture);
                C = double.Parse(semicolonMatch.Groups["C"].Value, CultureInfo.InvariantCulture);
                L1 = 0; L2 = 0; // Not present in this format
                Flag1 = byte.Parse(semicolonMatch.Groups["FLG1"].Value, CultureInfo.InvariantCulture);
                Flag2 = byte.Parse(semicolonMatch.Groups["FLG2"].Value, CultureInfo.InvariantCulture);
                return;
            }

            // Try parenthesis format: QokP_01=(...)(...)
            int eqIdx = input.IndexOf('=');
            if (eqIdx >= 0)
            {
                var rest = input.Substring(eqIdx + 1);
                var match = Regex.Match(rest, PositionXyzPattern);
                if (!match.Success)
                    throw new FormatException("Invalid position format.");

                X = double.Parse(match.Groups["X"].Value, CultureInfo.InvariantCulture);
                Y = double.Parse(match.Groups["Y"].Value, CultureInfo.InvariantCulture);
                Z = double.Parse(match.Groups["Z"].Value, CultureInfo.InvariantCulture);
                A = double.Parse(match.Groups["A"].Value, CultureInfo.InvariantCulture);
                B = double.Parse(match.Groups["B"].Value, CultureInfo.InvariantCulture);
                C = double.Parse(match.Groups["C"].Value, CultureInfo.InvariantCulture);
                L1 = double.Parse(match.Groups["L1"].Value, CultureInfo.InvariantCulture);
                L2 = double.Parse(match.Groups["L2"].Value, CultureInfo.InvariantCulture);
                Flag1 = byte.Parse(match.Groups["FLG1"].Value, CultureInfo.InvariantCulture);
                Flag2 = byte.Parse(match.Groups["FLG2"].Value, CultureInfo.InvariantCulture);
                return;
            }

            throw new FormatException("Unrecognized PositionP format.");
        }
    }
    public class State
    {
        private const string RunStateRegexPattern =
           @"(?'ProgramName'[^;]*);" +
           @"(?'LineNumber'\d+);" +
           @"(?'Override'\d+);" +
           @"(?'EditStatus'\d+);" +
           @"(?'RunStatus'[0-9a-fA-F]{2})" +
           @"(?'StopStatus'[0-9a-fA-F]{2})" +
           @"(?'ErrorNumber'\d+);(?'StepNumber'\d+);" +
           @"(?'MechInfo'\d+);{8}" +
           @"(?'TaskProgramName'[^;]*);" +
           @"(?'TaskMode'\w+);" +
           @"(?'TaskCondition'\w+);" +
           @"(?'TaskPriority'\d+);" +
           @"(?'MechNumber'\d+)";


        public string ProgramName { get; set; }
        public int LineNumber { get; set; }
        public int Override { get; set; }
        public int EditStatus { get; set; }
        public RunStatus RunStatus { get; set; }
        public string StopStatus { get; set; }
        public int ErrorNumber { get; set; }
        public int StepNumber { get; set; }
        public int MechInfo { get; set; }
        public string TaskProgramName { get; set; }
        public string TaskMode { get; set; }
        public string TaskCondition { get; set; }
        public int TaskPriority { get; set; }
        public int MechNumber { get; set; }

        public State (string input)
        {
            var match = Regex.Match(input, RunStateRegexPattern);

            if (!match.Success)
                throw new FormatException("Input string does not match the expected format.");

            this.ProgramName = match.Groups["ProgramName"].Value;
           this.LineNumber = int.Parse(match.Groups["LineNumber"].Value);
           this.Override = int.Parse(match.Groups["Override"].Value);
           this.EditStatus = int.Parse(match.Groups["EditStatus"].Value);
           this.RunStatus = (RunStatus)int.Parse(match.Groups["RunStatus"].Value, NumberStyles.HexNumber);
           this.StopStatus = match.Groups["StopStatus"].Value;
           this.ErrorNumber = int.Parse(match.Groups["ErrorNumber"].Value);
           this.StepNumber = int.Parse(match.Groups["StepNumber"].Value);
           this.MechInfo = int.Parse(match.Groups["MechInfo"].Value);
           this.TaskProgramName = match.Groups["TaskProgramName"].Value;
           this.TaskMode = match.Groups["TaskMode"].Value;
           this.TaskCondition = match.Groups["TaskCondition"].Value;
           this.TaskPriority = int.Parse(match.Groups["TaskPriority"].Value);
            this.MechNumber = int.Parse(match.Groups["MechNumber"].Value);
        }
    }


    public class MelfaClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public bool IsConnected { get; set; } = false;
        public bool OperationEnabled { set
            {
                if (value) 
                    SendCommand("1;1;CNTLON"); 
                else 
                    SendCommand("1;1;CNTLOFF");
            }
        }
        public int Override
        {
            get
            {
                int result = 0;
                string _override = SendCommand("1;1;OVRD");
                if (_override.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                    _override = _override.Substring(3);
                int.TryParse(_override, out result);
                return result;
            }
            set
            {
                SendCommand("1;1;OVRD=" + value.ToString());
            }
        }
        public PositionJ CurrentPositionJ
        {
            get
            {
                string posj = SendCommand("1;1;JPOSF");
                return new PositionJ(posj);
            }
        }
        public PositionP CurrentPositionP
        {
            get
            {
                string posp = SendCommand("1;1;PPOSF");
                return new PositionP(posp);
            }
        }

        public bool Connect(string ip, int port=10001)
        {
            try
            {
                _client = new TcpClient(ip, port);
                _stream = _client.GetStream();
                IsConnected = true;
                return true;
            }
            catch
            {
                IsConnected = false;
                return false;
            }
        }

        public string SendCommand(string command)
        {
            if (_stream == null) return null;

            byte[] data = Encoding.ASCII.GetBytes(command + "\r");
            _stream.Write(data, 0, data.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
        }
        public State GetState()
        {
            string state = SendCommand("1;1;STATE");
            if (state.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                state = state.Substring(3);
            return new State(state);
        }

        internal void RunProgram(string v)
        {
            string result = SendCommand("1;1;RUN"+v+";1;1");
        }

        internal void StopProgram()
        {
            string result = SendCommand("1;1;STOP");
        }

        internal void CycleStopProgram()
        {
            string result = SendCommand("1;1;CSTOP");
        }

        internal void ResetProgram()
        {
            string result = SendCommand("1;1;RSTPRG");
        }

        internal void ResetAllPrograms()
        {
            string result = SendCommand("1;1;SLOTINIT");
        }

        internal void WriteVariable(string program, string variable, string value)
        {
            string result = SendCommand($"1;1;HOT{program};{variable}={value}");
        }

        internal PositionP ProgramGetPosition(string positionName)
        {
            string result = SendCommand($"1;1;LISTP{positionName}");
            return new PositionP(result);
    }

        internal void ResetAlarm()
        {
            string result = SendCommand("1;1;RSTALRM");
        }
    }
    [Flags]
    public enum RunStatus : byte
    {
        Repeat = 0x01,
        CycleStopOff = 0x02,
        MachineLockOn = 0x04,
        Teach = 0x08,
        TeachRunning = 0x10,
        ServoOn = 0x20,
        Running = 0x40,
        OperationEnabled = 0x80,
    }
    [Flags]
    public enum TaskMode : byte
    {
        Repeat = 0,
        Cycle = 1,
        Unknown = 0xff,
    }
    [AddINotifyPropertyChangedInterface] // Fody automatically implements INotifyPropertyChanged
    public class RDItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsON { get; set; }
    }
}
