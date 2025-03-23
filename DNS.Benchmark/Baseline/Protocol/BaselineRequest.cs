using DNS.Benchmark.Baseline.Protocol.ResourceRecords;
using DNS.Benchmark.Baseline.Protocol.Utils;
using DNS.Protocol.ResourceRecords;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace DNS.Benchmark.Baseline.Protocol
{
    public class BaselineRequest : IBaselineRequest
    {
        private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

        private BaselineHeader _header;
        private readonly IList<BaselineQuestion> _questions;

        private readonly IList<IBaselineResourceRecord> _additional;

        public static BaselineRequest FromArray(byte[] message)
        {
            BaselineHeader header = BaselineHeader.FromArray(message);
            int offset = BaselineHeader.SIZE;

            if (header.Response || header.QuestionCount == 0 ||
                    header.AnswerRecordCount + header.AuthorityRecordCount > 0 ||
                    header.ResponseCode != BaselineResponseCode.NoError)

                throw new ArgumentException("Invalid request message");

            return new BaselineRequest(header,
                BaselineQuestion.GetAllFromArray(message, offset, header.QuestionCount, out offset),
                BaselineResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out _));
        }

        public BaselineRequest(BaselineHeader header, IList<BaselineQuestion> questions, IList<IBaselineResourceRecord> additional)
        {
            _header = header;
            _questions = questions;
            _additional = additional;
        }

        public BaselineRequest()
        {
            _questions = [];
            _header = new BaselineHeader();
            _additional = [];

            _header.OperationCode = BaselineOperationCode.Query;
            _header.Response = false;
            _header.Id = NextRandomId();
        }

        public BaselineRequest(IBaselineRequest request)
        {
            _header = new BaselineHeader();
            _questions = [.. request.Questions];
            _additional = [.. request.AdditionalRecords];

            _header.Response = false;

            Id = request.Id;
            OperationCode = request.OperationCode;
            RecursionDesired = request.RecursionDesired;
        }

        public IList<BaselineQuestion> Questions
        {
            get { return _questions; }
        }

        public IList<IBaselineResourceRecord> AdditionalRecords
        {
            get { return _additional; }
        }

        [JsonIgnore]
        public int Size
        {
            get
            {
                return BaselineHeader.SIZE +
                    _questions.Sum(q => q.Size) +
                    _additional.Sum(a => a.Size);
            }
        }

        public int Id
        {
            get => _header.Id; set => _header.Id = value;
        }

        public BaselineOperationCode OperationCode
        {
            get => _header.OperationCode; set => _header.OperationCode = value;
        }

        public bool RecursionDesired
        {
            get => _header.RecursionDesired; set => _header.RecursionDesired = value;
        }

        public byte[] ToArray()
        {
            UpdateHeader();
            BaselineByteStream result = new(Size);

            result
                .Append(_header.ToArray())
                .Append(_questions.Select(q => q.ToArray()))
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
            _header.AdditionalRecordCount = _additional.Count;
        }

        private static ushort NextRandomId()
        {
            byte[] buffer = new byte[sizeof(ushort)];
            _random.GetBytes(buffer);

            return BitConverter.ToUInt16(buffer, 0);
        }
    }
}
