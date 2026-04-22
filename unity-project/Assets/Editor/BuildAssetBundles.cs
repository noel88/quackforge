using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QuackForge.Editor
{
    // AssetBundle 빌드 엔트리포인트.
    //
    // 사용:
    //   Unity Editor 메뉴 > QuackForge > Build AssetBundles (Mac)
    //   Unity Editor 메뉴 > QuackForge > Build AssetBundles (Windows)
    //   Unity Editor 메뉴 > QuackForge > Build AssetBundles (All)
    //
    // 또는 batchmode CLI:
    //   scripts/build-mod.sh
    //
    // 산출물 경로:
    //   {repo}/mod/quackforge/mac/
    //   {repo}/mod/quackforge/windows/
    //
    // AssetBundle 태그 지정:
    //   Project 창에서 prefab/material/texture 선택
    //   → Inspector 하단 드롭다운 AssetBundle 을 "quackforge_weapons" 등으로 지정
    //   → Build 실행 시 해당 태그 별로 .bundle 산출
    public static class BuildAssetBundles
    {
        private const string OutputRoot = "../mod/quackforge";
        private const string MacDir = "mac";
        private const string WinDir = "windows";

        [MenuItem("QuackForge/Build AssetBundles (Mac)")]
        public static void BuildMac() => Build(MacDir, BuildTarget.StandaloneOSX);

        [MenuItem("QuackForge/Build AssetBundles (Windows)")]
        public static void BuildWin() => Build(WinDir, BuildTarget.StandaloneWindows64);

        [MenuItem("QuackForge/Build AssetBundles (All)")]
        public static void BuildAll()
        {
            Build(MacDir, BuildTarget.StandaloneOSX);
            Build(WinDir, BuildTarget.StandaloneWindows64);
        }

        // batchmode 진입점 (CLI 에서 -executeMethod QuackForge.Editor.BuildAssetBundles.BuildAllCli 로 호출)
        public static void BuildAllCli()
        {
            try
            {
                BuildAll();
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QuackForge] build failed: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static void Build(string subdir, BuildTarget target)
        {
            var outPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputRoot, subdir));
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

            Debug.Log($"[QuackForge] building AssetBundles → {outPath} (target={target})");

            var manifest = BuildPipeline.BuildAssetBundles(
                outPath,
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle,
                target);

            if (manifest == null)
            {
                throw new Exception($"BuildAssetBundles returned null manifest (target={target})");
            }

            var names = manifest.GetAllAssetBundles();
            Debug.Log($"[QuackForge] built {names.Length} bundles: {string.Join(", ", names)}");

            AssetDatabase.Refresh();
        }
    }
}
