using DNS.Benchmark.Baseline.Protocol.ResourceRecords;
using DNS.Benchmark.Baseline.Protocol.Utils;
using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol
{
    public class BaselineResponse : IBaselineResponse
    {
        private BaselineHeader _header;
        private readonly IList<BaselineQuestion> _questions;
        private readonly IList<IBaselineResourceRecord> _answers;
        private readonly IList<IBaselineResourceRecord> _authority;
        private readonly IList<IBaselineResourceRecord> _additional;

        public static BaselineResponse FromRequest(IBaselineRequest request)
        {
            BaselineResponse response = new()
            {
                Id = request.Id
            };

            foreach (BaselineQuestion question in request.Questions)
                response.Questions.Add(question);

            return response;
        }

        public static BaselineResponse FromArray(byte[] message)
        {
            BaselineHeader header = BaselineHeader.FromArray(message);
            int offset = BaselineHeader.SIZE;

            if (!header.Response)
                throw new ArgumentException("Invalid response message");

            if (header.Truncated)
                return new BaselineResponse(header,
                    BaselineQuestion.GetAllFromArray(message, offset, header.QuestionCount), [], [], []);

            return new BaselineResponse(header,
                BaselineQuestion.GetAllFromArray(message, offset, header.QuestionCount, out offset),
                BaselineResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
                BaselineResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
                BaselineResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out _));
        }

        public BaselineResponse(BaselineHeader header, IList<BaselineQuestion> questions, IList<IBaselineResourceRecord> answers,
                IList<IBaselineResourceRecord> authority, IList<IBaselineResourceRecord> additional)
        {
            _header = header;
            _questions = questions;
            _answers = answers;
            _authority = authority;
            _additional = additional;
        }

        public BaselineResponse()
        {
            _header = new BaselineHeader();
            _questions = [];
            _answers = [];
            _authority = [];
            _additional = [];

            _header.Response = true;
        }

        public BaselineResponse(IBaselineResponse response)
        {
            _header = new BaselineHeader();
            _questions = [.. response.Questions];
            _answers = [.. response.AnswerRecords];
            _authority = [.. response.AuthorityRecords];
            _additional = [.. response.AdditionalRecords];

            _header.Response = true;

            Id = response.Id;
            RecursionAvailable = response.RecursionAvailable;
            AuthorativeServer = response.AuthorativeServer;
            OperationCode = response.OperationCode;
            ResponseCode = response.ResponseCode;
        }

        public IList<BaselineQuestion> Questions
        {
            get { return _questions; }
        }

        public IList<IBaselineResourceRecord> AnswerRecords
        {
            get { return _answers; }
        }

        public IList<IBaselineResourceRecord> AuthorityRecords
        {
            get { return _authority; }
        }

        public IList<IBaselineResourceRecord> AdditionalRecords
        {
            get { return _additional; }
        }

        public int Id
        {
            get => _header.Id; set => _header.Id = value;
        }

        public bool RecursionAvailable
        {
            get => _header.RecursionAvailable; set => _header.RecursionAvailable = value;
        }

        public bool AuthenticData
        {
            get => _header.AuthenticData; set => _header.AuthenticData = value;
        }

        public bool CheckingDisabled
        {
            get => _header.CheckingDisabled; set => _header.CheckingDisabled = value;
        }

        public bool AuthorativeServer
        {
            get => _header.AuthorativeServer; set => _header.AuthorativeServer = value;
        }

        public bool Truncated
        {
            get => _header.Truncated; set => _header.Truncated = value;
        }

        public BaselineOperationCode OperationCode
        {
            get => _header.OperationCode; set => _header.OperationCode = value;
        }

        public BaselineResponseCode ResponseCode
        {
            get => _header.ResponseCode; set => _header.ResponseCode = value;
        }

        public int Size
        {
            get
            {
                return BaselineHeader.SIZE +
                    _questions.Sum(q => q.Size) +
                    _answers.Sum(a => a.Size) +
                    _authority.Sum(a => a.Size) +
                    _additional.Sum(a => a.Size);
            }
        }

        public byte[] ToArray()
        {
            UpdateHeader();
            BaselineByteStream result = new(Size);

            result
                .Append(_header.ToArray())
                .Append(_questions.Select(q => q.ToArray()))
                .Append(_answers.Select(a => a.ToArray()))
                .Append(_authority.Select(a => a.ToArray()))
                .Append(_additional.Select(a => a.ToArray()));

            return result.ToArray();
        }

        public override string? ToString()
        {
            UpdateHeader();

            return base.ToString();
        }

        private void UpdateHeader()
        {
            _header.QuestionCount = _questions.Count;
            _header.AnswerRecordCount = _answers.Count;
            _header.AuthorityRecordCount = _authority.Count;
            _header.AdditionalRecordCount = _additional.Count;
        }
    }
}
