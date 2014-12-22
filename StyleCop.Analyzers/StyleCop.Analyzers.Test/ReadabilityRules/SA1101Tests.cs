using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleCop.Analyzers.ReadabilityRules;
using TestHelper;

namespace StyleCop.Analyzers.Test.ReadabilityRules
{
    [TestClass]
    public class SA1101Tests : CodeFixVerifier
    {
        private const string DiagnosticId = SA1101PrefixLocalCallsWithThis.DiagnosticId;
        protected static readonly DiagnosticResult[] EmptyDiagnosticResults = { };

        [TestMethod]
        public async Task TestEmptySource()
        {
            var testCode = @"";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalFieldWithoutThis()
        {
            var testCode = @"
public class Foo
{
    private string name;

    public void Bar()
    {
        var localName = name;
    }
}";
            var expectedResult = new[]
            {
                new DiagnosticResult
                {
                    Id = DiagnosticId,
                    Message = "A call to an instance member of the local class or a base class is not prefixed with 'this.'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 8, 25)
                        }
                }
            };

            await VerifyCSharpDiagnosticAsync(testCode, expectedResult, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalFieldWhichHidesBaseField()
        {
            var testCode = @"
public class FooParent
{
    protected string name;
}
public class Foo : FooParent
{
    private string name;

    public void Bar()
    {
        var localName = name;
    }
}";
            var expectedResult = new[]
            {
                new DiagnosticResult
                {
                    Id = DiagnosticId,
                    Message = "A call to an instance member of the local class or a base class is not prefixed with 'this.'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 12, 25)
                        }
                }
            };

            await VerifyCSharpDiagnosticAsync(testCode, expectedResult, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessStaticField()
        {
            var testCode = @"
public class Foo
{
    private static string name;

    public void Bar()
    {
        var localName = name;
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }


        [TestMethod]
        public async Task TestAccessLocalFieldWitThis()
        {
            var testCode = @"
public class Foo
{
    private string name;

    public void Bar()
    {
        var localName = this.name;
    }
}";
            
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalMethodWithoutThis()
        {
            var testCode = @"
public class Foo
{
    private void Baz()
    {

    }

    public void Bar()
    {
        Baz();
    }
}";
            var expectedResult = new[]
            {
                new DiagnosticResult
                {
                    Id = DiagnosticId,
                    Message = "A call to an instance member of the local class or a base class is not prefixed with 'this.'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 11, 9)
                        }
                }
            };

            await VerifyCSharpDiagnosticAsync(testCode, expectedResult, CancellationToken.None);
        }
        
        [TestMethod]
        public async Task TestAccessStaticMethod()
        {
            var testCode = @"
public class Foo
{
    private static void Baz()
    {

    }

    public void Bar()
    {
        Baz();
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalMethodWithThis()
        {
            var testCode = @"
public class Foo
{
    private void Baz()
    {

    }

    public void Bar()
    {
        this.Baz();
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalMethodWithBaseNoOverride()
        {
            var testCode = @"
public class FooParent
{
    public virtual void Fun()
    {

    }
}
public class Foo : FooParent
{
    private void Baz()
    {

    }

    public void Bar()
    {
        base.Fun();
    }
}";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalMethodWithBaseOverrideExists()
        {
            var testCode = @"
public class FooParent
{
    public virtual void Fun()
    {

    }
}
public class Foo : FooParent
{
    private void Baz()
    {

    }

    public void Bar()
    {
        base.Fun();
    }

    public override void Fun()
    {

    }
}";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalPropertyWithoutThis()
        {
            var testCode = @"
public class Foo
{
    public string Name {get;set;}

    public void Bar()
    {
        var name = Name;
    }
}";
            var expectedResult = new[]
            {
                new DiagnosticResult
                {
                    Id = DiagnosticId,
                    Message = "A call to an instance member of the local class or a base class is not prefixed with 'this.'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 8, 20)
                        }
                }
            };

            await VerifyCSharpDiagnosticAsync(testCode, expectedResult, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessStaticProperty()
        {
            var testCode = @"
public class Foo
{
    public static string Name {get;set;}

    public void Bar()
    {
        var name = Name;
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalPropertyWithBaseNoOverride()
        {
            var testCode = @"
public class FooParent
{
    public virtual string Prop
    {
        get;set;
    }
}
public class Foo : FooParent
{
    private void Baz()
    {

    }

    public void Bar()
    {
        var s = base.Prop;
    }
}";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalPropertyWithBaseOverrideExists()
        {
            var testCode = @"
public class FooParent
{
    public virtual string Prop
    {
        get;set;
    }
}
public class Foo : FooParent
{
    private void Baz()
    {

    }

    public void Bar()
    {
        var s = base.Prop;
    }

    public override string Prop
    {
        get;set;
    }
}";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalPropertyWithThis()
        {
            var testCode = @"
public class Foo
{
    public string Name {get;set;}

    public void Bar()
    {
        var name = this.Name;
    }
}";
             
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalEventWithoutThis()
        {
            var testCode = @"
public class Foo
{
    public event Action MyEvent;

    public void Bar()
    {
        if(MyEvent != null)
        {
            MyEvent();

        }
    }
}";
            var expectedResult = new[]
            {
                new DiagnosticResult
                {
                    Id = DiagnosticId,
                    Message = "A call to an instance member of the local class or a base class is not prefixed with 'this.'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 8, 12)
                        }
                },
                new DiagnosticResult
                {
                    Id = DiagnosticId,
                    Message = "A call to an instance member of the local class or a base class is not prefixed with 'this.'",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
                }
            };

            await VerifyCSharpDiagnosticAsync(testCode, expectedResult, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalStaticEvent()
        {
            var testCode = @"
public class Foo
{
    public static event Action MyEvent;

    public void Bar()
    {
        if(MyEvent != null)
        {
            MyEvent();

        }
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalEventFromBaseClassNoOverride()
        {
            var testCode = @"
public class FooParent
{
    public virtual event Action MyEvent;
}
public class Foo : FooParent
{
    public void Bar()
    {
        if(base.MyEvent != null)
        {
            base.MyEvent();

        }
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalEventFromBaseClassOverrideExists()
        {
            var testCode = @"
public class FooParent
{
    public virtual event Action MyEvent;
}
public class Foo : FooParent
{
    public override event Action MyEvent;

    public void Bar()
    {
        if(base.MyEvent != null)
        {
            base.MyEvent();

        }
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalEventWithThis()
        {
            var testCode = @"
public class Foo
{
    public event Action MyEvent;

    public void Bar()
    {
        if(this.MyEvent != null)
        {
            this.MyEvent();

        }
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalFieldInStruct()
        {
            var testCode = @"
public struct Foo
{
    private string baz;

    public void Bar()
    {
        var name = baz;
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalPropertyInStruct()
        {
            var testCode = @"
public struct Foo
{
    private string Baz {get;set};

    public void Bar()
    {
        var name = Baz;
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        [TestMethod]
        public async Task TestAccessLocalMethodInStruct()
        {
            var testCode = @"
public struct Foo
{
    private void Baz() 
    {

    }

    public void Bar()
    {
        Baz();
    }
}";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SA1101PrefixLocalCallsWithThis();
        }
    }
}