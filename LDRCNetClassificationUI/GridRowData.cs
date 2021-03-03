namespace LDRCNetClassificationUI
{
    public class GridRowData
    {
        public int Idx { get; }
        public string NetName { get; }
        public string Classification { get; }

        public GridRowData(int idx, string netName, string classification)
        {
            Idx = idx;
            NetName = netName ?? string.Empty;
            Classification = classification;
        }
    }
}
