using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                 .Run(Environment.GetCommandLineArgs());

//using DNS.Benchmark;

//var benchmarks = new RequestBenchmarks();

//benchmarks.Setup();