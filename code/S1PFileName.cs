namespace S1P_Multiviewer
{
    /// <summary>
    /// Parses the "Name[_Param1[_Param2[_Param3[_Param4]]]].s1p" file naming
    /// convention. A file may specify 0 to 4 parameters - any that are missing
    /// default to 0.
    /// </summary>
    public static class S1PFileName
    {
        public static bool TryParse(string filePath, out string projectName, out int param1, out int param2, out int param3, out int param4)
        {
            projectName = "";
            param1 = 0;
            param2 = 0;
            param3 = 0;
            param4 = 0;

            string[] parts = System.IO.Path.GetFileNameWithoutExtension(filePath).Split('_');

            // "Name" (0 params) up to "Name_Param1_Param2_Param3_Param4" (4 params).
            if (parts.Length < 1 || parts.Length > 5) return false;
            if (string.IsNullOrEmpty(parts[0])) return false;

            if (parts.Length >= 2 && !int.TryParse(parts[1], out param1)) return false;
            if (parts.Length >= 3 && !int.TryParse(parts[2], out param2)) return false;
            if (parts.Length >= 4 && !int.TryParse(parts[3], out param3)) return false;
            if (parts.Length >= 5 && !int.TryParse(parts[4], out param4)) return false;

            projectName = parts[0];
            return true;
        }
    }
}
