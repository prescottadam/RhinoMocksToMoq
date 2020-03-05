using System.Linq;
using System.Text.RegularExpressions;

namespace RhinoMocksToMoq
{
    public static class ConvertExtensions
    {
        public static string RegexReplace(this string input, string pattern, string replacement)
        {
            string result = input;
            var re = new Regex(pattern, RegexOptions.Singleline);
            while (re.IsMatch(result))
            {
                result = re.Replace(result, replacement);
            }

            return result;
        }

        public static string ConvertUsings(this string input)
        {
            return input
                .Replace("using Rhino.Mocks;", "using Moq;");
        }

        public static string ConvertMockingKernel(this string input)
        {
            var result = 
                input
                    .Replace("using Ninject.MockingKernel.RhinoMock;", "using Ninject.MockingKernel.Moq;")
                    .Replace("IKernel", "MoqMockingKernel")
                    .Replace("new RhinoMocksMockingKernel", "new MoqMockingKernel");

            // look for member-level mocking kernel
            var memberLevelKernelRegex = new Regex(@"(private|public|internal)\s+MoqMockingKernel\s+(?<varName>\w+)");
            if (memberLevelKernelRegex.IsMatch(result))
            {
                var match = memberLevelKernelRegex.Match(result);
                var kernelVarName = match.Groups["varName"].Value;
                result = result.RegexReplace($@"{kernelVarName}\.Get<(I[A-Z]\w+?)>", $"{kernelVarName}.GetMock<$1>");
            }

            // look for method-level mocking kernel
            var methodLevelKernelRegex = new Regex(@"(var|MoqMockingKernel)\s+(?<varName>\w+)\s+=\s+new\s+MoqMockingKernel\(\);");
            if (methodLevelKernelRegex.IsMatch(result))
            {
                var match = methodLevelKernelRegex.Match(result);
                var kernelVarName = match.Groups["varName"].Value;
                result = result.RegexReplace($@"{kernelVarName}\.Get<(I[A-Z]\w+?)>", $"{kernelVarName}.GetMock<$1>");
            }

            return result;
        }

        public static string ConvertMockCreation(this string input)
        {
            return input
                .Replace("MockRepository.GenerateMock", "new Mock")
                .RegexReplace(@"MockRepository.GeneratePartialMock<(.*?)>\((.*?)\);", "new Mock<$1>($2) { CallBase = true };");
        }

        public static string ConvertMockUsage(this string input)
        {
            var result = input;

            // find mocks by assignment e.g. "something = something.GetMock" or "something = new Mock"
            var mockRegex = new Regex(@"(?<varName>\w+)\s+=\s+(((\w+?)\.GetMock)|new Mock)");
            if (mockRegex.IsMatch(result))
            {
                foreach (var match in mockRegex.Matches(result).OfType<Match>())
                {
                    var mockVarName = match.Groups["varName"].Value;
                    result =
                        result
                            .RegexReplace($@"([^vd\s]\w+)\s+{mockVarName};", $"Mock<$1> {mockVarName};")
                            .RegexReplace($@"{mockVarName}((?!\s*\.Setup|\s*\.Verify|\s*\.Object|\s*[=;]))", $"{mockVarName}.Object$1");
                }
            }

            return result;
        }

        public static string ConvertStubs(this string input)
        {
            return input
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.Return\((.*?)\);", ".Setup($1)$2.Returns($3);")
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.WhenCalled\((.*?)\);", ".Setup($1)$2.Returns($3);")
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.Do\((.*?)\);", ".Setup($1)$2.Returns($3);")
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.Throw\((.*?)\);", ".Setup($1)$2.Throws($3);");
        }

        public static string ConvertExpects(this string input)
        {
            return input
                .RegexReplace(@"\.Expect\((.*?)\)(\s*)\.Return\((.*?)\);", ".Setup($1)$2.Returns($3).Verifiable();")
                .RegexReplace(@"\.Expect\((.*?)\);", ".Setup($1).Verifiable();");
        }

        public static string ConvertArgumentConstraints(this string input)
        {
            return input
                .RegexReplace(@"Arg<([^(]+?)>\.Is\.Anything", @"It.IsAny<$1>()")
                .RegexReplace(@"Arg<([^(]+?)>\.Matches", "It.Is<$1>")
                .RegexReplace(@"Arg<([^(]+?)>\.Is\.Equal\((.*?)\)", "It.Is<$1>(arg => arg == $2)")
                .RegexReplace(@"Arg<([^(]+?)>\.Is\.Same\((.*?)\)", "It.Is<$1>(arg => arg == $2)");
        }

        public static string ConvertAssertions(this string input)
        {
            return input
                .RegexReplace(@"AssertWasCalled\((.*?)\);", "Verify($1);")
                .RegexReplace(@"AssertWasNotCalled\((.*?)\);", "Verify($1, Times.Never);")
                .Replace("VerifyAllExpectations", "Verify");
        }
    }
}
