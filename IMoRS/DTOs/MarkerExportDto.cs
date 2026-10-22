namespace IMoRS.DTOs;

public class MarkerExportDto
{
    public int Id { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Scale { get; set; } = 0.2;

    public string? Description { get; set; }

    public string? IconBase64 { get; set; }
    public string? IconExtension { get; set; }
    
    public bool IsCustomIcon { get; set; }
}