using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Box
{
    public int BoxId { get; set; }

    public string? ProductCode { get; set; }

    public int? QuantityPerTray { get; set; }

    public int? TrayPerBox { get; set; }

    public int? QuantityPerBox { get; set; }

    public string? Qrcontent { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PackingRecord> PackingRecords { get; set; } = new List<PackingRecord>();
}
