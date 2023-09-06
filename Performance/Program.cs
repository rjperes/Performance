using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Performance
{
    public class Target
    {
        public int A { get; set; } = 0;
        public string B { get; set; } = string.Empty;
    }

    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class Instantiator
    {
        static readonly Type _type = typeof(Target);
        static readonly ConstructorInfo _ci = _type.GetConstructor(Type.EmptyTypes);
        static readonly Expression<Func<Target>> _exp = () => new Target();
        static readonly Func<Target> _func = _exp.Compile();
        static readonly Func<object> _creator;

        static Instantiator()
        {
            var method = new DynamicMethod(string.Empty, typeof(object), null, _type, true);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Newobj, _ci);
            il.Emit(OpCodes.Ret);

            _creator = method.CreateDelegate(typeof(Func<object>)) as Func<object>;
        }

        [Benchmark]
        public Target UsingActivator()
        {
            var type = typeof(Target);
            var target = Activator.CreateInstance(type) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingActivatorWithCache()
        {
            var target = Activator.CreateInstance(_type) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingConstructor()
        {
            var type = typeof(Target);
            var ci = type.GetConstructor(Type.EmptyTypes);
            var target = ci.Invoke(null) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingConstructorWithCache()
        {
            var target = _ci.Invoke(null) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingDynamicMethod()
        {
            var type = typeof(Target);
            var ci = type.GetConstructor(Type.EmptyTypes);
            var m = new DynamicMethod(string.Empty, typeof(object), null, type, true);

            var il = m.GetILGenerator();
            il.Emit(OpCodes.Newobj, ci);
            il.Emit(OpCodes.Ret);

            var creator = m.CreateDelegate(typeof(Func<object>)) as Func<object>;
            var target = creator() as Target;
            return target;
        }

        [Benchmark]
        public Target UsingDynamicMethodWithCache()
        {
            var target = _creator() as Target;
            return target;
        }

        [Benchmark]
        public Target UsingRuntimeHelpers()
        {
            var type = typeof(Target);
            var target = RuntimeHelpers.GetUninitializedObject(type) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingRuntimeHelpersWithCache()
        {
            var target = RuntimeHelpers.GetUninitializedObject(_type) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingFormatterServices()
        {
            var type = typeof(Target);
            var target = FormatterServices.GetUninitializedObject(type) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingFormatterServicesWithCache()
        {
            var target = FormatterServices.GetUninitializedObject(_type) as Target;
            return target;
        }

        [Benchmark]
        public Target UsingExpression()
        {
            var type = typeof(Target);
            Expression<Func<Target>> exp = () => new Target();
            var func = exp.Compile();
            var target = func();
            return target;
        }

        [Benchmark]
        public Target UsingExpressionWithCache()
        {
            var target = _func();
            return target;
        }

        [Benchmark(Baseline = true)]
        public Target UsingNew()
        {
            var target = new Target();
            return target;
        }
    }

    internal class Program
    {        
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Instantiator>();
        }
    }
}