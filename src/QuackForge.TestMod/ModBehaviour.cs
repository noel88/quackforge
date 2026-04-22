using System;
using UnityEngine;

// 네임스페이스는 info.ini 의 name 값과 정확히 일치해야 함.
// ModManager.ActivateMod 가 GetType("<name>.ModBehaviour") 로 조회.
namespace quackforge_test
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string Tag = "[QuackForge.TestMod]";

        protected override void OnAfterSetup()
        {
            Debug.Log($"{Tag} OnAfterSetup — hello from official Duckov mod path.");
            Debug.Log($"{Tag} ModInfo: name={info.name} version={info.version} isSteamItem={info.isSteamItem}");
            Debug.Log($"{Tag} Unity.Application.dataPath = {Application.dataPath}");
            Debug.Log($"{Tag} Mod folder = {info.path}");

            // BepInEx 로 로드된 QuackForge.Core 와 공존 가능한지 체크.
            // (같은 Unity 프로세스이므로 AppDomain 공유, AssemblyLoadContext 도 공유)
            var appDomain = AppDomain.CurrentDomain;
            var qfCore = appDomain.GetAssemblies();
            int qfCount = 0;
            foreach (var asm in qfCore)
            {
                var name = asm.GetName().Name;
                if (name != null && name.StartsWith("QuackForge"))
                {
                    Debug.Log($"{Tag} sibling assembly: {name}");
                    qfCount++;
                }
            }
            Debug.Log($"{Tag} QuackForge.* assemblies in AppDomain: {qfCount} (>=1 → BepInEx 플러그인과 공존)");
        }

        protected override void OnBeforeDeactivate()
        {
            Debug.Log($"{Tag} OnBeforeDeactivate — cleanup.");
        }
    }
}
