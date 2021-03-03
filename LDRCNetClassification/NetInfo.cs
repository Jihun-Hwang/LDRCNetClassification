using Pentacube.CAx.ECAD.LcadIf.Model.PLDB;
using System;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Pentacube.CAx.DFx.LDRC.Model;
using Pentacube.CAx.DFx.LDRC.Views;
using Pentacube.Infrastructure.Service.Filter;

namespace LDRCNetClassification
{
    public interface ISetConfigure
    {
        [CanBeNull]
        Net Net { get; }

        [CanBeNull]
        string SymbolName { get; }

        void AppendTo([CanBeNull] DRCConfig configure);
        string GetSymbolName();
    }

    public sealed class NetInfo<T> : ISetConfigure
        where T : class
    {
        [CanBeNull]
        public T Group { get; }

        public Net Net { get; }
        public string SymbolName { get; }

        public NetInfo([CanBeNull] string symbolName, [NotNull] Net net) : this(null, symbolName, net)
        {
        }

        public NetInfo([CanBeNull] T group, [CanBeNull] string symbolName, [NotNull] Net net)
        {
            Group = group;
            SymbolName = symbolName;
            Net = net ?? throw new ArgumentNullException(nameof(net));
        }

        public void AppendTo([CanBeNull] DRCConfig configure)
        {
            if (configure == null || Group == null || string.IsNullOrWhiteSpace(SymbolName))
                return;

            var propertyName = GetSymbolGroupName();

            var groupProp = configure.GetProperty(propertyName, true);
            if (groupProp == null)
                return;

            (groupProp.GetValue(configure) as Collection<T>)?.Add(Group);
        }

        private string GetSymbolGroupName()
        {
            return GetSymbolName() + "s";
        }

        public string GetSymbolName()
        {
            switch (SymbolName)
            {
                case nameof(AnalogNetGroup): return "AnalogGroup";
                default: return SymbolName;
            }
        }
    }
}
