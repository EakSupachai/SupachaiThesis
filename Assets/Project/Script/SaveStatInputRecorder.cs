
public class SaveStatInputRecorder
{
    public static string fileName = "";
    public static int blinkInsideFalseTrigger = 0;
    public static int blinkOutsideFalseTrigger = 0;
    public static bool fileName_recordValid = false;
    public static bool blinkInsideFalseTrigger_recordValid = false;
    public static bool blinkOutsideFalseTrigger_recordValid = false;

    public static void ResetValue()
    {
        fileName = "";
        blinkInsideFalseTrigger = 0;
        blinkOutsideFalseTrigger = 0;
        fileName_recordValid = false;
        blinkInsideFalseTrigger_recordValid = false;
        blinkOutsideFalseTrigger_recordValid = false;
    }
}
