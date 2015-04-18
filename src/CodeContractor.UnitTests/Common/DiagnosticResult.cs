using System;
using Microsoft.CodeAnalysis;

namespace CodeContractor.UnitTests.Common
{
    /// <summary>
    /// Location where the diagnostic appears, as determined by path, line number, and column number.
    /// </summary>
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < 0 && column < 0)
            {
                throw new ArgumentOutOfRangeException("At least one of line and column must be > 0", line < 0 ? nameof(line) : nameof(column));
            }

            if (line < -1 || column < -1)
            {
                throw new ArgumentOutOfRangeException("Both line and column must be >= -1", line < -1 ? nameof(line) : nameof(column));
            }

            Path = path;
            Line = line;
            Column = column;
        }

        public string Path { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
    }

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] _locations;

        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (_locations == null)
                {
                    _locations = new DiagnosticResultLocation[] { };
                }
                return _locations;
            }

            set
            {
                _locations = value;
            }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path => Locations.Length > 0 ? Locations[0].Path : "";

        public int Line => Locations.Length > 0 ? Locations[0].Line : -1;

        public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
    }
}