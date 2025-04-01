using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System;
using System.Collections.Generic;
using Xunit;

namespace DNS.Tests.Protocol
{

    public class SerializeRequestTest
    {
        [Fact]
        public void BasicQuestionRequestWithEmptyHeader()
        {
            Header header = new();

            Domain domain = new(Helper.GetArray<string>());
            Question question = new(domain, RecordType.A, RecordClass.IN);
            IList<Question> questions = Helper.GetList(question);

            Request request = new(header, questions, new List<IResourceRecord>());

            byte[] content = Helper.ReadFixture("Request", "empty-header_basic-question");

            Assert.Equal(content, request.ToArray());
        }

        [Fact]
        public void SingleQuestionRequestWithHeader()
        {
            Header header = new();

            Domain domain = new(Helper.GetArray("www", "google", "com"));
            Question question = new(domain, RecordType.CNAME, RecordClass.IN);
            IList<Question> questions = Helper.GetList(question);

            Request request = new(header, questions, new List<IResourceRecord>())
            {
                Id = 1,
                RecursionDesired = true
            };

            byte[] content = Helper.ReadFixture("Request", "id-rd_www.google.com-cname");

            Assert.Equal(content, request.ToArray());
        }

        [Fact]
        public void RequestWithMultipleQuestions()
        {
            Header header = new();

            Domain domain1 = new(Helper.GetArray("www", "google", "com"));
            Question question1 = new(domain1, RecordType.CNAME, RecordClass.IN);

            Domain domain2 = new(Helper.GetArray("dr", "dk"));
            Question question2 = new(domain2, RecordType.A, RecordClass.ANY);

            Request request = new(header, new List<Question>(), new List<IResourceRecord>())
            {
                Id = 1,
                RecursionDesired = true
            };
            request.Questions.Add(question1);
            request.Questions.Add(question2);

            byte[] content = Helper.ReadFixture("Request", "multiple-questions");

            Assert.Equal(content, request.ToArray());
        }

        [Fact]
        public void RequestWithAdditionalRecords()
        {
            Header header = new();

            Domain domain1 = new(Helper.GetArray("google", "com"));
            Domain domain2 = new(string.Empty);
            Question question = new(domain1, RecordType.A, RecordClass.IN);
            ResourceRecord record = new(domain2, Helper.GetArray<byte>(),
                RecordType.OPT, (RecordClass)4096, TimeSpan.FromSeconds(0));

            Request request = new(header,
                Helper.GetList(question),
                Helper.GetList<IResourceRecord>(record))
            {
                Id = 11772,
                RecursionDesired = true
            };

            byte[] content = Helper.ReadFixture("Request", "edns-test");

            Assert.Equal(content, request.ToArray());
        }
    }
}
