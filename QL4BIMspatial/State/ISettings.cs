namespace QL4BIMspatial
{
    public interface ISettings
    {
        Settings.DistanceSetting Distance { get; set; }
        Settings.OverlapSetting Overlap { get; set; }
        Settings.TouchSetting Touch { get; set; }
        Settings.DirectionSetting Direction { get; set; }
        Settings.ContainSetting Contain { get; set; }
        Settings.EqualSetting Equal { get; set; }
        Settings.LogSetting Log { get; set; }
        Settings.RTreeSetting RsTreeSetting { get; set; }
    }
}