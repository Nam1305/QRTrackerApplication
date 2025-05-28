using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class TrayScan
{
    public int TrayScanId { get; set; }

    public string? TrayQrcode { get; set; }

    public TimeSpan? ScanTime { get; set; }

    public DateOnly? ScanDate { get; set; }

    public int GeneratedTrayId { get; set; }

    public int SessionId { get; set; }

    public virtual GeneratedTray GeneratedTray { get; set; } = null!;

    public virtual WorkSession Session { get; set; } = null!;
}
