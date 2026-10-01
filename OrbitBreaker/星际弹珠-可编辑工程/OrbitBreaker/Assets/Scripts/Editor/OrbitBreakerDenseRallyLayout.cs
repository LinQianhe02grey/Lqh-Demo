using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    public static class OrbitBreakerDenseRallyLayout
    {
        public static void Apply(GameObject root, GameObject arena, OrbitBreakerCombatConfig config)
        {
            if(arena.transform.Find("OrbitBreakerDenseRallyLayout")!=null)return;
            var group=new GameObject("OrbitBreakerDenseRallyLayout").transform;group.SetParent(arena.transform,false);
            var cyan=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicCyan.mat");
            var purple=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicPurple.mat");
            var gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicGold.mat");
            var line=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicCyanLine.mat");
            var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/OrbitBreakerCosmicMetal.mat");
            // Slim collision shapes leave ball-sized passages between dense clusters.
            foreach(var bumper in arena.GetComponentsInChildren<OrbitBreakerBumper>())
            {
                bumper.transform.localScale=new Vector3(.6f,.6f,.85f);
                bumper.liftAssist=bumper.transform.position.y < -3;
            }
            var cosmic=arena.transform.Find("CosmicTable");
            foreach(string n in new[]{"LeftBumperHalo","RightBumperHalo"}) cosmic.Find(n).localScale=Vector3.one*.6f;
            Vector2[] positions={new Vector2(-9,6.8f),new Vector2(-9,4),new Vector2(-4.3f,8),new Vector2(-2.6f,8.7f),
                new Vector2(1.5f,8.5f),new Vector2(5.1f,8.5f),new Vector2(8.5f,8.1f),new Vector2(-5.1f,1.2f),
                new Vector2(-.3f,2.5f),new Vector2(2.4f,3.6f),new Vector2(-8.5f,-4.5f),new Vector2(-6.8f,-6.4f),
                new Vector2(-2.6f,-6.5f),new Vector2(.5f,-3.9f),new Vector2(4.5f,-5.8f),new Vector2(8.8f,-6.5f),
                new Vector2(9.3f,1.5f),new Vector2(.1f,.3f)};
            for(int i=0;i<positions.Length;i++)
            {
                var go=new GameObject("RelayPin"+i.ToString("00"));go.transform.SetParent(group,false);go.transform.localPosition=positions[i];
                var collider=go.AddComponent<CapsuleCollider>();collider.direction=2;collider.radius=.38f;collider.height=1.5f;
                collider.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
                float height=.5f+(i%3)*.2f;
                var visual=Shape(go.transform,"Pin",PrimitiveType.Cylinder,new Vector3(0,0,-height*.3f),new Vector3(.76f,height,.76f),i%3==0?gold:i%2==0?cyan:purple);
                visual.localRotation=Quaternion.Euler(90,0,0);
                var b=go.AddComponent<OrbitBreakerBumper>();b.liftAssist=positions[i].y< -3;
                var so=new SerializedObject(b);so.FindProperty("visual").objectReferenceValue=visual;so.ApplyModifiedPropertiesWithoutUndo();
                Ring(go.transform,"ContactHalo",.48f,-height*.7f,line,.045f);
                Shape(go.transform,"Socket",PrimitiveType.Cylinder,new Vector3(0,0,.48f),new Vector3(1.05f,.10f,1.05f),dark).localRotation=Quaternion.Euler(90,0,0);
            }
            Vector2[] boards={new Vector2(-8.4f,.6f),new Vector2(5.2f,6.6f),new Vector2(-3.9f,5.6f),new Vector2(7.5f,-2.3f)};
            for(int i=0;i<boards.Length;i++)
            {
                var go=new GameObject("OrbitBreakerFloatingBoard"+i);go.transform.SetParent(group,false);go.transform.localPosition=boards[i];go.transform.localRotation=Quaternion.Euler(0,0,i%2==0?18:-22);
                var b=go.AddComponent<Rigidbody>();b.isKinematic=true;b.useGravity=false;
                var c=go.AddComponent<BoxCollider>();c.size=new Vector3(2.2f,.35f,1.1f);
                var moving=go.AddComponent<OrbitBreakerFloatingBoard>();moving.phase=i*1.2f;moving.travel=new Vector2(i%2==0?.5f:.7f,.25f);
                Shape(go.transform,"FloatingSlab",PrimitiveType.Cube,new Vector3(0,0,-.16f),new Vector3(2.2f,.35f,.6f),dark);
                Shape(go.transform,"EnergyEdge",PrimitiveType.Cube,new Vector3(0,-.2f,-.5f),new Vector3(2.2f,.065f,.06f),i%2==0?cyan:purple);
                Shape(go.transform,"Underglow",PrimitiveType.Cube,new Vector3(0,.08f,.48f),new Vector3(2.5f,.5f,.05f),i%2==0?purple:cyan);
            }
            var shuttle=new GameObject("RescueShuttle");shuttle.transform.SetParent(group,false);shuttle.transform.localPosition=new Vector3(0,-7.1f,0);
            shuttle.AddComponent<Rigidbody>().isKinematic=true;
            shuttle.AddComponent<BoxCollider>().size=new Vector3(3.4f,.3f,1.1f);
            shuttle.AddComponent<OrbitBreakerFloatingBoard>().travel=new Vector2(2.2f,0);
            Shape(shuttle.transform,"FloatingSlab",PrimitiveType.Cube,Vector3.zero,new Vector3(3.4f,.3f,.5f),dark);
            Shape(shuttle.transform,"EnergyEdge",PrimitiveType.Cube,Vector3.back*.3f,new Vector3(3.4f,.12f,.08f),gold);
            // Decorative terraces sit behind the physics plane and never block the ball.
            foreach(var spec in new[]{new Vector3(-7,5.6f,1.4f),new Vector3(5.7f,5.4f,2.1f),new Vector3(-3.9f,-1.1f,1.1f)})
            {
                var deck=Shape(group,"OrbitalTerrace",PrimitiveType.Cylinder,spec,new Vector3(5.2f,.22f,4.1f),dark);deck.localRotation=Quaternion.Euler(90,0,0);
                Ring(deck,"TerraceRim",.5f,-.52f,line,.008f);
                var rim=deck.Find("TerraceRim");rim.localPosition=Vector3.down*1.01f;rim.localRotation=Quaternion.Euler(90,0,0);
            }
            var core=cosmic.Find("BlackHoleCore");core.localPosition=new Vector3(0,-10.5f,0);core.localScale=new Vector3(2.8f,2.0f,.03f);
            for(int i=0;i<5;i++){var ring=cosmic.Find("AccretionDisk"+i);ring.position=new Vector3(0,-10.5f,-.04f-i*.02f);ring.localScale=new Vector3(.72f,.60f,1);}
            var zone=arena.transform.Find("OrbitBreakerKillZone");zone.localPosition=core.localPosition;zone.localScale=Vector3.one;
            zone.GetComponent<BoxCollider>().size=new Vector3(2.8f,2,2);
            var kz=new SerializedObject(zone.GetComponent<OrbitBreakerKillZone>());kz.FindProperty("visibleCore").objectReferenceValue=core;kz.ApplyModifiedPropertiesWithoutUndo();
            foreach(var f in arena.GetComponentsInChildren<OrbitBreakerFlipperController>())
            {
                f.transform.position=new Vector3(f.Side==FlipperSide.Left?-2.6f:2.6f,-8.5f,0);
                f.GetComponent<BoxCollider>().size=new Vector3(4.4f,2.5f,1.5f);
                f.transform.Find("PaddlePivot/Paddle").localScale=new Vector3(4,.3f,.5f);
            }
            root.transform.Find("PlayerSpawn").position=new Vector3(0,-5.8f,0);root.transform.Find("Player").position=root.transform.Find("PlayerSpawn").position;
            config.gravityStrength=7.5f;config.dashCooldown=1f;EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
        private static Transform Shape(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;return go.transform;
        }
        private static void Ring(Transform parent,string name,float radius,float depth,Material mat,float width)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=Vector3.forward*depth;
            var l=go.AddComponent<LineRenderer>();l.sharedMaterial=mat;l.useWorldSpace=false;l.loop=true;l.positionCount=64;l.startWidth=l.endWidth=width;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;l.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius);}
        }
    }
}
