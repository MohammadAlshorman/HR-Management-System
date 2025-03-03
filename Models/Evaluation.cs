using System;
using System.Collections.Generic;

namespace HR_Management_System.Models;

public partial class Evaluation
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Status { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime? DateEvaluate { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
