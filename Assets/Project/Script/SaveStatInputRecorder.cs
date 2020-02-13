
public class SaveStatInputRecorder
{
    public static string fileName = "";
    public static int shootFalseTrigger = 0;
    public static int coreFalseTrigger = 0;
    public static int blinkInsideFalseTrigger = 0;
    public static int blinkOutsideFalseTrigger = 0;
    public static bool fileName_recordValid = false;
    public static bool shootFalseTrigger_recordValid = false;
    public static bool coreFalseTrigger_recordValid = false;
    public static bool blinkInsideFalseTrigger_recordValid = false;
    public static bool blinkOutsideFalseTrigger_recordValid = false;

    public static void ResetValue()
    {
        fileName = "";
        shootFalseTrigger = 0;
        coreFalseTrigger = 0;
        blinkInsideFalseTrigger = 0;
        blinkOutsideFalseTrigger = 0;
        fileName_recordValid = false;
        shootFalseTrigger_recordValid = false;
        coreFalseTrigger_recordValid = false;
        blinkInsideFalseTrigger_recordValid = false;
        blinkOutsideFalseTrigger_recordValid = false;
    }
}
