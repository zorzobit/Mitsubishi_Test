using System.Globalization;
using System.Text.RegularExpressions;

namespace Mitsubishi_Test
{
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

        public State(string input)
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
}
