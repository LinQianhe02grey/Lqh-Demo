using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    // One explicit migration of project-owned objects; later builds preserve tuning/custom objects.
    public static class OrbitBreakerCosmicSceneBuilder
    {
        public static void Apply(GameObject root, GameObject arena, Camera camera, OrbitBreakerCombatConfig config)
        {
            if (arena.transform.Find("CosmicTable") != null) return;
            var group = new GameObject("CosmicTable").transform; group.SetParent(arena.transform, false);
            var dark = Mat("OrbitBreakerCosmicMetal", new Color(.025f,.035f,.085f), false);
            var cyan = Mat("OrbitBreakerCosmicCyan", new Color(.08f,.78f,1f), true);
            var purple = Mat("OrbitBreakerCosmicPurple", new Color(.6f,.2f,1f), true);
            var gold = Mat("OrbitBreakerCosmicGold", new Color(1f,.58f,.13f), true);
            var pink = Mat("OrbitBreakerCosmicPink", new Color(1f,.12f,.38f), true);
            var space = ShaderMat("OrbitBreakerCosmicNebula", "Pinball/OrbitBreakerCosmicField");
            var planet = ShaderMat("OrbitBreakerPlayerPlanet", "Pinball/OrbitBreakerPlanet");
            Set(arena.transform.Find("Backdrop"), new Vector3(0,0,1.1f), new Vector3(23,21,1.2f), space);
            var presentation = root.transform.Find("TablePresentation");
            Set(presentation.Find("Cabinet"),new Vector3(0,-.5f,2),new Vector3(24,22,1.2f),dark);
            Set(presentation.Find("LeftTrim"),new Vector3(-11.65f,0,.1f),new Vector3(.3f,21.5f,1.4f),cyan);
            Set(presentation.Find("RightTrim"),new Vector3(11.65f,0,.1f),new Vector3(.3f,21.5f,1.4f),purple);
            Set(presentation.Find("FarTrim"),new Vector3(0,10.7f,.1f),new Vector3(23.6f,.3f,1.4f),purple);
            Set(presentation.Find("FrontApron"),new Vector3(0,-11.2f,1),new Vector3(24,1.5f,1.8f),dark);
            foreach (string n in new[]{"LeftInlay","RightInlay"}) presentation.Find(n).gameObject.SetActive(false);
            var walls=arena.transform.Find("Walls");
            Set(walls.Find("LeftWall"),new Vector3(-11.25f,0,0),new Vector3(.5f,21,2),dark);
            Set(walls.Find("RightWall"),new Vector3(11.25f,0,0),new Vector3(.5f,21,2),dark);
            Set(walls.Find("TopWall"),new Vector3(0,10.25f,0),new Vector3(22,.5f,2),dark);
            Set(walls.Find("LeftGuard"),new Vector3(-6.5f,-10.25f,0),new Vector3(9,.5f,2),dark);
            Set(walls.Find("RightGuard"),new Vector3(6.5f,-10.25f,0),new Vector3(9,.5f,2),dark);
            walls.Find("LeftGuard").localRotation=Quaternion.Euler(0,0,-12);
            walls.Find("RightGuard").localRotation=Quaternion.Euler(0,0,12);
            arena.transform.Find("OrbitBreakerKillZone").localScale=new Vector3(24,2.5f,2);
            root.transform.Find("PlayerSpawn").position=new Vector3(-2.8f,-4.8f,0);
            root.transform.Find("Player").position=root.transform.Find("PlayerSpawn").position;
            root.transform.Find("Player").GetComponent<Renderer>().sharedMaterial=planet;
            root.transform.Find("Player").gameObject.AddComponent<OrbitBreakerPlanetCombatVisual>();
            camera.transform.position=new Vector3(0,-22,-24);
            camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-2f,0)-camera.transform.position,Vector3.up);
            camera.fieldOfView=46f; camera.farClipPlane=150;
            camera.backgroundColor=new Color(.003f,.005f,.025f);
            var sky=Primitive(group,"DeepSpace",PrimitiveType.Quad,new Vector3(0,0,7),new Vector3(100,100,1),space,false);
            sky.transform.localRotation=Quaternion.identity;
            for(int side=-1;side<=1;side+=2)
            {
                var guide=Primitive(group,side<0?"LeftReturnRail":"RightReturnRail",PrimitiveType.Cube,
                    new Vector3(side*7.1f,-6.8f,0),new Vector3(7.8f,.34f,1.2f),side<0?cyan:purple,true);
                guide.transform.localRotation=Quaternion.Euler(0,0,side*27);
                for(int i=0;i<7;i++) Primitive(group,"LaneLight"+side+"_"+i,PrimitiveType.Cube,
                    new Vector3(side*10.5f,-4.2f+i*1.8f,.38f),new Vector3(.15f,.5f,.08f),cyan,false);
            }
            var devices=arena.transform.Find("Devices");
            arena.transform.Find("ScoreTargets/HighTarget").position=new Vector3(0,8.7f,0);
            devices.Find("LeftBumper").position=new Vector3(-3.7f,2.2f,0);
            devices.Find("RightBumper").position=new Vector3(3.7f,2.2f,0);
            foreach(var bumper in devices.GetComponentsInChildren<OrbitBreakerBumper>())
            {
                bumper.GetComponentInChildren<Renderer>().sharedMaterial=purple;
                Ring(group,bumper.name+"Halo",bumper.transform.position+Vector3.back*.6f,1f,cyan,.055f);
            }
            foreach(var flipper in devices.GetComponentsInChildren<OrbitBreakerFlipperController>())
            {
                flipper.transform.Find("PaddlePivot/Paddle").GetComponent<Renderer>().sharedMaterial=cyan;
                flipper.transform.Find("EffectiveArea").GetComponent<Renderer>().sharedMaterial=dark;
                Label(group,flipper.Side==FlipperSide.Left?"J":"K",flipper.transform.position+new Vector3(0,-.8f,-.35f),.45f,Color.cyan);
            }
            MakeBumper(group,"IonBumperLeft",new Vector3(-7,-2.2f,0),cyan);
            MakeBumper(group,"IonBumperRight",new Vector3(7,-2.2f,0),purple);
            MakeMachine(group,"RotatingDeflector",new Vector3(-6.6f,4.8f,0),false,cyan,gold);
            MakeMachine(group,"OrbitalPendulum",new Vector3(6.6f,4.8f,0),true,purple,gold);
            var points=arena.transform.Find("AttachmentPoints");
            int a=0; foreach(Transform point in points)
            {
                point.position=new Vector3(a++==0?-4:4,-3.7f,0);
                Label(group,"ORBIT / E",point.position+new Vector3(0,-2,.3f),.19f,new Color(.3f,1,.75f));
            }
            var hazards=arena.transform.Find("Hazards");
            hazards.Find("LeftHazard").position=new Vector3(-9,.7f,0);
            hazards.Find("RightHazard").position=new Vector3(9,.7f,0);
            var spawn=arena.transform.Find("EnemySpawnPoints");
            int si=0; foreach(Transform point in spawn)
            { if(si<6) point.position=new Vector3(si%2==0?-9.5f:9.5f,-2.8f+(si/2)*5.2f,0); else point.position=new Vector3(si==6?-3:3,9,0); si++; }
            // Central jackpot well: sensor captures, the motor holds and ejects after .65 s.
            var hole=new GameObject("OrbitBreakerJackpotHole"); hole.transform.SetParent(group,false); hole.transform.localPosition=new Vector3(0,5.3f,0);
            hole.AddComponent<SphereCollider>().radius=.7f; hole.GetComponent<SphereCollider>().isTrigger=true; hole.AddComponent<OrbitBreakerJackpotHole>();
            Primitive(hole.transform,"Well",PrimitiveType.Cylinder,new Vector3(0,0,.38f),new Vector3(1.8f,.08f,1.8f),dark,false).transform.localRotation=Quaternion.Euler(90,0,0);
            Ring(hole.transform,"GoldenRim",Vector3.back*.25f,1.05f,gold,.14f);
            var swirl=Ring(hole.transform,"JackpotOrbit",Vector3.back*.3f,1.35f,gold,.04f,1.2f);
            swirl.gameObject.AddComponent<OrbitBreakerCosmicVisual>().speed=-35;
            Label(group,"JACKPOT",new Vector3(0,7.05f,.2f),.36f,new Color(1,.72f,.25f));
            Label(group,"300 x COMBO",new Vector3(0,3.8f,.2f),.2f,new Color(1,.72f,.25f));
            // Black hole is aligned with the unconditionally lethal drain, below both flippers.
            var black=ShaderMat("OrbitBreakerSingularityCore","Unlit/Color");black.color=Color.black;
            Primitive(group,"BlackHoleCore",PrimitiveType.Sphere,new Vector3(0,-10.6f,-.3f),new Vector3(4.1f,3f,.1f),black,false);
            for(int i=0;i<5;i++)
            {
                var ring=Ring(group,"AccretionDisk"+i,new Vector3(0,-10.6f,-.42f-i*.035f),1.55f+i*.27f,i%2==0?gold:pink,.035f+i*.014f,.6f);
                ring.gameObject.AddComponent<OrbitBreakerCosmicVisual>().speed=(i%2==0?1:-1)*(18+i*9);
            }
            Label(group,"EVENT HORIZON",new Vector3(0,-12,.1f),.22f,new Color(1,.4f,.25f));
            Label(group,"COSMIC  PINBALL",new Vector3(0,10,.2f),.36f,new Color(.65f,.85f,1));
            config.targetScore=3000; EditorUtility.SetDirty(config);
            RenderSettings.ambientLight=new Color(.38f,.43f,.6f);
            AssetDatabase.SaveAssets();
        }
        private static void MakeBumper(Transform parent,string name,Vector3 pos,Material mat)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=pos;
            var c=go.AddComponent<CapsuleCollider>();c.direction=2;c.radius=.75f;c.height=1.8f;
            var visual=Primitive(go.transform,"Visual",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.5f,.5f,1.5f),mat,false);
            visual.transform.localRotation=Quaternion.Euler(90,0,0);
            var so=new SerializedObject(go.AddComponent<OrbitBreakerBumper>());so.FindProperty("visual").objectReferenceValue=visual.transform;so.ApplyModifiedPropertiesWithoutUndo();
            Ring(go.transform,"OuterRing",Vector3.back*.6f,.9f,mat,.065f);
        }
        private static void MakeMachine(Transform parent,string name,Vector3 pos,bool pendulum,Material mat,Material gold)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=pos;
            var body=go.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
            var mechanism=go.AddComponent<OrbitBreakerCosmicMechanism>();mechanism.pendulum=pendulum;
            Primitive(go.transform,"Arm",PrimitiveType.Cube,new Vector3(0,pendulum?-1:0,0),new Vector3(.28f,pendulum?2.4f:3.4f,.65f),mat,true);
            var hub=Primitive(go.transform,"Pivot",PrimitiveType.Cylinder,Vector3.back*.3f,new Vector3(.7f,.45f,.7f),gold,false);hub.transform.localRotation=Quaternion.Euler(90,0,0);
            if(pendulum) Primitive(go.transform,"PendulumHead",PrimitiveType.Sphere,new Vector3(0,-2.3f,0),new Vector3(1.3f,1.3f,1.3f),gold,true);
            Ring(parent,name+"Track",pos+Vector3.forward*.4f,2.65f,mat,.025f);
            Label(parent,pendulum?"PENDULUM":"SPINNER",pos+new Vector3(0,3.1f,.2f),.2f,Color.cyan);
        }
        private static void Set(Transform t,Vector3 pos,Vector3 scale,Material mat)
        {t.localPosition=pos;t.localScale=scale;if(mat!=null)t.GetComponent<Renderer>().sharedMaterial=mat;}
        private static GameObject Primitive(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat,bool solid)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);Set(go.transform,pos,scale,mat);
            if(!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            else go.GetComponent<Collider>().sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/OrbitBreakerArenaSurface.physicMaterial");
            return go;
        }
        private static Material Mat(string name,Color color,bool glow)
        {
            var mat=ShaderMat(name,"Standard");mat.color=color;mat.SetFloat("_Glossiness",.65f);mat.SetFloat("_Metallic",glow?.35f:.6f);
            if(glow){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",color*.65f);}EditorUtility.SetDirty(mat);return mat;
        }
        private static Material ShaderMat(string name,string shader)
        {
            string path="Assets/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find(shader)){name=name};AssetDatabase.CreateAsset(mat,path);}return mat;
        }
        private static Transform Ring(Transform parent,string name,Vector3 pos,float radius,Material mat,float width,float gap=0)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=pos;
            var luminous=ShaderMat(mat.name+"Line","Unlit/Color");luminous.color=mat.color;EditorUtility.SetDirty(luminous);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=gap==0;line.positionCount=96;line.sharedMaterial=luminous;
            line.startWidth=line.endWidth=width;line.alignment=LineAlignment.TransformZ;
            for(int i=0;i<96;i++){float a=i/95f*(Mathf.PI*2-gap);line.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius);}return go.transform;
        }
        private static void Label(Transform parent,string text,Vector3 pos,float size,Color color)
        {
            var go=new GameObject(text);go.transform.SetParent(parent,false);go.transform.localPosition=pos;
            var tm=go.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=size*.35f;tm.anchor=TextAnchor.MiddleCenter;tm.color=color;
        }
    }
}
