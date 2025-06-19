using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class WorkSession
{
    public int SessionId { get; set; }

    public TimeSpan? ScanTime { get; set; }

    public DateOnly? ScanDate { get; set; }

    public string? BoxSequence { get; set; }

    public int ProductId { get; set; }

    public int? ExpectedTrayCount { get; set; }

    public string? IsCompleted { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<TrayScan> TrayScans { get; set; } = new List<TrayScan>();
}
