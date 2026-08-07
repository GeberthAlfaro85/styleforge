namespace StyleForge.Application.DTOs.BusinessHours;

public class UpdateBusinessHoursRequest
{
    public List<BusinessHourItem> Days { get; set; } = new();
}

public class BusinessHourItem
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
}
