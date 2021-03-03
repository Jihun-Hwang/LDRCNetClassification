using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Pentacube.CAx.ECAD.LcadIf.Model.PLDB;
using Pentacube.Infrastructure.Service.Helper;

namespace LDRCNetClassification
{
    public struct MapperType
    {
        [CanBeNull]
        public Regex Regex { get; }

        [CanBeNull]
        public string SymbolName { get; }

        public MapperType([CanBeNull] Regex regex, [CanBeNull] string symbolName)
        {
            Regex = regex;
            SymbolName = symbolName;
        }

        public Mapper Convert()
        {
            return new Mapper
            {
                Regex = Regex.ToEnumerable().IgnoreNull().ToList(),
                SymbolName = SymbolName,
            };
        }

        public override string ToString() => $"{SymbolName} - Regex: {Regex}";
    }

    [JsonObject]
    public class Mapper
    {
        [JsonProperty]
        [NotNull, ItemNotNull]
        public List<Regex> Regex { get; set; } = new List<Regex>();

        [JsonProperty]
        [CanBeNull]
        public string SymbolName { get; set; }

        [JsonIgnore]
        [CanBeNull]
        public Net Net { get; private set; }

        [JsonIgnore]
        public bool IsValid =>
            Regex.Any() && !string.IsNullOrWhiteSpace(SymbolName) && Net != null;

        public bool IsMatch(Net net) => Regex.Any(r => r.IsMatch(net?.Name ?? string.Empty));

        public Mapper AddNet([CanBeNull] Net net)
        {
            Net = net;
            return this;
        }

        public override string ToString() =>
            $"{SymbolName} - Regex Count: {Regex.Count}, Net: {Net?.Name ?? "{Empty}"}";
    }

    [XmlRoot("Classifications")]
    public class XmlMapper
    {
        [XmlElement("NetClassification")]
        public List<XmlMapperItem> Items { get; set; }

        public IEnumerable<Mapper> ConvertItems()
        {
            return Items.Select(mp =>
            {
                return new Mapper
                {
                    SymbolName = mp.SymbolName,
                    Regex = mp.Regex.Select(r => new Regex(r)).ToList(),
                };
            });
        }

        public class XmlMapperItem
        {
            [XmlArray("RegexItems"), XmlArrayItem("Regex", typeof(string))]
            [NotNull, ItemNotNull]
            public List<string> Regex { get; set; } = new List<string>();

            [XmlElement]
            [CanBeNull]
            public string SymbolName { get; set; }
        }
    }
}