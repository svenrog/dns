namespace DNS.Benchmark.Baseline.Protocol;

public enum BaselineResponseCode
{
    NoError = 0,
    FormatError,
    ServerFailure,
    NameError,
    NotImplemented,
    Refused,
    YXDomain,
    YXRRSet,
    NXRRSet,
    NotAuth,
    NotZone,
}
