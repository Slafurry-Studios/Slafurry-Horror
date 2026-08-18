using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    public static void Build()
    {
        string targetArgument = GetArgument("-buildTarget");

        if (string.IsNullOrEmpty(targetArgument))
        {
            Fail("Missing -buildTarget argument.");
            return;
        }

        if (!Enum.TryParse(targetArgument, out BuildTarget target))
        {
            Fail($"Unknown build target: {targetArgument}");
            return;
        }

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Fail(
                "No enabled scenes found in Build Settings. " +
                "Add your scenes through File > Build Settings."
            );
            return;
        }

        string productName = PlayerSettings.productName;

        if (string.IsNullOrWhiteSpace(productName))
        {
            productName = "Game";
        }

        string outputPath = GetOutputPath(target, productName);

        string outputDirectory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log($"Game               : {productName}");
        UnityEngine.Debug.Log($"Unity Version      : {Application.unityVersion}");
        UnityEngine.Debug.Log($"Build Target       : {target}");
        UnityEngine.Debug.Log($"Output             : {outputPath}");
        UnityEngine.Debug.Log($"Scenes             : {scenes.Length}");
        UnityEngine.Debug.Log("========================================");

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = target,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            Fail(
                $"Build failed for {target}. " +
                $"Result: {report.summary.result}"
            );

            return;
        }

        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log("BUILD SUCCEEDED");
        UnityEngine.Debug.Log($"Game   : {productName}");
        UnityEngine.Debug.Log($"Target : {target}");
        UnityEngine.Debug.Log(
            $"Size   : {report.summary.totalSize / (1024f * 1024f):F2} MB"
        );
        UnityEngine.Debug.Log($"Output : {outputPath}");
        UnityEngine.Debug.Log("========================================");

        EditorApplication.Exit(0);
    }

    private static string GetOutputPath(
        BuildTarget target,
        string productName
    )
    {
        string safeName = SanitizeFileName(productName);

        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                return Path.Combine(
                    "build",
                    "StandaloneWindows64",
                    safeName + ".exe"
                );

            case BuildTarget.StandaloneLinux64:
                return Path.Combine(
                    "build",
                    "StandaloneLinux64",
                    safeName + ".x86_64"
                );

            case BuildTarget.StandaloneOSX:
                return Path.Combine(
                    "build",
                    "StandaloneOSX",
                    safeName + ".app"
                );

            case BuildTarget.WebGL:
                return Path.Combine(
                    "build",
                    "WebGL"
                );

            default:
                throw new Exception(
                    $"Unsupported build target: {target}"
                );
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(
                invalidChar.ToString(),
                ""
            );
        }

        return fileName.Trim();
    }

    private static string GetArgument(string argumentName)
    {
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argumentName)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static void Fail(string message)
    {
        UnityEngine.Debug.LogError("========================================");
        UnityEngine.Debug.LogError("BUILD FAILED");
        UnityEngine.Debug.LogError(message);
        UnityEngine.Debug.LogError("========================================");

        EditorApplication.Exit(1);
    }
}
