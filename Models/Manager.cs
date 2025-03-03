using System;
using System.Collections.Generic;

namespace HR_Management_System.Models;

public partial class Manager
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? ProfileImage { get; set; }

    public string? Address { get; set; }

    public int DepartmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
