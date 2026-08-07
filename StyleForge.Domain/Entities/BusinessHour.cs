namespace StyleForge.Domain.Entities;

/// <summary>
/// Horario de atención del salón para un día de la semana. Un tenant tiene a lo sumo un registro por día.
/// </summary>
public class BusinessHour : BaseEntity
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
}
