using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class GeneratedTray
{
    public int GeneratedTrayId { get; set; }

    public string QrcodeContent { get; set; } = null!;

    public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<TrayScan> TrayScans { get; set; } = new List<TrayScan>();
}
