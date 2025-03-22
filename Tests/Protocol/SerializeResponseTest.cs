using System;
using Xunit;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol.ResourceRecords {
    
    public class SerializeResponseTest {
        [Fact]
        public void BasicQuestionResponseWithEmptyHeader() {
            Header header = new()
            {
                Response = true
            };

            Domain domain = new(Helper.GetArray<string>());
            Question question = new(domain, RecordType.A, RecordClass.IN);
            ResourceRecord record = new(domain, Helper.GetArray<byte>(0, 0, 0, 0), 
                RecordType.A, RecordClass.IN, new TimeSpan());

            Response response = new(header,
                Helper.GetList(question), 
                Helper.GetList<IResourceRecord>(record),
                Helper.GetList<IResourceRecord>(record),
                Helper.GetList<IResourceRecord>(record));

            byte[] content = Helper.ReadFixture("Response", "empty-header_basic");

            Assert.Equal(content, response.ToArray());
        }

        [Fact]
        public void RequestWithHeaderAndResourceRecords() {
            Header header = new()
            {
                Response = true
            };

            Domain domain = new(Helper.GetArray("www", "google", "com"));
            Question question = new(domain, RecordType.A, RecordClass.IN);

            Domain domain1 = new(Helper.GetArray("www", "google", "com"));
            ResourceRecord record1 = new(domain1, Helper.GetArray<byte>(3, 119, 119, 119, 0),
                RecordType.CNAME, RecordClass.IN, TimeSpan.FromSeconds(1));

            Domain domain2 = new(Helper.GetArray("dr", "dk"));
            ResourceRecord record2 = new(domain2, Helper.GetArray<byte>(1, 1, 1, 1),
                RecordType.A, RecordClass.ANY, TimeSpan.FromSeconds(0));

            Domain domain3 = new(Helper.GetArray("www"));
            ResourceRecord record3 = new(domain3, Helper.GetArray<byte>(192, 12),
                RecordType.CNAME, RecordClass.ANY, TimeSpan.FromSeconds(1));

            Response response = new(header,
                Helper.GetList(question),
                Helper.GetList<IResourceRecord>(record1),
                Helper.GetList<IResourceRecord>(record2),
                Helper.GetList<IResourceRecord>(record3))
            {
                Id = 1,
                RecursionAvailable = true
            };

            byte[] content = Helper.ReadFixture("Response", "id-ra_all");

            Assert.Equal(content, response.ToArray());
        }
    }
}
