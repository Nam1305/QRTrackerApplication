using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Account
{
    public int SupervisorId { get; set; }

    public string Name { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}
