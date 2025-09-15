using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace Mitsubishi_Test
{
    public class MelfaClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public bool IsConnected { get; set; } = false;
        public bool OperationEnabled
        {
            set
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

        public bool Connect(string ip, int port = 10001)
        {
            try
            {
                _client = new TcpClient(ip, port);
                _stream = _client.GetStream();
                var open = SendCommand("1;1;OPEN=EMO");
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
            string result = SendCommand("1;1;RUN" + v + ";1;1");
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

        internal void WriteNumData(RDItem rdi)
        {
            double val = 0.0;
            double val2 = 0.0;
            string result = SendCommand($"1;1;LISTPR_" + rdi.Name);
            if (result.StartsWith("Qok", StringComparison.OrdinalIgnoreCase) && result.Contains("="))
                result = result.Substring(3).Split('=')[1];
            else
                return;
            double.TryParse(rdi.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
            double.TryParse(result, NumberStyles.Float, CultureInfo.InvariantCulture, out val2);
            if (val != val2)
            {
                string result2 = SendCommand($"1;1;HOT;R_{rdi.Name}={rdi.Value}");
            }
        }

        internal void ReadNumData(RDItem rdi)
        {
            if (string.IsNullOrEmpty(rdi.Name)) return;
            try
            {
                string result = SendCommand($"1;1;LISTPR_" + rdi.Name);
                if (result.StartsWith("Qok", StringComparison.OrdinalIgnoreCase) && result.Contains("="))
                    result = result.Substring(3).Split('=')[1];
                else
                    return;
                double val = 0;
                if (double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    double roundedVal = Math.Round(val, 3, MidpointRounding.AwayFromZero);
                    rdi.Value = roundedVal.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        internal void WritePosData(RDItem rdi)
        {
            if (string.IsNullOrEmpty(rdi.Name)) return;
            try
            {
                string result = SendCommand($"1;1;LISTPP_" + rdi.Name);
                if (result.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                {
                    PositionP posp = new PositionP(result);
                    if (rdi.Value != posp.ToString())
                    {
                        PositionP posp2 = new PositionP(rdi.Value);
                        string result2 = SendCommand($"1;1;HOT;P_{rdi.Name}={rdi.Value}");
                    }
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        internal void ReadPosData(RDItem rdi)
        {
            if (string.IsNullOrEmpty(rdi.Name)) return;
            try
            {
                string result = SendCommand($"1;1;LISTPP_" + rdi.Name);
                if (result.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                {
                    PositionP posp = new PositionP(result);
                    rdi.Value = posp.ToString();
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        internal void WriteIOData(RDItem rdi)
        {
            if (string.IsNullOrEmpty(rdi.Name)) return;

            try
            {
                int ioBitNumber;
                if (!int.TryParse(rdi.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out ioBitNumber))
                {
                    throw new ArgumentException($"RDItem Name '{rdi.Name}' is not a valid integer I/O bit number.");
                }

                int wordIndex = ioBitNumber / 16;
                int bitInWord = ioBitNumber % 16;
                string readCommand = $"1;1;OUT" + (wordIndex * 16); // Query the starting bit of the word
                string robotResponse = SendCommand(readCommand);

                if (!robotResponse.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Failed to read current output word {wordIndex}: {robotResponse}");
                }

                string hexValue = robotResponse.Substring(3);
                if (!int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int currentWordValue))
                {
                    throw new FormatException($"Could not parse hexadecimal word value '{hexValue}' from robot response.");
                }
                if (rdi.IsON != ((currentWordValue & (1 << bitInWord)) != 0))
                {
                    int newWordValue;
                    if (rdi.IsON)
                    {
                        newWordValue = currentWordValue | (1 << bitInWord);
                    }
                    else
                    {
                        newWordValue = currentWordValue & ~(1 << bitInWord);
                    }
                    string writeCommand = $"1;1;OUT={wordIndex * 16};{newWordValue:X4}";
                    string writeResult = SendCommand(writeCommand);

                    if (!writeResult.StartsWith("Qok", StringComparison.OrdinalIgnoreCase) && !writeResult.StartsWith("OK", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Failed to write output word {wordIndex} with value {newWordValue}: {writeResult}");
                    }
                }

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                Console.WriteLine($"Error writing IO data for {rdi.Name}: {msg}");
            }
        }

        internal void ReadIOData(RDItem rdi)
        {
            if (string.IsNullOrEmpty(rdi.Name)) return;
            try
            {
                int val = 0;
                int.TryParse(rdi.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out val);
                int wordIndex = val / 16;
                int bitInWord = val % 16;
                string result = SendCommand($"1;1;OUT" + wordIndex * 16);
                if (result.StartsWith("Qok", StringComparison.OrdinalIgnoreCase))
                    result = result.Substring(3);
                else
                    return;
                if (!int.TryParse(result, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int wordValue))
                {
                    throw new ArgumentException($"Could not parse hexadecimal value: {result}");
                }
                rdi.IsON = (wordValue & (1 << bitInWord)) != 0;

                //return isBitSet ? 1 : 0;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        public int GetOutputBitStatus(int ionum)
        {
            int wordIndex = ionum / 16;
            int bitInWord = ionum % 16;
            string robotResponse = SendCommand($"1;1;OUT{ionum}");
            if (string.IsNullOrEmpty(robotResponse) || !robotResponse.StartsWith("Qok"))
            {
                throw new ArgumentException($"Invalid robot response: {robotResponse}");
            }

            string hexValue = robotResponse.Substring(3); // Remove "Qok"

            if (!int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int wordValue))
            {
                throw new ArgumentException($"Could not parse hexadecimal value: {hexValue}");
            }
            bool isBitSet = (wordValue & (1 << bitInWord)) != 0;

            return isBitSet ? 1 : 0;
        }
        public bool ParseOutputBitStatus(string robotResponse, int ionum)
        {
            int bitInWord = ionum % 16;
            if (string.IsNullOrEmpty(robotResponse) || !robotResponse.StartsWith("Qok"))
            {
                throw new ArgumentException($"Invalid robot response: {robotResponse}");
            }

            string hexValue = robotResponse.Substring(3); // Remove "Qok"

            if (!int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int wordValue))
            {
                throw new ArgumentException($"Could not parse hexadecimal value: {hexValue}");
            }
            bool isBitSet = (wordValue & (1 << bitInWord)) != 0;

            return isBitSet;
        }
    }
}
