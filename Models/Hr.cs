using System;
using System.Collections.Generic;

namespace HR_Management_System.Models;

public partial class Hr
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? ProfileImage { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }
}
