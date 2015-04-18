using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeContractor.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace CodeContractor.UnitTests.Common
{
    /// <summary>
    /// Uses by <see cref="DiagnosticVerifier"/> to notify that diagnostic verification failed.
    /// </summary>
    public class DiagnosticVerificationException : Exception
    {
        public DiagnosticVerificationException(string message)
            : base(message)
        {}
    }

    /// <summary>
    /// Generic superclass of all Unit Tests for DiagnosticAnalyzers
    /// </summary>
    public abstract class DiagnosticVerifier<T> : DiagnosticVerifier where T : DiagnosticAnalyzer, new()
    {
        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new T();
        }
    }

    /// <summary>
    /// Superclass of all Unit Tests for DiagnosticAnalyzers
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        protected void VerifyCSharpHasNoDiagnostics(string source) => VerifyCSharpDiagnostic(source);

        protected void VerifyCSharpHasNoDiagnostics(params string[] sources) => VerifyCSharpDiagnostic(sources);

        /// <summary>
        /// Get the CSharp analyzer being tested - to be implemented in non-abstract class
        /// </summary>
        protected abstract DiagnosticAnalyzer GetDiagnosticAnalyzer();

        /// <summary>
        /// Get the existing compiler diagnostics on the inputted document.
        /// </summary>
        /// <param name="document">The Document to run the compiler diagnostic analyzers on</param>
        /// <returns>The compiler diagnostics that were found in the code</returns>
        protected static IEnumerable<Diagnostic> GetCompilerDiagnostics(Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on</param>
        /// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
        protected void VerifyCSharpDiagnostic(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(new[] { source }, LanguageNames.CSharp, GetDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// Called to test a VB DiagnosticAnalyzer when applied on the single inputted string as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A class in the form of a string to run the analyzer on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source</param>
        protected void VerifyBasicDiagnostic(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(new[] { source }, LanguageNames.VisualBasic, GetDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        protected void VerifyCSharpDiagnostic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(sources, LanguageNames.CSharp, GetDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// Called to test a VB DiagnosticAnalyzer when applied on the inputted strings as a source
        /// Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        protected void VerifyBasicDiagnostic(string[] sources, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(sources, LanguageNames.VisualBasic, GetDiagnosticAnalyzer(), expected);
        }

        /// <summary>
        /// General method that gets a collection of actual diagnostics found in the source after the analyzer is run,
        /// then verifies each of them.
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run teh analyzers on</param>
        /// <param name="language">The language of the classes represented by the source strings</param>
        /// <param name="analyzer">The analyzer to be run on the source code</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        private void VerifyDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected)
        {
            var diagnostics = GetSortedDiagnostics(sources, language, analyzer);
            Option<string> failure = VerifyDiagnosticResultsFailure(diagnostics, analyzer, expected);

            if (failure.HasValue)
            {
                throw new DiagnosticVerificationException(failure.Value);
            }
        }


        /// <summary>
        /// Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array of expected results.
        /// Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the DiagnosticResult match the actual diagnostic.
        /// </summary>
        /// <param name="actualResults">The Diagnostics found by the compiler after running the analyzer on the source code</param>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="expectedResults">Diagnsotic Results that should have appeared in the code</param>
        private static Option<string> VerifyDiagnosticResultsFailure(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
        {
            var expectedCount = expectedResults.Count();
            var actualCount = actualResults.Count();

            if (expectedCount != actualCount)
            {
                var diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(analyzer, actualResults.ToArray()) : "    NONE.";

                return $"Mismatch between number of diagnostics returned, expected \"{expectedCount}\" acutal \"{actualCount}\"\r\n\r\nDiagnostics:\r\n{diagnosticsOutput}\r\n";
            }

            for (int i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults.ElementAt(i);
                var expected = expectedResults[i];

                if (expected.Line == -1 && expected.Column == -1)
                {
                    if (actual.Location != Location.None)
                    {
                        return $"Expected:\nA project diagnostic with No location\nActual:\n{FormatDiagnostics(analyzer, actual)}";
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());
                    var additionalLocations = actual.AdditionalLocations.ToArray();

                    if (additionalLocations.Length != expected.Locations.Length - 1)
                    {
                        return $"Expected {expected.Locations.Length - 1} additional locations but got {additionalLocations.Length} for Diagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n";
                    }

                    for (int j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(analyzer, actual, additionalLocations[j], expected.Locations[j + 1]);
                    }
                }

                if (actual.Id != expected.Id)
                {
                    return $"Expected diagnostic id to be \"{expected.Id}\" was \"{actual.Id}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n";
                };

                if (actual.Severity != expected.Severity)
                {
                    return $"Expected diagnostic severity to be \"{expected.Severity}\" was \"{actual.Severity}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n";
                };

                if (actual.GetMessage() != expected.Message)
                {
                    return $"Expected diagnostic message to be \"{expected.Message}\" was \"{actual.GetMessage()}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, actual)}\r\n";
                };
            }

            return null;
        }

        /// <summary>
        /// Helper method to VerifyDiagnosticResult that checks the location of a diagnostic and compares it with the location in the expected DiagnosticResult.
        /// </summary>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="diagnostic">The diagnostic that was found in the code</param>
        /// <param name="actual">The Location of the Diagnostic found in the code</param>
        /// <param name="expected">The DiagnosticResultLocation that should have been found</param>
        private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            Option<string> failure = VerifyDiagnosticLocationWithMessage(analyzer, diagnostic, actual, expected);

            if (failure.HasValue)
            {
                throw new DiagnosticVerificationException(failure.Value);
            }
        }

        private static Option<string> VerifyDiagnosticLocationWithMessage(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            bool isValid = actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test."));
            if (!isValid)
            {
                return $"Expected diagnostic to be in file \"{expected.Path}\" was actually in file \"{actualSpan.Path}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, diagnostic)}\r\n";
            }


            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0 && actualLinePosition.Line + 1 != expected.Line)
            {
                return $"Expected diagnostic to be on line \"{expected.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, diagnostic)}\r\n";
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0 && actualLinePosition.Character + 1 != expected.Column)
            {
                return $"Expected diagnostic to start at column \"{expected.Column}\" was actually at column \"{actualLinePosition.Character + 1}\"\r\n\r\nDiagnostic:\r\n    {FormatDiagnostics(analyzer, diagnostic)}\r\n";
            }

            return null;
        }

        /// <summary>
        /// Helper method to format a Diagnostic into an easily reasible string
        /// </summary>
        /// <param name="analyzer">The analyzer that this Verifer tests</param>
        /// <param name="diagnostics">The Diagnostics to be formatted</param>
        /// <returns>The Diagnostics formatted as a string</returns>
        private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i].ToString());

                var analyzerType = analyzer.GetType();
                var rules = analyzer.SupportedDiagnostics;

                foreach (var rule in rules)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                        }
                        else
                        {
                            if (!location.IsInSource)
                            {
                                throw new DiagnosticVerificationException($"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata:\r\n{diagnostics[i]}");
                            }

                            var resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs") ? "GetCSharpResultAt" : "GetBasicResultAt";
                            var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                            builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
                                resultMethodName,
                                linePosition.Line + 1,
                                linePosition.Character + 1,
                                analyzerType.Name,
                                rule.Id);
                        }

                        if (i != diagnostics.Length - 1) builder.Append(',');

                        builder.AppendLine();
                        break;
                    }
                }
            }

            return builder.ToString();
        }
    }
}