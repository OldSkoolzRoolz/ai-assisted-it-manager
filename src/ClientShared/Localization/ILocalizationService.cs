// Project Name: ClientShared
// File Name: ILocalizationService.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Globalization;

namespace KC.ITCompanion.ClientShared.Localization;

/// <summary>
/// Abstraction for culture management and change notifications.
/// UI layers can listen for <see cref="CultureChanged"/> to trigger resource re-bind.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Current UI culture in effect.</summary>
    CultureInfo CurrentUICulture { get; }
    /// <summary>Raised after culture successfully changed.</summary>
    event EventHandler<CultureInfo>? CultureChanged;
    /// <summary>
    /// Attempts to change UI culture. Implementations should throw if culture name invalid.
    /// </summary>
    /// <param name="cultureName">IETF culture tag.</param>
    void ChangeCulture(string cultureName);
}
