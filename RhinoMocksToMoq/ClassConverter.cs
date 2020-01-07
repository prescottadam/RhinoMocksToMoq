namespace RhinoMocksToMoq
{
    /// <summary>
    /// Converts a C# class using Rhino Mocks to Moq
    /// </summary>
    public class ClassConverter
    {
        public static string Convert(string input)
        {
            return input
                .ConvertUsings()
                .ConvertMockingKernel()
                .ConvertMockCreation()
                .ConvertStubs()
                .ConvertExpects()
                .ConvertArgumentConstraints()
                .ConvertAssertions()
                .ConvertMockUsage();
        }
    }
}
