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
        // TODO: set ManifestContent based on configuration

        // TODO: set PackageCommands based on configuration
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
}