using System;
using System.Collections.Generic;

namespace HR_Management_System.Models;

public partial class Task
{
    public int Id { get; set; }

    public int AssignedToEmployeeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly DueDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee AssignedToEmployee { get; set; } = null!;
}
