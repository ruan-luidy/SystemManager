// SysManager · TestPaths — one place that knows where the repository is
// Author: laurentiu021 · https://github.com/laurentiu021/SystemManager
// License: MIT

using System.IO;

namespace SysManager.Tests;

/// <summary>
/// Resolves the repository directories a source-reading test needs, by walking up from the test output.
/// </summary>
/// <remarks>
/// The build copies no <c>.cs</c> or <c>.xaml</c> into the test output, so a guard that reads source has to
/// find the tree first. That walk was written 31 times across 25 files in this project, behind 32 private
/// helpers with 17 different names, and the copies had already drifted in ways that matter:
/// <list type="bullet">
/// <item>The repository root had two definitions — <c>SUPPORT.md</c> with <c>README.md</c> in two files, the
/// <c>.github/workflows</c> folder in a third. They agree today because all three sit in the same directory,
/// which is exactly the kind of agreement that stops holding without anyone noticing.</item>
/// <item>The app project was located by six different markers: its own <c>.csproj</c> one or two levels down,
/// <c>MainWindow.xaml</c>, or the presence of its <c>Services</c>, <c>Views</c>, <c>ViewModels</c> or
/// <c>Helpers</c> folder. Four of those verify a NEIGHBOUR of the directory they return instead of the
/// directory itself, so they answer confidently for a tree that does not contain the project.</item>
/// <item>The test project's own walk fails when the suite is hosted from somewhere other than its output
/// folder, and <c>ArchitectureTests</c> grew the sibling fallback below to cover it — while
/// <c>ServicesViewModelTests</c>, holding the same walk, did not. One copy was repaired and taught the other
/// nothing, which is the whole failure mode.</item>
/// <item>Two failure idioms: throwing, or <c>Assert.NotNull(dir)</c> followed by <c>dir!</c>. The second only
/// works inside a test method, so a helper written that way cannot move anywhere useful.</item>
/// </list>
/// <para>Counting them was itself instructive: the first sweep found 28, because it keyed on
/// <c>new DirectoryInfo(</c>. Three copies wrote the same walk in shapes that needle could not see — one
/// spelled <c>new System.IO.DirectoryInfo</c> fully qualified, one walked plain strings through
/// <c>Path.GetDirectoryName</c> and never named <c>DirectoryInfo</c> at all, and one held no walk because it
/// delegated to a sibling helper. That is why the guard below looks for <c>AppContext.BaseDirectory</c>, the
/// one thing all of them had to name.</para>
/// <para>Every resolver here verifies the thing it returns rather than something beside it, and throws with
/// the starting directory in the message. <see cref="ArchitectureTests.EveryRepositoryPathATestNeeds_IsAskedForInOnePlace"/>
/// keeps the walk here — copy 32 would restore the drift in silence, since a walk that lands in the wrong
/// place still yields a confident verdict. See <c>SourceBraces</c> and <c>Symlinks</c> for the same shape.</para>
/// </remarks>
internal static class TestPaths
{
    /// <summary>The repository root — the docs and workflows the guards read are not copied to the output.</summary>
    internal static string RepoRoot() => Ancestor(
        dir => File.Exists(Path.Combine(dir, "SUPPORT.md")) && File.Exists(Path.Combine(dir, "README.md")),
        "the repository root");

    /// <summary>The solution folder, <c>&lt;repo&gt;/SysManager</c>, which holds every project.</summary>
    internal static string SolutionDir() => Directory.GetParent(AppProject())!.FullName;

    /// <summary>The app project directory, <c>&lt;repo&gt;/SysManager/SysManager</c>.</summary>
    /// <remarks>
    /// Keyed on the project file rather than on one of its folders, so what it returns is the project it
    /// claims to have found.
    /// </remarks>
    internal static string AppProject() => Path.Combine(
        Ancestor(dir => File.Exists(Path.Combine(dir, "SysManager", "SysManager.csproj")),
                 "the SysManager app project"),
        "SysManager");

    /// <summary>The source directory of this test project.</summary>
    /// <remarks>
    /// Walks up from the output folder first, which is how it resolves under <c>dotnet test</c>. That walk
    /// fails whenever the tests are hosted from elsewhere in the tree, and then every guard reading test
    /// source throws instead of running. The fallback covers it: the app project is found by a walk looking
    /// for a SUBPATH, which succeeds from anywhere under the repository, and this project is its sibling.
    /// </remarks>
    internal static string TestProject()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SysManager.Tests.csproj"))) return dir.FullName;
        }

        var sibling = Path.Combine(SolutionDir(), "SysManager.Tests");
        if (File.Exists(Path.Combine(sibling, "SysManager.Tests.csproj"))) return sibling;

        throw new DirectoryNotFoundException(
            "Could not locate the SysManager.Tests source directory from " + AppContext.BaseDirectory);
    }

    /// <summary>A file inside the app project, by the path segments below it.</summary>
    /// <param name="parts">The segments below the app project, for example <c>"Views", "AboutView.xaml"</c>.</param>
    /// <exception cref="FileNotFoundException">The file is not there, so the caller would assert over nothing.</exception>
    /// <remarks>
    /// The app is organised by feature (<c>Features/&lt;Page&gt;</c>, <c>Shared</c>, <c>Shell</c>), so a file is found
    /// by its name when the segments still name the folder it had before that move — <c>"Views", "AboutView.xaml"</c>
    /// resolves to <c>Features/About/AboutView.xaml</c>. File names are unique across the project, and a name that is
    /// not would throw here rather than pick one of them.
    /// </remarks>
    internal static string AppFile(params string[] parts)
    {
        var path = AppPath(parts);
        if (!File.Exists(path)) throw new FileNotFoundException($"Not found in the app project: {path}", path);

        return path;
    }

    /// <summary>Same as <see cref="AppFile"/>, but returns the path even when the file is not there.</summary>
    /// <remarks>For the guards that ask <c>File.Exists</c> themselves, where a missing file is a valid answer.</remarks>
    internal static string AppPath(params string[] parts)
    {
        var path = Path.Combine([AppProject(), .. parts]);
        if (File.Exists(path)) return path;

        var matches = SourceFiles(Path.GetFileName(path)).ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => path,
            _ => throw new InvalidOperationException($"{Path.GetFileName(path)} is in {matches.Length} places in the app project"),
        };
    }

    /// <summary>Every view: the XAML of each page, the shared controls and the shell's popups, with their code-behind.</summary>
    /// <param name="pattern">A file pattern, for example <c>"*.xaml"</c> or <c>"*View.xaml.cs"</c>.</param>
    /// <remarks>
    /// What the <c>Views</c> folder held before the app was organised by feature. <c>App.xaml</c> and the main window
    /// were never in it, and stay out.
    /// </remarks>
    internal static IEnumerable<string> ViewFiles(string pattern = "*.xaml") =>
        SourceFiles(pattern).Where(file =>
            (file.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
            && !Path.GetFileName(file).StartsWith("App.xaml", StringComparison.OrdinalIgnoreCase)
            && !Path.GetFileName(file).StartsWith("MainWindow.xaml", StringComparison.OrdinalIgnoreCase));

    /// <summary>Every view model, plus the navigation types and shared state that lived beside them.</summary>
    /// <param name="pattern">A file pattern, for example <c>"*ViewModel.cs"</c>.</param>
    /// <remarks>What the <c>ViewModels</c> folder held before the app was organised by feature.</remarks>
    internal static IEnumerable<string> ViewModelFiles(string pattern = "*.cs") =>
        SourceFiles(pattern).Where(file =>
            Path.GetFileName(file).EndsWith("ViewModel.cs", StringComparison.Ordinal)
            || ViewModelNeighbours.Contains(Path.GetFileName(file)));

    /// <summary>Every source file of a layer folder — <c>Services</c>, <c>Models</c> or <c>Helpers</c> — shared or a page's own.</summary>
    /// <param name="layer">The folder name: <c>Shared/Services</c> and <c>Features/&lt;Page&gt;/Services</c> are both "Services".</param>
    /// <param name="pattern">A file pattern, for example <c>"*.cs"</c>.</param>
    internal static IEnumerable<string> LayerFiles(string layer, string pattern = "*.cs") =>
        SourceFiles(pattern).Where(file => Path.GetFileName(Path.GetDirectoryName(file)) == layer);

    /// <summary>Whether <paramref name="path"/> is a source file of a layer folder (see <see cref="LayerFiles"/>).</summary>
    internal static bool IsLayerFile(string path, string layer) =>
        Path.GetFileName(Path.GetDirectoryName(path)) == layer;

    /// <summary>Whether <paramref name="path"/> is one of the files <see cref="ViewModelFiles"/> returns.</summary>
    internal static bool IsViewModelFile(string path) =>
        Path.GetFileName(path).EndsWith("ViewModel.cs", StringComparison.Ordinal)
        || ViewModelNeighbours.Contains(Path.GetFileName(path));

    private static readonly HashSet<string> ViewModelNeighbours =
        ["NavGroup.cs", "NavItem.cs", "ViewModelBase.cs", "NetworkSharedState.cs"];

    /// <summary>The app's source files matching <paramref name="pattern"/>, wherever they are, without build output.</summary>
    internal static IEnumerable<string> SourceFiles(string pattern)
    {
        var app = AppProject();
        var bin = Path.Combine(app, "bin") + Path.DirectorySeparatorChar;
        var obj = Path.Combine(app, "obj") + Path.DirectorySeparatorChar;
        return Directory.EnumerateFiles(app, pattern, SearchOption.AllDirectories)
            .Where(file => !file.StartsWith(bin, StringComparison.OrdinalIgnoreCase) && !file.StartsWith(obj, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>A directory inside the app project, by the path segments below it.</summary>
    /// <param name="parts">The segments below the app project, for example <c>"ViewModels"</c>.</param>
    /// <exception cref="DirectoryNotFoundException">The directory is not there, so an enumeration of it would be empty.</exception>
    internal static string AppDir(params string[] parts)
    {
        var path = Path.Combine([AppProject(), .. parts]);
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"Not found in the app project: {path}");

        return path;
    }

    /// <summary>The nearest ancestor of the test output that <paramref name="found"/> accepts.</summary>
    /// <param name="found">Whether a candidate directory is the one being looked for.</param>
    /// <param name="what">What is being looked for, for the exception message.</param>
    private static string Ancestor(Func<string, bool> found, string what)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (found(dir.FullName)) return dir.FullName;
        }

        throw new DirectoryNotFoundException($"Could not locate {what} from " + AppContext.BaseDirectory);
    }
}
