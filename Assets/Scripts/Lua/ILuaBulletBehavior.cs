using UnityEngine;

namespace Cardwin.Lua
{
    /// <summary>
    /// Behaviour contract a Lua bullet implements. Mirrors the Lua callback shape
    /// (OnSpawn / OnUpdate / OnHit / OnRecycle). While no Lua VM is integrated, these
    /// are implemented in C# and selected by the registry's `behavior` string; once a
    /// Lua VM (xLua/tolua) is added, a LuaBackedBehavior can implement this interface
    /// by forwarding to the matching Lua module without changing the host.
    /// </summary>
    public interface ILuaBulletBehavior
    {
        void OnSpawn(LuaBulletHost host);
        void OnUpdate(LuaBulletHost host, float dt);
        void OnHit(LuaBulletHost host, GameObject target);
        void OnRecycle(LuaBulletHost host);
    }
}
