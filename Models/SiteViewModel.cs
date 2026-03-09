namespace IISWebApp.Models;

public class SiteViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PhysicalPath { get; set; } = string.Empty;
    public List<string> Bindings { get; set; } = new();
}
