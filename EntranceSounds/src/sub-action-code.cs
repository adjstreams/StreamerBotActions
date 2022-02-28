using System;

public class CPHInline
{
    public bool Execute()
    {
        var path = args["pathToMp3s"].ToString();
        var volume = ConvertToFloatOrDefault(args["volume"]);
        var waitUntilFinished = ConvertToBoolOrDefault(args["waitUntilFinished"]);
        var userName = args["userName"].ToString();

		    try {
			    CPH.PlaySound(path + userName + ".mp3", volume, waitUntilFinished);
		    } catch(Exception e) {
			    // swallow the error
		    }

        return true;
    }

    public float ConvertToFloatOrDefault(object value, float defaultValue = 1.0f)
    {
        string str = value as string;
        if (float.TryParse(str, out float percentage))
        {
            return percentage / 100;
        }

        return defaultValue;
    }

    public bool ConvertToBoolOrDefault(object value, bool defaultValue = true)
    {
        string str = value as string;
        if (string.IsNullOrEmpty(str))
        {
            return defaultValue;
        }

        return str?.ToLower() switch
        {
          "true" => true,
          "yes" => true,
          "y" => true,
          "t" => true,
          "1" => true,
          _ => false
        };
    }
}
