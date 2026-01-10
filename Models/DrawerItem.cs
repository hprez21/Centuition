using Telerik.SvgIcons;

namespace CentuitionApp.Models;

public class DrawerItem
{    
    public string Text { get; set; } = string.Empty;
    public ISvgIcon? Icon { get; set; }
    public string? Url { get; set; }
    public bool IsSeparator { get; set; }    
    public bool IsLogout { get; set; }
}
