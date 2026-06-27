-- PierceBullet.lua  (behavior id: "Bullets.PierceBullet")
--
-- Canonical Lua behaviour format for the Card_G Lua bullet system.
--
-- NOTE (current limitation): no Lua VM (xLua/tolua) is integrated yet, so this file
-- is the FORMAT REFERENCE + future hot-swap target. Behaviour is currently executed
-- by the matching C# bridge (Cardwin.Lua.PierceBulletBehavior). Once a Lua VM is
-- added, a LuaBackedBehavior can run this file unchanged.
--
-- Callbacks:
--   OnSpawn(host)        once when the bullet is created
--   OnUpdate(host, dt)   every frame
--   OnHit(host, target)  when the bullet overlaps a combat target
--   OnRecycle(host)      once before the bullet is destroyed
--
-- Available to behaviours:
--   host.Definition      the bullet definition (Speed, Damage, DamageMode, PierceCount, ...)
--   host.Direction       current flight direction (Vector2)
--   host.RemainingPierce remaining pierce count
--   BattleAPI.Move(host, dir, speed, dt)
--   BattleAPI.Damage(target, amount)
--   BattleAPI.RecycleBullet(host)

local PierceBullet = {}

function PierceBullet.OnSpawn(host)
    -- RemainingPierce is initialised from definition.PierceCount by the host.
end

function PierceBullet.OnUpdate(host, dt)
    local def = host.Definition
    BattleAPI.Move(host, host.Direction, def.Speed, dt)
end

function PierceBullet.OnHit(host, target)
    -- damageMode == "Flat"
    BattleAPI.Damage(target, math.floor(host.Definition.Damage + 0.5))

    host.RemainingPierce = host.RemainingPierce - 1
    if host.RemainingPierce <= 0 then
        BattleAPI.RecycleBullet(host)
    end
end

function PierceBullet.OnRecycle(host)
end

return PierceBullet
