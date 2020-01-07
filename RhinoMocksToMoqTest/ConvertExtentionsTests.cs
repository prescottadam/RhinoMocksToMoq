using NUnit.Framework;
using RhinoMocksToMoq;

namespace RhinoMocksToMoqTest
{
    [TestFixture]
    public class ConvertExtentionsTests
    {
        [Test]
        public void ConvertUsings_ImportsMoqNamespace_WhenRhinoMocksNamespaceIsImported()
        {
            // Arrange
            var input =
@"using Something;
using Rhino.Mocks;
using Something.Else;";

            // Act
            var actual = input.ConvertUsings();

            // Assert
            var expected =
@"using Something;
using Moq;
using Something.Else;";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockingKernel_ImportsNinjectMockingKernelMoq_WhenNinjectMockingKernelRhinoMocksIsImported()
        {
            // Arrange
            var input =
@"using Something;
using Ninject.MockingKernel.RhinoMock;
using Something.Else;";

            // Act
            var actual = input.ConvertMockingKernel();

            // Assert
            var expected =
@"using Something;
using Ninject.MockingKernel.Moq;
using Something.Else;";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockingKernel_InstantiatesMoqMockingKernel_WhenRhinoMocksMockingKernelWasUsed()
        {
            // Arrange
            var input =
@"[SetUp]
public void SetUp()
{
    _kernel = new RhinoMocksMockingKernel();
}";

            // Act
            var actual = input.ConvertMockingKernel();

            // Assert
            var expected =
@"[SetUp]
public void SetUp()
{
    _kernel = new MoqMockingKernel();
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockingKernel_UsesExplicitType_WhenIKernelWasUsed()
        {
            // Arrange
            var input =
@"[TestFixture]
public class FooTests
{
    private IKernel _kernel;
}";

            // Act
            var actual = input.ConvertMockingKernel();

            // Assert
            var expected =
@"[TestFixture]
public class FooTests
{
    private MoqMockingKernel _kernel;
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockingKernel_UsesGetMock_WhenMocksAreRetrievedFromMemberLevelKernel()
        {
            // Arrange
            var input =
@"[TestFixture]
public class FooTests
{
    private MoqMockingKernel _kernel;

    [Test]
    public void Test()
    {
        var mock = _kernel.Get<IFoo>();
    }
}";

            // Act
            var actual = input.ConvertMockingKernel();

            // Assert
            var expected =
@"[TestFixture]
public class FooTests
{
    private MoqMockingKernel _kernel;

    [Test]
    public void Test()
    {
        var mock = _kernel.GetMock<IFoo>();
    }
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockingKernel_UsesGetMock_WhenMocksAreRetrievedFromMethodLevelKernel()
        {
            // Arrange
            var input =
@"[TestFixture]
public class FooTests
{
    [Test]
    public void Test()
    {
        var kernel = new MoqMockingKernel();
        var mock = kernel.Get<IFoo>();
    }
}";

            // Act
            var actual = input.ConvertMockingKernel();

            // Assert
            var expected =
@"[TestFixture]
public class FooTests
{
    [Test]
    public void Test()
    {
        var kernel = new MoqMockingKernel();
        var mock = kernel.GetMock<IFoo>();
    }
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockUsage_UsesMockDotObject_WhenMemberLevelMockIsPassedAsArgument()
        {
            // Arrange
            var input =
@"[TestFixture]
public class FooTests
{
    private Foo _mockFoo;

    [SetUp]
    public void SetUp()
    {
        _mockFoo = new Mock<Foo>();
    }

    [Test]
    public void Test()
    {
        _mockFoo.Setup(x => x.IsTrue()).Returns(true);
        var foo = new Foo(_mockFoo);
        var bar = new Bar(_mockFoo, 1);
    }
}";

            // Act
            var actual = input.ConvertMockUsage();

            // Assert
            var expected =
@"[TestFixture]
public class FooTests
{
    private Mock<Foo> _mockFoo;

    [SetUp]
    public void SetUp()
    {
        _mockFoo = new Mock<Foo>();
    }

    [Test]
    public void Test()
    {
        _mockFoo.Setup(x => x.IsTrue()).Returns(true);
        var foo = new Foo(_mockFoo.Object);
        var bar = new Bar(_mockFoo.Object, 1);
    }
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockUsage_UsesMockDotObject_WhenMethodLevelMockIsPassedAsArgument()
        {
            // Arrange
            var input =
@"[Test]
public void Test()
{
    var mock = kernel.GetMock<IFoo>();
    mock.Setup(x => x.IsTrue()).Returns(true);
    var foo = new Foo(mock);
    var bar = new Bar(mock, 1);
}";

            // Act
            var actual = input.ConvertMockUsage();

            // Assert
            var expected =
@"[Test]
public void Test()
{
    var mock = kernel.GetMock<IFoo>();
    mock.Setup(x => x.IsTrue()).Returns(true);
    var foo = new Foo(mock.Object);
    var bar = new Bar(mock.Object, 1);
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockUsage_UsesMockDotObject_WhenLocalMockIsPassedAsArgument()
        {
            // Arrange
            var input =
@"[Test]
public void Test()
{
    var mock = new Mock<IFoo>();
    mock.Setup(x => x.IsTrue()).Returns(true);
    var foo = new Foo(mock);
    var bar = new Bar(mock, 1);
}";

            // Act
            var actual = input.ConvertMockUsage();

            // Assert
            var expected =
@"[Test]
public void Test()
{
    var mock = new Mock<IFoo>();
    mock.Setup(x => x.IsTrue()).Returns(true);
    var foo = new Foo(mock.Object);
    var bar = new Bar(mock.Object, 1);
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockUsage_UseSMockDotObject_WhenBaseMethodsOfMockAreCalled()
        {
            // Arrange
            var input =
@"private Mock<Foo> _foo;

[Setup]
public void SetUp()
{
    _foo = new Mock<Foo>() { CallBase = true };
}

[Test]
public void Test()
{
    _foo.Bar();
}";

            // Act
            var actual = input.ConvertMockUsage();

            // Assert
            var expected =
@"private Mock<Foo> _foo;

[Setup]
public void SetUp()
{
    _foo = new Mock<Foo>() { CallBase = true };
}

[Test]
public void Test()
{
    _foo.Object.Bar();
}";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockCreation_InstantiatesMock_WhenMockRepositoryWasUsed()
        {
            // Arrange
            var input =
@"var mock = MockRepository.GenerateMock<IFoo>();";

            // Act
            var actual = input.ConvertMockCreation();

            // Assert
            var expected =
@"var mock = new Mock<IFoo>();";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertMockCreation_InstantiatesMockWithCallBase_WhenMockRepositoryCreatePartialMockWasUsed()
        {
            // Arrange
            var input =
@"var mock = MockRepository.GeneratePartialMock<IFoo>();";

            // Act
            var actual = input.ConvertMockCreation();

            // Assert
            var expected =
@"var mock = new Mock<IFoo>() { CallBase = true };";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertStubs_UsesSetup_WhenStubWasUsed()
        {
            // Arrange
            var input =
@"mock
    .Stub(x => x.Foo())
    .Return(true);

mock.Stub(x => x.Bar).Return(1);";

            // Act
            var actual = input.ConvertStubs();

            // Assert
            var expected =
@"mock
    .Setup(x => x.Foo())
    .Returns(true);

mock.Setup(x => x.Bar).Returns(1);";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertExpects_UsesSetupAndVerifiable_WhenExpectWasUsed()
        {
            // Arrange
            var input =
@"mock
    .Expect(x => x.Foo())
    .Return(true);

mock.Expect(x => x.Bar).Return(1);

mock.Expect(x => x.Bar);";

            // Act
            var actual = input.ConvertExpects();

            // Assert
            var expected =
@"mock
    .Setup(x => x.Foo())
    .Returns(true).Verifiable();

mock.Setup(x => x.Bar).Returns(1).Verifiable();

mock.Setup(x => x.Bar).Verifiable();";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertArgumentConstraints_UsesItIsAny_WhenArgIsAnythingWasUsed()
        {
            // Arrange
            var input =
@"mock
    .Setup(x => x.Foo(
        Arg<int>.Is.Anything,
        Arg<Func<string, bool>>.Is.Anything,
        Arg<int>.Is.Anything)))
    .Return(true);";

            // Act
            var actual = input.ConvertArgumentConstraints();

            // Assert
            var expected =
@"mock
    .Setup(x => x.Foo(
        It.IsAny<int>(),
        It.IsAny<Func<string, bool>>(),
        It.IsAny<int>())))
    .Return(true);";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertArgumentConstraints_UsesItIs_WhenArgMatchesWasUsed()
        {
            // Arrange
            var input =
@"mock
    .Setup(x => x.Foo(
        Arg<int>.Matches((y,z) => y == z),
        Arg<int>.Matches(() => 1)))
    .Return(true);";

            // Act
            var actual = input.ConvertArgumentConstraints();

            // Assert
            var expected =
@"mock
    .Setup(x => x.Foo(
        It.Is<int>((y,z) => y == z),
        It.Is<int>(() => 1)))
    .Return(true);";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertArgumentConstraints_UsesItIs_WhenArgIsEqualWasUsed()
        {
            // Arrange
            var input =
@"mock
    .Setup(x => x.Foo(
        Arg<int>.Is.Equal(1),
        Arg<int>.Is.Equal(2)))
    .Return(true);";

            // Act
            var actual = input.ConvertArgumentConstraints();

            // Assert
            var expected =
@"mock
    .Setup(x => x.Foo(
        It.Is<int>(arg => arg == 1),
        It.Is<int>(arg => arg == 2)))
    .Return(true);";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertAssertions_UsesVerify_WhenAssertWasCalledWasUsed()
        {
            // Arrange
            var input =
@"mock
    .AssertWasCalled(x => x.Foo(
        Arg<int>.Is.Equal(1),
        Arg<int>.Is.Equal(2)));";

            // Act
            var actual = input.ConvertAssertions();

            // Assert
            var expected =
@"mock
    .Verify(x => x.Foo(
        Arg<int>.Is.Equal(1),
        Arg<int>.Is.Equal(2)));";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertAssertions_UsesVerifyAndTimesNever_WhenAssertWasNotCalledWasUsed()
        {
            // Arrange
            var input =
@"mock
    .AssertWasNotCalled(x => x.Foo(
        Arg<int>.Is.Equal(1),
        Arg<int>.Is.Equal(2)));";

            // Act
            var actual = input.ConvertAssertions();

            // Assert
            var expected =
@"mock
    .Verify(x => x.Foo(
        Arg<int>.Is.Equal(1),
        Arg<int>.Is.Equal(2)), Times.Never);";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertAssertions_UsesVerifyAll_WhenVerifyAllExpectationsWasUsed()
        {
            // Arrange
            var input =
@"mock.VerifyAllExpectations();";

            // Act
            var actual = input.ConvertAssertions();

            // Assert
            var expected =
@"mock.Verify();";

            Assert.AreEqual(expected, actual);
        }
    }
}
