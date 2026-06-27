-- BulletRegistry.lua
-- Canonical registry of Lua-defined bullets for Card_G.
-- Parsed at runtime by Cardwin.Lua.LuaBulletDatabase (in-house Lua-table parser).
-- CRUD:
--   Create : add a new id table under bullets.
--   Read   : LuaBulletDatabase.Get / List* in C#.
--   Update : edit any field, then LuaBulletDatabase.ReloadLuaBullets().
--   Delete : soft-delete with enabled = false (kept for old saves, never re-fired/dropped).

return {
    version = 1,

    bullets = {
        lua_pierce_001 = {
            enabled = true,

            display = {
                name = "Lua Pierce Bullet",
                desc = "穿透多个敌人的热更新子弹",
                icon = "Icon_LuaPierce",
                sprite = "Bullet_LuaPierce",
                rarity = "Rare"
            },

            card = {
                cardType = "Attack",
                tags = { "Lua", "Attack", "Projectile", "Pierce" },
                leftClickEffect = "LuaBullet",
                rightClickEffect = "None"
            },

            bullet = {
                prefab = "LuaBulletHost",
                behavior = "Bullets.PierceBullet",

                speed = 12,
                lifeTime = 4,
                damage = 8,
                damageMode = "Flat",

                pierceCount = 3,
                visualScale = 1.5,
                hitRadius = 0.35
            },

            inventory = {
                enabled = true,
                defaultCount = 8,
                addToBackpack = true
            },

            drop = {
                enabled = true,
                weight = 20,
                enemies = { "MeleeEnemy", "RangedEnemy" },
                minNight = 1
            }
        },

        lua_homing_001 = {
            enabled = true,

            display = {
                name = "Lua Homing Bullet",
                desc = "自动追踪最近敌人的热更新子弹",
                icon = "Icon_LuaHoming",
                sprite = "Bullet_LuaHoming",
                rarity = "Epic"
            },

            card = {
                cardType = "Attack",
                tags = { "Lua", "Attack", "Projectile", "Homing" },
                leftClickEffect = "LuaBullet",
                rightClickEffect = "None"
            },

            bullet = {
                prefab = "LuaBulletHost",
                behavior = "Bullets.HomingBullet",

                speed = 10,
                turnSpeed = 720,
                lifeTime = 5,
                damage = 0.03,
                damageMode = "PercentTargetMaxHp",

                visualScale = 2.0,
                hitRadius = 0.45
            },

            inventory = {
                enabled = true,
                defaultCount = 4,
                addToBackpack = true
            },

            drop = {
                enabled = true,
                weight = 10,
                enemies = { "MeleeEnemy", "RangedEnemy", "BossRoomEnemy" },
                minNight = 1
            }
        }
    }
}
