using System.Collections.Generic;
using UnityEngine;
namespace OrbitBreaker
{
    // Small explanatory illustrations are generated once and kept in the same palette as the world.
    public sealed class OrbitBreakerCatalogIllustrations
    {
        private const int W=160,H=120;
        private readonly Dictionary<int,Texture2D> cache=new Dictionary<int,Texture2D>();
        private Color[] pixels;
        private static readonly Color Cyan=new Color(.15f,.9f,1),Gold=new Color(1,.72f,.18f),Purple=new Color(.7f,.3f,1),Green=new Color(.15f,1,.65f),Red=new Color(1,.23f,.3f);
        private void Dot(float x,float y,float r,Color c)
        {
            int minX=Mathf.Max(0,(int)(x-r)),maxX=Mathf.Min(W-1,(int)(x+r));
            int minY=Mathf.Max(0,(int)(y-r)),maxY=Mathf.Min(H-1,(int)(y+r));
            for(int j=minY;j<=maxY;j++)for(int i=minX;i<=maxX;i++)if((i-x)*(i-x)+(j-y)*(j-y)<=r*r)pixels[j*W+i]=c;
        }
        private void Line(float x,float y,float xx,float yy,float width,Color c)
        {int n=Mathf.CeilToInt(Vector2.Distance(new Vector2(x,y),new Vector2(xx,yy))*1.5f);for(int i=0;i<=n;i++){float t=n==0?0:(float)i/n;Dot(Mathf.Lerp(x,xx,t),Mathf.Lerp(y,yy,t),width,c);}}
        private void Ring(float x,float y,float radius,Color c,float width=2)
        {for(int i=0;i<100;i++){float a=i*Mathf.PI*2/100;Dot(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,width,c);}}
        private void Box(int x,int y,int width,int height,Color c)
        {for(int j=Mathf.Max(0,y);j<Mathf.Min(H,y+height);j++)for(int i=Mathf.Max(0,x);i<Mathf.Min(W,x+width);i++)pixels[j*W+i]=c;}
        private void Arrow(float x,float y,float xx,float yy,Color c)
        {Line(x,y,xx,yy,2,c);var d=(new Vector2(xx-x,yy-y)).normalized;var side=new Vector2(-d.y,d.x);var back=new Vector2(xx,yy)-d*10;Line(xx,yy,back.x+side.x*6,back.y+side.y*6,2,c);Line(xx,yy,back.x-side.x*6,back.y-side.y*6,2,c);}
        private void Pillar(Color c)
        {Dot(80,40,23,c*.35f);Box(58,40,44,39,c*.7f);Dot(80,79,22,c);Line(65,48,65,76,2,Color.white);}
        public Texture2D Get(int page,int item)
        {
            int key=page*100+item;if(cache.TryGetValue(key,out var texture))return texture;
            pixels=new Color[W*H];for(int i=0;i<pixels.Length;i++)pixels[i]=new Color(.025f,.045f,.09f);
            if(page==0)
            {
                if(item==0){Arrow(25,60,132,60,Cyan);Dot(62,60,15,Gold);}
                else if(item==1){for(int i=0;i<10;i++){float a=i*Mathf.PI/5;Line(80,60,80+Mathf.Cos(a)*42,60+Mathf.Sin(a)*42,2,Gold);}Dot(80,60,19,Red);}
                else if(item==2){for(int i=0;i<3;i++){Line(50+i*30,40,50+i*30,76,8,Cyan);}}
                else if(item==3){Ring(80,60,36,Purple);Dot(116,60,9,Cyan);Arrow(86,24,105,31,Gold);}
                else {Dot(52,60,25,Cyan);Ring(52,60,33,Purple);Box(102,39,31,42,Red);}
            }
            else if(page==1)
            {
                if(item<3)Pillar(item==0?Cyan:item==1?Purple:Gold);
                else if(item==3){Ring(80,60,46,Green);Pillar(Green);Dot(125,65,8,Cyan);Arrow(118,82,104,99,Color.white);}
                else if(item==4){Line(33,43,128,78,9,Purple);Line(33,49,126,84,2,Cyan);Arrow(28,82,48,96,Cyan);}
                else if(item==5){Ring(80,60,43,Purple);Line(43,28,115,91,6,Purple);Dot(80,60,10,Gold);Dot(114,90,14,Gold);}
                else if(item==6){Ring(80,60,42,Red,4);Ring(80,60,34,Gold);Dot(80,60,28,new Color(.11f,.02f,.1f));for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Line(80,60,80+Mathf.Cos(a)*23,60+Mathf.Sin(a)*23,3,Gold);}}
                else {Ring(58,60,35,Purple,4);Dot(58,60,27,Color.black);Ring(124,65,17,Gold);Line(124,40,124,86,1,Gold);}
            }
            else
            {
                if(item==0){Box(53,37,55,49,Red);Line(53,86,109,86,3,Gold);Box(46,99,70,5,Red);}
                else if(item==1){Ring(80,60,46,Purple);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Dot(80+Mathf.Cos(a)*15,60+Mathf.Sin(a)*15,22,new Color(.09f,.06f,.15f));}Dot(71,69,4,Purple);Dot(91,69,4,Purple);}
                else if(item==2){Ring(80,60,39,Green);Line(58,60,102,60,6,Green);Line(80,38,80,82,6,Green);}
                else if(item==3){for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(80,59,80+Mathf.Cos(a)*49,59+Mathf.Sin(a)*43,5,Purple);}Dot(80,65,29,Purple);Dot(68,69,7,Gold);Dot(92,69,7,Gold);Dot(68,69,3,Color.black);Dot(92,69,3,Color.black);}
                else if(item==4){Ring(80,35,29,Red);Line(76,31,87,62,12,Purple);Line(87,62,71,89,9,Purple);Dot(71,89,9,Purple);}
                else{Line(20,24,138,95,8,Red);Line(20,24,138,95,2,Color.white);Ring(30,30,16,Gold);}
            }
            texture=new Texture2D(W,H,TextureFormat.RGBA32,false){name="Catalog_"+page+"_"+item,hideFlags=HideFlags.DontSave};
            texture.SetPixels(pixels);texture.Apply();cache.Add(key,texture);return texture;
        }
        public void Dispose(){foreach(var t in cache.Values)if(t!=null)Object.Destroy(t);cache.Clear();}
    }
}
