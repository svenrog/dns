using DNS.Protocol;
using Xunit;

namespace DNS.Tests.Protocol;


public class SerializeHeaderTest
{
    [Fact]
    public void EmptyHeader()
    {
        Header header = new();
        byte[] content = Helper.ReadFixture("Header", "empty");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithId()
    {
        Header header = new()
        {
            Id = 1
        };
        byte[] content = Helper.ReadFixture("Header", "id");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithQueryResponseFlag()
    {
        Header header = new()
        {
            Response = true
        };
        byte[] content = Helper.ReadFixture("Header", "qr");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithOperationCode()
    {
        Header header = new()
        {
            OperationCode = OperationCode.Status
        };
        byte[] content = Helper.ReadFixture("Header", "opcode");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithAuthorativeAnswerFlag()
    {
        Header header = new()
        {
            AuthorativeServer = true
        };
        byte[] content = Helper.ReadFixture("Header", "aa");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithTruncatedFlag()
    {
        Header header = new()
        {
            Truncated = true
        };
        byte[] content = Helper.ReadFixture("Header", "tc");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithRecusionDesiredFlag()
    {
        Header header = new()
        {
            RecursionDesired = true
        };
        byte[] content = Helper.ReadFixture("Header", "rd");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithRecusionAvailableFlag()
    {
        Header header = new()
        {
            RecursionAvailable = true
        };
        byte[] content = Helper.ReadFixture("Header", "ra");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithAuthenticDataFlag()
    {
        Header header = new()
        {
            AuthenticData = true
        };
        byte[] content = Helper.ReadFixture("Header", "ad");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithCheckingDisabledFlag()
    {
        Header header = new()
        {
            CheckingDisabled = true
        };
        byte[] content = Helper.ReadFixture("Header", "cd");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithResponseCode()
    {
        Header header = new()
        {
            ResponseCode = ResponseCode.ServerFailure
        };
        byte[] content = Helper.ReadFixture("Header", "rcode");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithQuestionCount()
    {
        Header header = new()
        {
            QuestionCount = 1
        };
        byte[] content = Helper.ReadFixture("Header", "qdcount");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithAnswerRecordCount()
    {
        Header header = new()
        {
            AnswerRecordCount = 1
        };
        byte[] content = Helper.ReadFixture("Header", "ancount");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithAuthorityRecordCount()
    {
        Header header = new()
        {
            AuthorityRecordCount = 1
        };
        byte[] content = Helper.ReadFixture("Header", "nscount");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithAdditionalRecordCount()
    {
        Header header = new()
        {
            AdditionalRecordCount = 1
        };
        byte[] content = Helper.ReadFixture("Header", "arcount");

        Assert.Equal(content, header.ToArray());
    }

    [Fact]
    public void HeaderWithAllSet()
    {
        Header header = new()
        {
            Id = 1,
            Response = true,
            OperationCode = OperationCode.Status,
            AuthorativeServer = true,
            Truncated = true,
            RecursionDesired = true,
            RecursionAvailable = true,
            AuthenticData = true,
            CheckingDisabled = true,
            ResponseCode = ResponseCode.ServerFailure,
            QuestionCount = 1,
            AnswerRecordCount = 1,
            AuthorityRecordCount = 1,
            AdditionalRecordCount = 1
        };

        byte[] content = Helper.ReadFixture("Header", "all");

        Assert.Equal(content, header.ToArray());
    }
}
