using System.Collections.Generic;
using UnityEngine;
namespace OrbitBreaker
{
    public enum CombatSound { Bounce, Dash, Attach, Hit, Kill, Reward, Shield, Burst }
    public sealed class OrbitBreakerCombatAudio : MonoBehaviour
    {
        private static OrbitBreakerCombatAudio instance;
        private AudioSource source;
        private OrbitBreakerPlayerMotor player;
        private readonly Dictionary<CombatSound,AudioClip> clips=new Dictionary<CombatSound,AudioClip>();
        private readonly Dictionary<CombatSound,float> next=new Dictionary<CombatSound,float>();
        public int PlayedCount {get;private set;}
        public CombatSound LastSound {get;private set;}
        private void Awake()
        {
            instance=this;source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=.22f;
            player=FindObjectOfType<OrbitBreakerPlayerMotor>();
            foreach(CombatSound sound in System.Enum.GetValues(typeof(CombatSound)))
            {
                float length=sound==CombatSound.Reward?.45f:.13f;int count=Mathf.RoundToInt(length*22050);var samples=new float[count];
                float start=180+(int)sound*95;
                for(int i=0;i<count;i++){float t=(float)i/22050;float envelope=Mathf.Sin(Mathf.PI*i/count)*Mathf.Exp(-t*8);float hz=start+(sound==CombatSound.Hit?-700:900)*t;samples[i]=Mathf.Sin(2*Mathf.PI*(start*t+(hz-start)*t*.5f))*envelope*.6f;}
                var clip=AudioClip.Create("Synth"+sound,count,1,22050,false);clip.SetData(samples,0);clips.Add(sound,clip);
            }
            player.BoardImpulseApplied+=Bounce;player.DashStarted+=Dash;player.HitReceived+=Hit;player.AttachmentChanged+=Attach;
        }
        private void Bounce()=>Play(CombatSound.Bounce);
        private void Dash()=>Play(CombatSound.Dash);
        private void Hit()=>Play(CombatSound.Hit);
        private void Attach(bool attached){if(attached)Play(CombatSound.Attach);}
        public static void Play(CombatSound sound)
        {
            if(instance==null||!instance.isActiveAndEnabled)return;
            if(instance.next.TryGetValue(sound,out float t)&&Time.unscaledTime<t)return;
            instance.next[sound]=Time.unscaledTime+(sound==CombatSound.Bounce?.075f:.1f);
            instance.source.PlayOneShot(instance.clips[sound]);instance.PlayedCount++;instance.LastSound=sound;
        }
        private void OnDestroy()
        {
            if(player!=null){player.BoardImpulseApplied-=Bounce;player.DashStarted-=Dash;player.HitReceived-=Hit;player.AttachmentChanged-=Attach;}
            foreach(var c in clips.Values)Destroy(c);if(instance==this)instance=null;
        }
    }
}
