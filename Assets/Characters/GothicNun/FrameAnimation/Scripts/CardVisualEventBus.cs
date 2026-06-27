using System;

namespace Cardwin.Characters
{
    public static class CardVisualEventBus
    {
        public static event Action<VisualActionType, float> OnVisualAction;

        public static void Notify(VisualActionType action, float shotDirectionX = 0f)
        {
            OnVisualAction?.Invoke(action, shotDirectionX);
        }
    }
}
