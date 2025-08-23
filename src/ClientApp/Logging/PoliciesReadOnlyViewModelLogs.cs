// Project Name: ClientApp
// File Name: PoliciesReadOnlyViewModelLogs.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientApp.Logging;

internal static partial class PoliciesReadOnlyViewModelLogs
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Error, Message = "Policies read-only refresh failed")] 
    public static partial void RefreshFailed(ILogger logger, Exception ex);
}
