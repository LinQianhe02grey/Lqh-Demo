using System.Linq;
using UnityEditor;
using UnityEngine;
namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerEndlessArenaLayout
    {
        public static void Apply(GameObject root)
        {
            if(root.transform.Find("OrbitBreakerProceduralArena")!=null)return;
            var arena=root.transform.Find("Arena");
            var dense=arena.Find("OrbitBreakerDenseRallyLayout");var cosmic=arena.Find("CosmicTable");
            var player=root.transform.Find("Player").GetComponent<OrbitBreakerPlayerMotor>();
            player.transform.position=Vector3.zero;player.transform.localScale*=OrbitBreakerProceduralArena.WorldScale;
            root.transform.Find("PlayerSpawn").position=Vector3.zero;
            var field=new GameObject("OrbitBreakerProceduralArena");field.transform.SetParent(root.transform,false);
            var generator=field.AddComponent<OrbitBreakerProceduralArena>();generator.player=player;
            generator.pinMaterials=new[]{"OrbitBreakerCosmicCyan","OrbitBreakerCosmicPurple","OrbitBreakerCosmicGold"}.Select(n=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/"+n+".mat")).ToArray();
            // Retain the prior owned layout as inactive source templates, with no live boundaries.
            arena.gameObject.SetActive(false);root.transform.Find("TablePresentation").gameObject.SetActive(false);
            var hole=new GameObject("OrbitBreakerDriftingBlackHole");hole.transform.SetParent(arena,false);
            hole.AddComponent<SphereCollider>().isTrigger=true;hole.GetComponent<SphereCollider>().radius=1.6f;
            hole.AddComponent<OrbitBreakerDriftingBlackHole>();
            var core=Object.Instantiate(cosmic.Find("BlackHoleCore").gameObject,hole.transform).transform;
            core.name="Core";core.localPosition=Vector3.zero;core.localScale=new Vector3(2,2,.03f);
            for(int i=0;i<3;i++)
            {
                var ring=Object.Instantiate(cosmic.Find("AccretionDisk"+i).gameObject,hole.transform).transform;
                ring.localPosition=Vector3.back*(.04f+i*.02f);ring.localScale=Vector3.one*.82f;
            }
            generator.templates=new[]{dense.Find("RelayPin04").gameObject,dense.Find("FloatingBoard0").gameObject,
                cosmic.Find("RotatingDeflector").gameObject,cosmic.Find("OrbitalPendulum").gameObject,
                arena.GetComponentInChildren<OrbitBreakerAttachmentPoint>(true).gameObject,arena.GetComponentInChildren<OrbitBreakerScoreTarget>(true).gameObject,
                cosmic.Find("OrbitBreakerJackpotHole").gameObject,dense.Find("OrbitalTerrace").gameObject,hole};
            var background=Object.Instantiate(cosmic.Find("DeepSpace").gameObject,root.transform).transform;
            background.name="EndlessSpace";background.localScale=Vector3.one*200;
            var camera=root.transform.Find("Main Camera").GetComponent<Camera>();
            var old=camera.GetComponent<OrbitBreakerBoardCameraFraming>();if(old!=null)old.enabled=false;
            camera.fieldOfView=38;camera.farClipPlane=250;
            var follow=camera.gameObject.AddComponent<OrbitBreakerCenteredCamera>();follow.target=player.transform;follow.background=background;follow.Follow();
            var live=new GameObject("LiveEnemies");live.transform.SetParent(root.transform,false);
            var spawn=new SerializedObject(root.GetComponentInChildren<OrbitBreakerEnemySpawner>());
            spawn.FindProperty("liveRoot").objectReferenceValue=live.transform;
            spawn.FindProperty("roaming").boolValue=true;spawn.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
