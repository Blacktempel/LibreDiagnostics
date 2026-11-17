/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

#pragma warning disable CA1416 // Platform compatibility warning

using LibreHardwareMonitor.Hardware;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Security;

namespace LibreDiagnostics.Models.Helper;

/// <summary>
/// Filters network adapters using the inactive, virtual and hidden-interface criteria from HardwareMonitor.
/// </summary>
internal static class NetworkAdapterFilter
{
    #region Fields

    /// <summary>
    /// The Windows network-adapter driver registry class.
    /// </summary>
    private const string NetworkAdapterClassPath =
        @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";

    /// <summary>
    /// The NCF_HIDDEN, NCF_FILTER, NCF_NDIS_PROTOCOL and NCF_LW_FILTER component flags.
    /// </summary>
    private const long HiddenOrFilterCharacteristicsMask = 0x8 | 0x400 | 0x4000 | 0x40000;

    /// <summary>
    /// Name and description markers used by HardwareMonitor to identify virtual adapters.
    /// </summary>
    private static readonly string[] s_virtualMarkers =
    [
        "virtual", "hyper-v", "vmware", "virtualbox", "vpn", "wireguard", "tailscale", "zerotier",
        "docker", "container", "veth", "vethernet", "wsl", "tap", "tun", "loopback",
    ];

    /// <summary>
    /// Identity markers used by HardwareMonitor to identify hidden adapters and filter interfaces.
    /// </summary>
    private static readonly string[] s_hiddenOrFilterMarkers =
    [
        "qos packet scheduler", "packet scheduler miniport", "native wifi filter driver",
        "wfp 802.3 mac layer lightweight filter", "wfp native mac layer lightweight filter",
        "microsoft wan miniport", "wan miniport (", "ms_psched", "ms_wfplwf", "ms_nativewifip", "ms_ndiswan",
    ];

    #endregion

    #region Internal

    /// <summary>
    /// Reads the identifiers of active adapters that pass the virtual and hidden-interface filters.
    /// </summary>
    /// <returns>
    /// LibreHardwareMonitor adapter identifiers, or null if network enumeration is temporarily unavailable.
    /// </returns>
    internal static HashSet<string> GetVisibleAdapterIdentifiers()
    {
        var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // Inspect the current status and identity of each system network adapter.
            foreach (var network in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (network.OperationalStatus != OperationalStatus.Up || IsVirtual(network))
                {
                    continue;
                }

                var identity = network.Name + " " + network.Description;
                if (!IsHiddenOrFilter(identity))
                {
                    identifiers.Add(new Identifier("nic", network.Id).ToString());
                }
            }
        }
        catch (NetworkInformationException)
        {
            // Retain the previous visibility while an adapter is being enabled, disabled or reconfigured.
            return null;
        }

        if (OperatingSystem.IsWindows())
        {
            RemoveHiddenWindowsAdapters(identifiers);
        }

        return identifiers;
    }

    #endregion

    #region Private

    /// <summary>
    /// Checks the interface type, name and description against HardwareMonitor's virtual-adapter criteria.
    /// </summary>
    private static bool IsVirtual(NetworkInterface network)
    {
        if (network.NetworkInterfaceType == NetworkInterfaceType.Loopback
         || network.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
        {
            return true;
        }

        var identity = network.Name + " " + network.Description;

        return s_virtualMarkers.Any(marker => identity.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks the identity markers and Windows component flags for hidden or filter interfaces.
    /// </summary>
    private static bool IsHiddenOrFilter(string identity, long characteristics = 0)
    {
        return (characteristics & HiddenOrFilterCharacteristicsMask) != 0
            || s_hiddenOrFilterMarkers.Any(marker => identity.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Removes network identifiers whose registry metadata marks them as hidden or filter interfaces.
    /// </summary>
    private static void RemoveHiddenWindowsAdapters(HashSet<string> identifiers)
    {
        try
        {
            using var classKey = Registry.LocalMachine.OpenSubKey(NetworkAdapterClassPath);

            if (classKey == null)
            {
                return;
            }

            // Match each driver registry entry to a visible network identifier and apply its filter metadata.
            foreach (var subKeyName in classKey.GetSubKeyNames())
            {
                RemoveHiddenWindowsAdapter(classKey, subKeyName, identifiers);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            // Keep the name/status filter results when the registry class cannot be read.
        }
    }

    /// <summary>
    /// Reads a driver registry entry and removes its matching identifier when filter metadata is present.
    /// </summary>
    private static void RemoveHiddenWindowsAdapter(RegistryKey classKey, string subKeyName, HashSet<string> identifiers)
    {
        try
        {
            using var adapterKey = classKey.OpenSubKey(subKeyName);
            var instanceID = adapterKey?.GetValue("NetCfgInstanceId") as string;

            if (string.IsNullOrWhiteSpace(instanceID))
            {
                return;
            }

            var identifier = new Identifier("nic", instanceID).ToString();
            if (!identifiers.Contains(identifier))
            {
                return;
            }

            var identity = string.Join(" ",
                adapterKey.GetValue("DriverDesc"),
                adapterKey.GetValue("DriverName"),
                adapterKey.GetValue("ComponentId"),
                adapterKey.GetValue("MatchingDeviceId"));

            var characteristics = Convert.ToInt64(adapterKey.GetValue("Characteristics") ?? 0);

            if (IsHiddenOrFilter(identity, characteristics))
            {
                identifiers.Remove(identifier);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            // Continue with other registry entries when this adapter's metadata cannot be read.
        }
    }

    #endregion
}

#pragma warning restore CA1416
