namespace CivicFix.Domain.Entities;

public class WorkerProfile
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public string Specialization { get; set; } = string.Empty; // e.g. "Plumbing", "Electrical", "Road Work"
    public string AssignedWardOrZone { get; set; } = string.Empty; // e.g. "Sector 14 / Ward 3"
    public int ActiveJobsCount { get; set; } = 0;
    public int MaxCapacity { get; set; } = 5;
    public bool IsAvailable { get; set; } = true;
    public double Rating { get; set; } = 5.0;
    public int TotalCompletedJobs { get; set; } = 0;
}
