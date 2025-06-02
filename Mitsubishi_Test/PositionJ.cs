using System.Globalization;

namespace Mitsubishi_Test
{
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
}
