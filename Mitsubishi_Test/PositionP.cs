using System.Globalization;
using System.Text.RegularExpressions;

namespace Mitsubishi_Test
{
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

        private const string PositionXyzPattern = @"^\((?<X>[+-]?\d+\.\d+),(?<Y>[+-]?\d+\.\d+),(?<Z>[+-]?\d+\.\d+),(?<A>[+-]?\d+\.\d+),(?<B>[+-]?\d+\.\d+),(?<C>[+-]?\d+\.\d+),(?<L1>[+-]?\d+\.\d+),(?<L2>[+-]?\d+\.\d+)\)\((?<Flag1>\d*),(?<Flag2>\d*)\)$"; // Modified here

        private const string SemicolonPattern =
    @"X;(?<X>[+-]?\d+\.\d+);Y;(?<Y>[+-]?\d+\.\d+);Z;(?<Z>[+-]?\d+\.\d+);A;(?<A>[+-]?\d+\.\d+);B;(?<B>[+-]?\d+\.\d+);C;(?<C>[+-]?\d+\.\d+);;(?<Flag1>\d+),(?<Flag2>\d+);(?<Ignored1>\d+);(?<Ignored2>[+-]?\d+\.\d+);(?<Identifier>\d+)";

        public PositionP(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or empty.");

            input = input.Trim();

            // Save identifier if present (this part of your original code is fine)
            int eqIdx = input.IndexOf('=');
            if (eqIdx >= 0 && input.StartsWith("QoK", StringComparison.OrdinalIgnoreCase)) // Refined check for Qok prefix
            {
                this.Identifier = input.Substring(0, eqIdx); // Capture "QoKP_200"
                input = input.Substring(eqIdx + 1); // Get only the coordinate string
            }
            else if (input.StartsWith("Qok", StringComparison.OrdinalIgnoreCase)) // Handle Qok without '='
            {
                input = input.Substring(3); // Remove just "Qok"
            }
            // If there's no "Qok" or '=', the identifier remains null/empty

            // Try semicolon format starting from "X;"
            var semicolonMatch = Regex.Match(input, SemicolonPattern);
            if (semicolonMatch.Success)
            {
                X = double.Parse(semicolonMatch.Groups["X"].Value, CultureInfo.InvariantCulture);
                Y = double.Parse(semicolonMatch.Groups["Y"].Value, CultureInfo.InvariantCulture);
                Z = double.Parse(semicolonMatch.Groups["Z"].Value, CultureInfo.InvariantCulture);
                A = double.Parse(semicolonMatch.Groups["A"].Value, CultureInfo.InvariantCulture);
                B = double.Parse(semicolonMatch.Groups["B"].Value, CultureInfo.InvariantCulture);
                C = double.Parse(semicolonMatch.Groups["C"].Value, CultureInfo.InvariantCulture);
                L1 = 0; // L1 and L2 are not in this format
                L2 = 0;
                Flag1 = byte.Parse(semicolonMatch.Groups["Flag1"].Value, CultureInfo.InvariantCulture);
                Flag2 = byte.Parse(semicolonMatch.Groups["Flag2"].Value, CultureInfo.InvariantCulture);
                // Identifier is parsed within the semicolon pattern in your original code
                // Identifier = semicolonMatch.Groups["Identifier"].Value; // If you want to use this Identifier
                return;
            }

            // Try parenthesis format: (...)(...)
            var match = Regex.Match(input, PositionXyzPattern);
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

            // Handle empty strings for flags
            Flag1 = string.IsNullOrEmpty(match.Groups["Flag1"].Value) ? (byte)0 : byte.Parse(match.Groups["Flag1"].Value, CultureInfo.InvariantCulture);
            Flag2 = string.IsNullOrEmpty(match.Groups["Flag2"].Value) ? (byte)0 : byte.Parse(match.Groups["Flag2"].Value, CultureInfo.InvariantCulture);

            return;
        }

        /// <summary>
        /// Returns a string representation of the DataParser object in the format:
        /// (+X,Y,Z,A,B,C,L1,L2)(Flag1,Flag2)
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "({0:+0.00;-0.00;+0.00},{1:+0.00;-0.00;+0.00},{2:+0.00;-0.00;+0.00},{3:+0.00;-0.00;+0.00},{4:+0.00;-0.00;+0.00},{5:+0.00;-0.00;+0.00},{6:+0.00;-0.00;+0.00},{7:+0.00;-0.00;+0.00})({8},{9})",
                X, Y, Z, A, B, C, L1, L2, Flag1, Flag2);
        }
    }
}
