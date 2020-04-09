using System.Linq;
using System.Text.RegularExpressions;

namespace RhinoMocksToMoq
{
    public static class ConvertExtensions
    {
        public static string RegexReplace(this string input, string pattern, string replacement)
        {
            string result = input;
            var regex = new Regex(pattern, RegexOptions.Singleline);
            var matches = regex.Matches(input).Cast<Match>().ToList();

            matches.ForEach(match =>
            {
                result = regex.Replace(result, replacement);
            });

            return result;
        }

        public static string ConvertUsings(this string input)
        {
            return input.Replace("using Rhino.Mocks;", "using Moq;");
        }

        public static string ConvertMockingKernel(this string input)
        {
            var result =
                input
                    .Replace("using Ninject.MockingKernel.RhinoMock;", "using Ninject.MockingKernel.Moq;")
                    .Replace("IKernel", "MoqMockingKernel")
                    .Replace("new RhinoMocksMockingKernel", "new MoqMockingKernel");

            // look for member-level mocking kernel
            var memberLevelKernelRegex = new Regex(@"(private|public|internal)\s+MoqMockingKernel\s+(?<varName>[a-zA-Z_0-9]+)");
            if (memberLevelKernelRegex.IsMatch(result))
            {
                var match = memberLevelKernelRegex.Match(result);
                var kernelVarName = match.Groups["varName"].Value;
                result = result.RegexReplace($@"{kernelVarName}\.Get<([A-Z]([a-zA-Z_0-9.])+?)>", $"{kernelVarName}.GetMock<$1>");
            }

            // look for method-level mocking kernel
            var methodLevelKernelRegex = new Regex(@"(var|MoqMockingKernel)\s+(?<varName>\w+)\s+=\s+new\s+MoqMockingKernel\(\);");
            if (methodLevelKernelRegex.IsMatch(result))
            {
                var match = methodLevelKernelRegex.Match(result);
                var kernelVarName = match.Groups["varName"].Value;
                result = result.RegexReplace($@"{kernelVarName}\.Get<([A-Z]([a-zA-Z_0-9.])+?)>", $"{kernelVarName}.GetMock<$1>");
            }

            return result;
        }

        public static string ConvertMockCreation(this string input)
        {
            return input
                .Replace("MockRepository.GenerateStub", "new Mock")
                .Replace("MockRepository.GenerateMock", "new Mock")
                .RegexReplace(@"MockRepository.GeneratePartialMock<(.*?)>\((.*?)\);", "new Mock<$1>($2) { CallBase = true };");
        }

        public static string ConvertMockUsage(this string input)
        {
            var result = input;

            // find mocks by assignment e.g. "something = something.GetMock" or "something = new Mock"
            var mockRegex = new Regex(@"(?<varName>\w+)\s+=\s+(((\w+?)\.GetMock)|new Mock)");
            var matches = mockRegex.Matches(input).Cast<Match>().ToList();

            foreach (var match in matches)
            {
                var mockVarName = match.Groups["varName"].Value;
                result =
                    result
                        .RegexReplace($@"[\s\r\n]([A-Z][a-zA-Z_0-9.]+)\s+{mockVarName};", $"Mock<$1> {mockVarName};")
                        .RegexReplace($@"{mockVarName}(?!(\s*\.Setup|\s*\.Verify|\s*\.Object|\s*[=;>>A-Za-z0-9]))", $"{mockVarName}.Object$1");
            }

            return result;
        }

        public static string ConvertStubs(this string input)
        {
            return input
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.Return\((.*?)\);", ".Setup($1)$2.Returns($3);")
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.WhenCalled\((.*?)\);", ".Setup($1)$2.Returns($3);")
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.Do\((.*?)\);", ".Setup($1)$2.Returns($3);")
                .RegexReplace(@"\.Stub\((.*?)\)(\s*)\.Throw\((.*?)\);", ".Setup($1)$2.Throws($3);")
                .RegexReplace(@"\.Setup\((.*?)\)(\s*)\.Throw\((.*?)\);", ".Setup($1)$2.Throws($3);")
                .RegexReplace(@"\.Expect\((.*?)\)(\s*)\.Throw\((.*?)\);", ".Expect($1)$2.Throws($3);");
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
                .RegexReplace(@"ref Arg<([^(,]+?)>\.Ref\(([A-Z0-9a-z_]+?)\)", @"ref $2)")
                .RegexReplace(@"out Arg\<[^(,]+\>.Out\((.*?)\).Dummy", @"out $1")
                .RegexReplace(@"Arg.Is\(([^(,]+?)\)", @"It.Is<>(arg => arg == $1)")
                .RegexReplace(@"Arg<([^(,]+?)>\.Is\.Anything", @"It.IsAny<$1>()")
                .RegexReplace(@"Arg<([^(,]+?)>\.Is\.Null", @"It.Is<$1>(arg => arg == null)")
            .RegexReplace(@"Arg<([^(,]+?)>\.Is\.NotNull", @"It.Is<$1>(arg => arg != null)")
            .RegexReplace(@"Arg<([^(,]+?)>\.Matches", "It.Is<$1>")
            .RegexReplace(@"Arg<([^(,]+?)>\.Is\.Equal\((.*?)\)", "It.Is<$1>(arg => arg == $2)")
            .RegexReplace(@"Arg<([^(,]+?)>\.Is\.Same\((.*?)\)", "It.Is<$1>(arg => arg == $2)")
            .RegexReplace(@"Arg<([^(,]+?)>\.Is\.NotSame\((.*?)\)", "It.Is<$1>(arg => arg != $2)");
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
