using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace LDRCNetClassification.Tests
{
    [TestFixture]
    public class TestJsonMapper
    {
        private const string JsonFile = "script.json";
        private const string XmlFile = "script.xml";

        [Test]
        public void TestDeserializeForJson()
        {
            // given
            var script = File.ReadAllText(JsonFile);

            // when
            var mappers = JsonConvert.DeserializeObject<List<Mapper>>(script);

            // then
            Assert.That(mappers.Count, Is.EqualTo(1));

            if (mappers.Count == 1)
            {
                var first = mappers.First();
                Assert.That(first.Regex.Count, Is.EqualTo(4));
                Assert.That(first.SymbolName, Is.EqualTo("PowerNetGroup"));
            }
        }

        [Test]
        public void TestDeserializeForXml()
        {
            // given
            var serializer = new XmlSerializer(typeof(XmlMapper));

            using (var reader = File.OpenRead(XmlFile))
            {
                // when
                var mappers = (serializer.Deserialize(reader) as XmlMapper)?.ConvertItems().ToList();

                // then
                Assert.NotNull(mappers);
                Assert.That(mappers.Count, Is.EqualTo(1));

                if (mappers.Count == 1)
                {
                    var first = mappers.First();
                    Assert.That(first.Regex.Count, Is.EqualTo(4));
                    Assert.That(first.SymbolName, Is.EqualTo("PowerNetGroup"));
                }
            }
        }
    }
}