namespace MenuFixes;

public static class Compat
{
    private static bool _NMUC => typeof(NMUC.NoModUpdateConfirm) != null;
    public static bool NMUC()
    {
        try { return _NMUC; }
        catch { return false; }
    }

    private static bool _RAR => typeof(RemixAutoRestart.RemixAutoRestart) != null;
    public static bool RAR()
    {
        try { return _RAR; }
        catch { return false; }
    }
}
