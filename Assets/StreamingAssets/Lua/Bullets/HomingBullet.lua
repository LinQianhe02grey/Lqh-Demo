-- HomingBullet.lua  (behavior id: "Bullets.HomingBullet")
--
-- Canonical Lua behaviour format. Currently executed by the C# bridge
-- (Cardwin.Lua.HomingBulletBehavior) because no Lua VM is integrated yet; this file
-- is the format reference and future hot-swap target.
--
-- Steers toward the nearest enemy (turnSpeed deg/s) and recycles on first hit,
-- dealing damage equal to a percent of the target's max HP.
--
-- Extra API used here:
--   BattleAPI.FindNearestEnemy(pos) -> target or nil
--   BattleAPI.IsDead(target) -> bool
--   BattleAPI.DamagePercentOfMaxHp(target, percent)

local HomingBullet = {}

function HomingBullet.OnSpawn(host)
end

function HomingBullet.OnUpdate(host, dt)
    local def = host.Definition
    local pos = host.Position

    if host.CurrentTarget == nil or BattleAPI.IsDead(host.CurrentTarget) then
        host.CurrentTarget = BattleAPI.FindNearestEnemy(pos)
    end

    if host.CurrentTarget ~= nil then
        -- Steer Direction toward the target by at most turnSpeed * dt degrees,
        -- then move along the new Direction. (See HomingBulletBehavior for the
        -- exact angle interpolation the C# bridge performs.)
        BattleAPI.MoveToward(host, host.CurrentTarget.Position, def.Speed, dt)
    else
        BattleAPI.Move(host, host.Direction, def.Speed, dt)
    end
end

function HomingBullet.OnHit(host, target)
    -- damageMode == "PercentTargetMaxHp"; damage = fraction of target max HP.
    BattleAPI.DamagePercentOfMaxHp(target, host.Definition.Damage)
    BattleAPI.RecycleBullet(host)
end

function HomingBullet.OnRecycle(host)
end

return HomingBullet
