using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerCosmicLayoutRefinement
    {
        public static void Apply(GameObject root, GameObject arena, Camera camera)
        {
            if (arena.transform.Find("OpenOrbitLayout") != null) return;
            var group = new GameObject("OpenOrbitLayout").transform; group.SetParent(arena.transform, false);
            // Keep the existing physical boundaries, replace only their bulky presentation.
            foreach (var r in root.transform.Find("TablePresentation").GetComponentsInChildren<Renderer>(true)) r.enabled=false;
            foreach (var r in arena.transform.Find("Walls").GetComponentsInChildren<Renderer>()) r.enabled=false;
            arena.transform.Find("Backdrop").GetComponent<Renderer>().enabled=false;
            var cyan=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicCyanLine.mat");
            var purple=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicPurpleLine.mat");
            foreach (int side in new[]{-1,1})
            {
                var pts=new[]{new Vector3(side*2.2f,-11.15f,-.65f),new Vector3(side*10.6f,-9.36f,-.65f),new Vector3(side*11.1f,-8.8f,-.65f),new Vector3(side*11.1f,9.3f,-.65f),new Vector3(side*10.4f,10.1f,-.65f),new Vector3(side*.25f,10.1f,-.65f)};
                Line(group,side<0?"PortBoundary":"StarboardBoundary",pts,side<0?cyan:purple,.065f);
                var rail=arena.transform.Find("CosmicTable/"+(side<0?"LeftReturnRail":"RightReturnRail"));
                rail.GetComponent<Renderer>().enabled=false;
                Line(rail,"RailLight",new[]{new Vector3(-.5f,0,-.52f),new Vector3(.5f,0,-.52f)},side<0?cyan:purple,.07f);
            }
            camera.transform.position=new Vector3(0,-28,-20);
            camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-3,0)-camera.transform.position,Vector3.up);
            camera.fieldOfView=OrbitBreakerBoardCameraFraming.ReferenceFov;
            camera.gameObject.AddComponent<OrbitBreakerBoardCameraFraming>();
            var cosmic=arena.transform.Find("CosmicTable");
            cosmic.Find("COSMIC  PINBALL").GetComponent<Renderer>().enabled=false;
            foreach(var flipper in arena.GetComponentsInChildren<OrbitBreakerFlipperController>()) flipper.transform.Find("EffectiveArea").GetComponent<Renderer>().enabled=false;
            foreach(Transform t in cosmic)
            {
                if(t.name=="BlackHoleCore" || t.name.StartsWith("AccretionDisk")) { t.localPosition+=Vector3.up*.4f;t.localScale*=.82f; }
                if(t.name=="EVENT HORIZON") { t.localPosition=new Vector3(0,-11.9f,.1f);t.GetComponent<TextMesh>().characterSize*=.7f;t.GetComponent<Renderer>().enabled=false; }
                if(t.name=="J" || t.name=="K") t.GetComponent<TextMesh>().characterSize*=.65f;
            }
            Move(arena.transform,"Devices/LeftBumper",new Vector3(-2.8f,1.1f,0)); Move(cosmic,"LeftBumperHalo",new Vector3(-2.8f,1.1f,-.6f));
            Move(arena.transform,"Devices/RightBumper",new Vector3(5.1f,2.7f,0)); Move(cosmic,"RightBumperHalo",new Vector3(5.1f,2.7f,-.6f));
            Move(cosmic,"IonBumperLeft",new Vector3(-7.7f,-1.7f,0)); Move(cosmic,"IonBumperRight",new Vector3(7.3f,-4.1f,0));
            Shift(cosmic,"RotatingDeflector",new Vector3(-.8f,1,0)); Shift(cosmic,"RotatingDeflectorTrack",new Vector3(-.8f,1,0)); Shift(cosmic,"SPINNER",new Vector3(-.8f,1,0));
            Shift(cosmic,"OrbitalPendulum",new Vector3(.7f,-.6f,0)); Shift(cosmic,"OrbitalPendulumTrack",new Vector3(.7f,-.6f,0)); Shift(cosmic,"PENDULUM",new Vector3(.7f,-.6f,0));
            Shift(cosmic,"OrbitBreakerJackpotHole",new Vector3(-1.3f,.6f,0)); Shift(cosmic,"JACKPOT",new Vector3(-1.3f,.6f,0)); Shift(cosmic,"300 x COMBO",new Vector3(-1.3f,.6f,0));
            var points=arena.transform.Find("AttachmentPoints");
            foreach(Transform t in points) t.position=t.position.x<0?new Vector3(-4.6f,-3.9f,0):new Vector3(2.8f,-1.6f,0);
            foreach(Transform t in cosmic) if(t.name=="ORBIT / E") t.position=t.position.x<0?new Vector3(-4.6f,-5.9f,.3f):new Vector3(2.8f,-3.6f,.3f);
            Move(arena.transform,"ScoreTargets/LeftTarget",new Vector3(-4.4f,3.8f,0));
            Move(arena.transform,"ScoreTargets/RightTarget",new Vector3(3.1f,6.6f,0));
            Move(arena.transform,"ScoreTargets/HighTarget",new Vector3(-.8f,8.8f,0));
            arena.transform.Find("ScoreTargets/LeftTarget").localRotation=Quaternion.Euler(0,0,-14);
            arena.transform.Find("ScoreTargets/RightTarget").localRotation=Quaternion.Euler(0,0,17);
            Move(arena.transform,"Hazards/LeftHazard",new Vector3(-9,2.1f,0));
            Move(arena.transform,"Hazards/RightHazard",new Vector3(8.9f,-.4f,0));
            var material=new Material(Shader.Find("Pinball/OrbitBreakerEnemyPhase")){name="OrbitBreakerEnemyPhase"};
            material.SetColor("_Color",new Color(1,.12f,.32f));material.SetFloat("_Opacity",.46f);
            AssetDatabase.CreateAsset(material,"Assets/Materials/OrbitBreakerEnemyPhase.mat");
            const string path="Assets/Prefabs/OrbitBreakerEnemy.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(path);
            try { prefab.GetComponent<Renderer>().sharedMaterial=material; PrefabUtility.SaveAsPrefabAsset(prefab,path); }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            AssetDatabase.SaveAssets();
        }
        private static void Move(Transform parent,string path,Vector3 pos) { parent.Find(path).localPosition=pos; }
        private static void Shift(Transform parent,string path,Vector3 delta) { parent.Find(path).localPosition+=delta; }
        private static void Line(Transform parent,string name,Vector3[] points,Material mat,float width)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);
            line.sharedMaterial=mat;line.startWidth=line.endWidth=width;line.numCornerVertices=6;line.numCapVertices=6;
        }
    }
}
