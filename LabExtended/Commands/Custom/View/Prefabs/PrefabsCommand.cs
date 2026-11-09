using LabExtended.API.Prefabs;
using LabExtended.Commands.Attributes;
using LabExtended.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.View;

public partial class ViewCommand
{
    /// <summary>
    /// Lists the names of all available prefabs to the command output.
    /// </summary>
    /// <remarks>This method is intended to be used as a command overload for displaying all registered prefab
    /// names. The output includes the total count of prefabs followed by each prefab's identifier and name.</remarks>
    [CommandOverload("prefabs", "Lists prefab names.", "view.prefabs")]
    public void PrefabsOverload()
    {
        Ok(x =>
        {
            x.AppendLine($"&3{PrefabList.AllPrefabs.Count}&r prefabs (active holiday: &3{PrefabList.ActiveHoliday}&r)");

            foreach (var pair in PrefabList.AllPrefabs)
            {
                if (pair.Value.IsLoaded)
                    x.AppendLine($"[&2LOADED&r - &2{pair.Key}&r]: &3{pair.Value.Name}&r (&3Holiday&r: {pair.Value.ActiveHolidays.NullOrValueTrueColor(holiday => holiday.ToString())} | &3IsHolidayActive&r: {pair.Value.IsHolidayActive.TrueColorFormatBool()})");
                else
                    x.AppendLine($"[&1MISSING&r - &1{pair.Key}&r]: &3{pair.Value.Name}&r (&3Holiday&r: {pair.Value.ActiveHolidays.NullOrValueTrueColor(holiday => holiday.ToString())} | &3IsHolidayActive&r: {pair.Value.IsHolidayActive.TrueColorFormatBool()})");
            }
        });
    }
}