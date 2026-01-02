// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-26
// LICENSE   : AGPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/PupNet
//
// PupNet is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// PupNet is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along
// with PupNet. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

// MsiBuilder created by Julian Rossbach (httpS://github.com/Juff-Ma).

using System.Text;

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for MSI package.
/// Leverages SimpleMSI.
/// </summary>
public class MsiBuilder : PackageBuilder
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public MsiBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Msi)
    {
        BuildAppBin = Path.Combine(BuildRoot, "Publish");

        // SimpleMSI automatically installs to Program Files/LocalAppData
        // also user can define during installation
        InstallBin = "";

        ManifestBuildPath = Path.Combine(Root, Configuration.AppBaseName + ".msi.toml");
        ManifestContent = GetMsiConfig();

        string buildCommand = $"simplemsi build -c \"{ManifestBuildPath}\" -o \"{OutputPath}\"";

        if (Configuration.MsiCodeSignCertPassword is not null)
        {
            buildCommand +=
                $" --certificate-password \"{Configuration.MsiCodeSignCertPassword}\"";
        }

        PackageCommands =
            [
                buildCommand
            ];

    }

    public override string PackageArch
    {
        get
        {
            if (Arguments.Arch != null)
            {
                return Arguments.Arch;
            }

            // This works for everything besides Arm32, which current .NET does not support anyway
            return Runtime.BuildArch.ToString().ToLowerInvariant(); 
        }
    }

    public override string OutputName => GetOutputName(Configuration.MsiVersionOutput, Configuration.MsiSuffixOutput,
                                                        PackageArch, ".msi");

    public override string BuildAppBin { get; }

    public override string InstallBin { get; }

    public override string? ManifestContent { get; }

    public override string? ManifestBuildPath { get; }

    public override IReadOnlyCollection<string> PackageCommands { get; }

    public override bool SupportsStartCommand => true;

    public override bool SupportsPostRun => false;

    /// <summary>
    /// Path to RTF license file in build directory, or null if no license file specified.
    /// </summary>
    private string? RtfLicensePath => 
        Configuration.AppLicenseFile is null
        ? null
        : Path.Combine(BuildAppBin, Path.GetFileNameWithoutExtension(Configuration.AppLicenseFile));

    public override void Create(string? desktop, string? metainfo)
    {
        base.Create(desktop, metainfo);

        if (Configuration.StartCommand is not null &&
            !Configuration.StartCommand.Equals(AppExecName, StringComparison.InvariantCultureIgnoreCase))
        {
            var path = Path.Combine(BuildAppBin, Configuration.StartCommand + ".bat");
            var script = $"start {InstallExec} %*";
            Operations.WriteFile(path, script);
        }

        if (RtfLicensePath is {} licensePath)
        {
            var content = Operations.ReadFile(Configuration.AppLicenseFile)!;
            var rtf = GetRtfFromTxt(content);

            Operations.WriteFile(licensePath, rtf);
        }
    }

    /// <summary>
    /// This converts plain text to RTF format.
    /// This is a very basic conversion using only the most basic RTF syntax.
    /// </summary>
    private static string GetRtfFromTxt(string text)
    {
        text = text.Replace("\\", "\\\\")
                     .Replace("{", "\\{")
                     .Replace("}", "\\}")
                     .Replace("\r", "")
                     .Replace("\n", "\\line ");

        return $$"""
                {\rtf\ansi
                {{text}}
                }
                """;
    }

    private string GetMsiConfig()
    {
        StringBuilder sb = new();

        sb.AppendLine($"[general]");
        sb.AppendLine($"guid = \"{GetGuid()}\"");
        sb.AppendLine($"name = \"{Configuration.AppBaseName}\"");
        sb.AppendLine($"platform = \"{PackageArch}\"");
        // we use PackageRelease here since the fourth part of the version is the microsoft recommended way to include revision data
        // since we include the fourth part, we set allow_same_version_upgrades = true so packages can upgrade on revision
        sb.AppendLine($"version = \"{AppVersion}.{PackageRelease}\"");
        sb.AppendLine("allow_same_version_upgrades = true");
        sb.AppendLine($"install_scope = \"{(Configuration.MsiMachineInstall ? "machine" : "user")}\"");
        sb.AppendLine("ui_mode = \"full\""); // use full ui including license page and installation folder selection

        sb.AppendLine("[meta]");
        sb.AppendLine($"display_name = \"{Configuration.AppFriendlyName}\"");
        sb.AppendLine($"description = \"{Configuration.AppShortSummary}\""); // Even though this is called description, the MSI expects a short text
        sb.AppendLine($"author = \"{Configuration.PublisherName}\"");

        if (RtfLicensePath is not null)
        {
            sb.AppendLine($"license_file = \"{RtfLicensePath}\"");
        }

        if (PrimaryIcon is not null)
        {
            sb.AppendLine($"product_icon = \"{PrimaryIcon}\"");
        }

        if (Configuration.PublisherLinkUrl is not null)
        {
            sb.AppendLine($"about_url = \"{Configuration.PublisherLinkUrl}\"");
        }

        sb.AppendLine($"hide_program_entry = {Configuration.MsiHideProgramEntry.ToString().ToLowerInvariant()}");

        sb.AppendLine("[install]");
        sb.AppendLine($"source_dirs = [\"{BuildAppBin}\\*.*\"]");

        if (Configuration.MsiCodeSignCertName is not null)
        {
            sb.AppendLine("[install.signing]");
            sb.AppendLine($"cert_name = \"{Configuration.MsiCodeSignCertName}\"");
            
            if (Configuration.MsiCodeSignDescription is not null)
                sb.AppendLine($"description = \"{Configuration.MsiCodeSignDescription}\"");
            if (Configuration.MsiCodeSignTimestampUrl is not null)
                sb.AppendLine($"time_url = \"{Configuration.MsiCodeSignTimestampUrl}\"");
            if (Configuration.MsiCodeSignStore is not null)
                sb.AppendLine($"store_type = \"{Configuration.MsiCodeSignStore}\"");
            if (Configuration.MsiCodeSignAlgorithm is not null)
                sb.AppendLine($"algorithm = \"{Configuration.MsiCodeSignAlgorithm}\"");

            sb.AppendLine($"sign_embedded = \"{Configuration.MsiCodeSignEmbedded.ToString().ToLowerInvariant()}\"");

            if (Configuration.MsiSignToolLocation is not null)
                sb.AppendLine($"signtool_location = \"{Configuration.MsiSignToolLocation}\"");
            if (Configuration.MsiSignToolExtraArguments is not null)
                sb.AppendLine($"extra_arguments = \"{Configuration.MsiSignToolExtraArguments}\"");
        }

        if (Configuration.StartCommand is not null)
        {
            sb.AppendLine("[[install.env_vars]]");
            sb.AppendLine("name = \"PATH\"");
            sb.AppendLine("value = \"@\""); // @ is the installation directory
            sb.AppendLine("part = \"suffix\"");
        }

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine("[[install.shortcuts]]"); 
            sb.AppendLine($"target = \"{AppExecName}\"");
            sb.AppendLine($"name = \"{Configuration.AppFriendlyName}\"");
        }

        return sb.ToString().Replace("\\", "\\\\"); // TOML expects backslashes to be escaped
    }

    private string GetGuid()
    {
        // Just a constant namespace for generating the MSI UUID so that it's stable across builds
        // If this is ever changed and the user doesn't provide their own UUID, it will result in a different
        // product code and thus a different installation.
        // This is also the UUID used by PupNet in all demos/examples for itself.
        const string NAMESPACE = "2754bd46-1ef3-467b-b72a-aaa778a62bbb";

        return Configuration.MsiUuid ?? 
               GenerateV5Uuid(Guid.Parse(NAMESPACE),
                       $"{Configuration.PublisherId}_{Configuration.AppId}")
                   .ToString();
    }

    /// <summary>
    /// Generates a Version 5 UUID based on namespace and name.
    /// This is used to generate a UUID for the MSI package if the user has not specified one.
    /// </summary>
    /// <param name="namespace">UUID which sets the scope of the generated v5 UUID.</param>
    /// <param name="name">The name to be hashed and incorporated unto the generated UUID.</param>
    /// <remarks>
    /// Code courtesy of Elephant.Uuidv5 (https://github.com/S-Elephant/Elephant.NuGets/blob/master/Elephant.Uuidv5/Uuidv5Utils.cs)
    /// Licensed under MIT, Copyright (c) 2022 SquirtingElephant
    /// </remarks>
    private Guid GenerateV5Uuid(Guid @namespace, string name)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(name);
        byte[] namespaceBytes = @namespace.ToByteArray();

        SwapByteOrder(namespaceBytes);

        using var sha1 = System.Security.Cryptography.SHA1.Create();

        sha1.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
        sha1.TransformFinalBlock(nameBytes, 0, nameBytes.Length);

        var result = new byte[16];

        Array.Copy(sha1.Hash!, 0, result,0 , 16);

        result[6] = (byte)((result[6] & 0x0F) | (5 << 4));
        result[8] = (byte)((result[8] & 0x3F) | 0x80);

        SwapByteOrder(result);

        return new(result);
    }

    /// <summary>
    /// Swaps the byte order of a GUID to conform to RFC 4122.
    /// </summary>
    /// <param name="guidBytes">Byte array representing the GUID to have its byte order swapped.</param>
    /// /// <remarks>
    /// Code courtesy of Elephant.Uuidv5 (https://github.com/S-Elephant/Elephant.NuGets/blob/master/Elephant.Uuidv5/Uuidv5Utils.cs)
    /// Licensed under MIT, Copyright (c) 2022 SquirtingElephant
    /// </remarks>
    private static void SwapByteOrder(byte[] guidBytes)
    {
        // Reverse the first 4 bytes.
        Array.Reverse(guidBytes, 0, 4);

        // Reverse the 5th and 6th bytes.
        Array.Reverse(guidBytes, 4, 2);

        // Reverse the 7th and 8th bytes.
        Array.Reverse(guidBytes, 6, 2);
    }
}