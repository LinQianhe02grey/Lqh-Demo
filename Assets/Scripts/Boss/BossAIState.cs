namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 55 — visualization-only Boss AI automaton state. This enum exists for the
    /// runtime monitor (MirrorAngelBossDebugState) and the portfolio documentation; it
    /// does NOT replace the brain's internal MirrorAngelBossBrainState and does NOT
    /// change any decision logic, skill values or cooldowns. The brain mirrors its
    /// internal transitions into this enum so the Inspector / Scene view / Console can
    /// show a clean, documented state machine.
    /// </summary>
    public enum BossAIState
    {
        Idle,
        Decide,
        Approach,
        KeepDistance,
        Reposition,
        Windup,
        Casting,
        Recovery,
        AirMode,
        Dead
    }
}
