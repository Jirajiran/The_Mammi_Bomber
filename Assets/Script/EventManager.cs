using System;

public static class EventManager
{
    public static Action<int> OnHPChanged;
    public static Action<int> OnPointChanged;
    public static Action<int> OnTakeDamage;
    public static Action<int> OnWinFlagReached;
    public static Action OnGameWin;
}
