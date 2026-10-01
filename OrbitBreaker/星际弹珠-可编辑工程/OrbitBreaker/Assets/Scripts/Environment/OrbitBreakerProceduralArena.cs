using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    // Chunks are deterministic within a run, independently of the order they are visited.
    // Only a bounded neighbourhood is alive; templates remain in the inactive legacy arena.
    public sealed class OrbitBreakerProceduralArena : MonoBehaviour
    {
        public const float WorldScale = .8f, ChunkSize = 18f;
        public OrbitBreakerPlayerMotor player;
        public GameObject[] templates; // pin, board, rotor, pendulum, magnet, target, jackpot, terrace, hole
        public int seed;
        public Material[] pinMaterials;
        private readonly Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
        private readonly List<Vector2Int> retired = new List<Vector2Int>();
        private Vector2Int current = new Vector2Int(int.MinValue, int.MinValue);
        public int LiveChunkCount => chunks.Count;
        public int GeneratedCount { get; private set; }
        public Vector2Int CurrentChunk => current;
        private readonly HashSet<string> destroyedProps=new HashSet<string>();
        public void DestroyProp(GameObject prop)
        {
            if(prop==null || !prop.activeSelf || prop.transform.parent==null)return;
            destroyedProps.Add(prop.transform.parent.name+"/"+prop.name);prop.SetActive(false);
        }
        private void Awake()
        {
            if (seed == 0) seed = Random.Range(1, int.MaxValue);
            RefreshAroundPlayer();
        }
        private void Update() => RefreshAroundPlayer();
        public void RefreshAroundPlayer()
        {
            if (player == null || templates == null || templates.Length != 9) return;
            var center = new Vector2Int(Mathf.FloorToInt(player.Position.x / ChunkSize), Mathf.FloorToInt(player.Position.y / ChunkSize));
            if (center == current) return;
            current = center;
            retired.Clear();
            foreach (var pair in chunks)
                if (Mathf.Abs(pair.Key.x-center.x)>2 || Mathf.Abs(pair.Key.y-center.y)>2) retired.Add(pair.Key);
            foreach (var key in retired) { chunks[key].SetActive(false); Destroy(chunks[key]); chunks.Remove(key); }
            for (int y=-2; y<=2; y++) for (int x=-2; x<=2; x++)
            {
                var key = center + new Vector2Int(x,y);
                if (!chunks.ContainsKey(key)) Generate(key);
            }
        }
        private void Generate(Vector2Int key)
        {
            var root = new GameObject($"Sector_{key.x}_{key.y}"); root.transform.SetParent(transform, false);
            chunks.Add(key, root); GeneratedCount++;
            int hash = unchecked(seed ^ key.x*73856093 ^ key.y*19349663);
            var random = new System.Random(hash);
            int special = random.Next(9);
            bool hole = random.Next(8)==0 && key.sqrMagnitude>1;
            for (int i=0; i<9; i++)
            {
                // Six-unit spacing leaves open routes even through the moving pieces' full sweep.
                var position = new Vector3(key.x*ChunkSize+3+(i%3)*6+(float)(random.NextDouble()-.5)*1.6f,
                    key.y*ChunkSize+3+(i/3)*6+(float)(random.NextDouble()-.5)*1.6f, 0);
                int kind = random.Next(3)==0 ? 1 : 0;
                if(i==special) kind=hole?8:random.Next(2,7);
                var rotation=templates[kind].transform.rotation;
                if(kind==1)rotation=Quaternion.Euler(0,0,(float)random.NextDouble()*110-55);
                var go=Instantiate(templates[kind], position, rotation, root.transform);
                go.name=$"{templates[kind].name}_{i}";
                go.transform.localScale=templates[kind].transform.localScale*WorldScale;
                go.SetActive(!destroyedProps.Contains(root.name+"/"+go.name));
                var bumper=go.GetComponent<OrbitBreakerBumper>();
                if(bumper!=null)
                {
                    bumper.liftAssist=random.Next(2)==0;
                    var visual=go.transform.Find("Pin");
                    int variant=random.Next(3);bumper.Configure(variant);
                    if(visual!=null && pinMaterials!=null && pinMaterials.Length>0)visual.GetComponent<Renderer>().sharedMaterial=pinMaterials[variant%pinMaterials.Length];
                }
                var board=go.GetComponent<OrbitBreakerFloatingBoard>(); if(board!=null){board.travel*=WorldScale;board.phase=(float)random.NextDouble()*6.28f;}
            }
            // Recessed islands add depth without hidden collision planes.
            var terrace=Instantiate(templates[7],new Vector3(key.x*ChunkSize+6+(float)random.NextDouble()*6,key.y*ChunkSize+6+(float)random.NextDouble()*6,1.5f+(float)random.NextDouble()),templates[7].transform.rotation,root.transform);
            terrace.name="RecessedIsland";terrace.transform.localScale=templates[7].transform.localScale*WorldScale;terrace.SetActive(true);
        }
    }
}
