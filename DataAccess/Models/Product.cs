using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public int QuantityPerTray { get; set; }

    public int TrayPerBox { get; set; }

    public int QuantityPerBox { get; set; }

    public virtual ICollection<GeneratedTray> GeneratedTrays { get; set; } = new List<GeneratedTray>();

    public virtual ICollection<WorkSession> WorkSessions { get; set; } = new List<WorkSession>();
}
