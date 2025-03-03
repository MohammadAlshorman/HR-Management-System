using System;
using System.Collections.Generic;

namespace HR_Management_System.Models;

public partial class VaccationRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string? VaccType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? VaccDays { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
