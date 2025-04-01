using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                 .Run(Environment.GetCommandLineArgs());


//var benchmarks = new DomainBenchmarks();

//benchmarks.Setup();

//for (var i = 0; i < 100_000_000; i++)
//{
//    benchmarks.StringDomainParseArray();
//}