using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Pentacube.CAx.DFx.LDRC.Model;
using System.Linq;
using CubicLDRC.Infrastructure.Helper;
using JetBrains.Annotations;
using Microsoft.Practices.ObjectBuilder2;

namespace LDRCNetClassification
{
    public sealed class NetNameMapping
    {
        private readonly string[] AllowFileType = { ".csv", ".json", ".xml" };

        // 모든 mapping 결과를 하나의 리스트에 저장함
        public List<ISetConfigure> NetConfigInfoList { get; } = new List<ISetConfigure>();
        // Regex ref, classification
        public List<Mapper> MapperTypeList { get; } = new List<Mapper>();

        /// <summary>
        /// Called from UI, read regex file(csv) and fill the RegexCategorization dictionary
        /// </summary>
        /// <param name="path">regex file path</param>
        public bool TryLoadRegexConfig([CanBeNull] string path)
        {
            if (!File.Exists(path) || AllowFileType.All(type => type != Path.GetExtension(path)?.ToLower()))
                return false;

            switch (Path.GetExtension(path).ToLower())
            {
                case ".csv":
                    {
                        var mapperTypes = ParseFromCsv(path);
                        MapperTypeList.AddRange(mapperTypes.Select(m => m.Convert()));

                        return true;
                    }

                case ".json":
                    {
                        var script = File.ReadAllText(path);
                        var result = JsonSerializationHelper.TryDeSerialize<List<Mapper>>(script, out var mappers);
                        MapperTypeList.AddRange(mappers);

                        return result;
                    }

                case ".xml":
                    {
                        var mappers = XmlSerializationHelper.Deserialize<XmlMapper>(path)?.ConvertItems();
                        if (mappers != null)
                            MapperTypeList.AddRange(mappers);

                        return mappers != null;
                    }

                default: return false;
            }
        }

        private IEnumerable<MapperType> ParseFromCsv(string path)
        {
            using (var sr = new StreamReader(path))
            {
                string curPair;
                while ((curPair = sr.ReadLine()) != null)
                {
                    var tokens = curPair.Split(',');
                    if (tokens.Length < 2)
                        continue;

                    var regex = new Regex(tokens[0]);
                    var symbolName = tokens[1];

                    yield return new MapperType(regex, symbolName);
                }
            }
        }

        /// <summary>
        /// Called from UI, fill 8 Group list and NetConfigInfoList
        /// </summary>
        /// <param name="path">net name file path</param>
        /// <returns></returns>
        public bool AddToNetGroups([CanBeNull] string path)
        {
            if (!File.Exists(path))
                return false;

            var design = PLDBHelper.GetDesignFrom(path);  // Nets dictionary
            var configVm = PLDBHelper.GetPLDBRepo();      // 9개 Group List 참조

            design.Nets.Values
                .Select(net => MapperTypeList.FirstOrDefault(mapper => mapper.IsMatch(net))?.AddNet(net))
                .Where(mapper => mapper?.IsValid ?? false)
                .ForEach(mapper =>
                {
                    var configureInfo = GetConfigureInfo(mapper);
                    if (configureInfo != null)
                        NetConfigInfoList.Add(configureInfo);
                });

            NetConfigInfoList.ForEach(netInfo => netInfo.AppendTo(configVm));

            return true;
        }

        private ISetConfigure GetConfigureInfo(Mapper mapper)
        {
            switch (mapper.SymbolName)
            {
                case nameof(PowerNetGroup):
                    var pg = new PowerNetGroup { PowerNetName = mapper.Net.Name };
                    return new NetInfo<PowerNetGroup>(pg, mapper.SymbolName, mapper.Net);

                case nameof(ILLGroup):
                    var il = new ILLGroup { ILLNetName = mapper.Net.Name };
                    return new NetInfo<ILLGroup>(il, mapper.SymbolName, mapper.Net);

                case nameof(AnalogNetGroup):
                case "AnalogGroup":
                    var an = new AnalogNetGroup { AnalogNetName = mapper.Net.Name };
                    return new NetInfo<AnalogNetGroup>(an, mapper.SymbolName, mapper.Net);

                case nameof(LINGroup):
                    var li = new LINGroup { Bus = mapper.Net.Name };
                    return new NetInfo<LINGroup>(li, mapper.SymbolName, mapper.Net);

                case "HighSideOutGroup":
                    var hs = new SideOutGroup { OutputNetName = mapper.Net.Name };
                    return new NetInfo<SideOutGroup>(hs, mapper.SymbolName, mapper.Net);

                case "LowSideOutGroup":
                    var ls = new SideOutGroup { OutputNetName = mapper.Net.Name };
                    return new NetInfo<SideOutGroup>(ls, mapper.SymbolName, mapper.Net);

                case "ActiveHighInputGroup":
                    var ah = new ActiveInputGroup { InputNetName = mapper.Net.Name };
                    return new NetInfo<ActiveInputGroup>(ah, mapper.SymbolName, mapper.Net);

                case "ActiveLowInputGroup":
                    var al = new ActiveInputGroup { InputNetName = mapper.Net.Name };
                    return new NetInfo<ActiveInputGroup>(al, mapper.SymbolName, mapper.Net);

                case nameof(CANGroup):
                    var ca = new CANGroup { HighNet = mapper.Net.Name };
                    return new NetInfo<CANGroup>(ca, mapper.SymbolName, mapper.Net);

                default: return null;
            }
        }
    }
}
