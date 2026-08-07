namespace StyleForge.Application.DTOs.BusinessHours;

public class BusinessHourDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
}
