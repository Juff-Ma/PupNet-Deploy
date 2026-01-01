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

// MsiBuilder created by Julian Rossbach.

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for MSI package.
/// Leverages SimpleMSI.
/// </summary>
public class MsiBuilder : PackageBuilder
{
    private const string PromptBat = "CommandPrompt.bat";

    /// <summary>
    /// Constructor.
    /// </summary>
    public MsiBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Msi)
    {
        throw new NotImplementedException();
    }

    public override string PackageArch => throw new NotImplementedException();

    public override string OutputName => throw new NotImplementedException();

    public override string BuildAppBin => throw new NotImplementedException();

    public override string InstallBin => throw new NotImplementedException();

    public override string? ManifestContent => throw new NotImplementedException();

    public override string? ManifestBuildPath => throw new NotImplementedException();

    public override IReadOnlyCollection<string> PackageCommands => throw new NotImplementedException();

    public override bool SupportsStartCommand => throw new NotImplementedException();

    public override bool SupportsPostRun => throw new NotImplementedException();
}