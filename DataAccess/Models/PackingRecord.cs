using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class PackingRecord
{
    public int PackingRecordId { get; set; }

    public TimeSpan? ScanTime { get; set; }

    public DateOnly? ScanDate { get; set; }

    public int BoxId { get; set; }

    public int KanbanId { get; set; }

    public virtual Box Box { get; set; } = null!;

    public virtual Kanban Kanban { get; set; } = null!;
}
