using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ErrorLog
{
    public int ErrorLogId { get; set; }

    public string ErrorKey { get; set; } = null!;

    public string ErrorMessage { get; set; } = null!;

    public DateTime ErrorDate { get; set; }

    public TimeSpan ErrorTime { get; set; }

    public int? IsResolved { get; set; }

    public int? SessionId { get; set; }

    public virtual WorkSession? Session { get; set; }
}

