using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

public static class FileManager {

    private static readonly Guid FOLDER_SAVED_GAMES = new Guid("4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4");
    private static readonly Guid FOLDER_ONEDRIVE = new Guid("A52BBA46-E9E1-435f-B3D9-28DAA648C0F6");

    private static readonly string FILESTORE_ROOT;

    static FileManager() {
        if (Environment.OSVersion.Version.Major < 6) {
            Debug.Log("Saved games folder not available, using persistent data path instead");
            FILESTORE_ROOT = Application.persistentDataPath;
            return;
        }

        IntPtr pathPtr = IntPtr.Zero;

        // Try the OneDrive first
        try {
            pathPtr = IntPtr.Zero;
            SHGetKnownFolderPath(ref FOLDER_ONEDRIVE, 0, IntPtr.Zero, out pathPtr);
            FILESTORE_ROOT = Path.GetFullPath(Marshal.PtrToStringUni(pathPtr) + "/Saved Games/" + Application.productName);

            return;
        } catch {
            // Ignore
        } finally {
            Marshal.FreeCoTaskMem(pathPtr);
        }

        // Try Saved Games
        try {
            pathPtr = IntPtr.Zero;
            SHGetKnownFolderPath(ref FOLDER_SAVED_GAMES, 0, IntPtr.Zero, out pathPtr);
            FILESTORE_ROOT = Path.GetFullPath(Marshal.PtrToStringUni(pathPtr) + "/" + Application.productName);

            return;
        } catch {
            Debug.Log("Failed to resolve saved games folder, using persistent data path instead");
            FILESTORE_ROOT = Application.persistentDataPath;
        } finally {
            Marshal.FreeCoTaskMem(pathPtr);
        }
    }

    /**
     * Write the given file to the given name, grouped by the given type (which will become a directory)
     */
    public static void WriteFile(string type, string name, string content) {
        string fullPath;
        WriteFile(type, name, content, out fullPath);
    }

    /**
     * Write the given file to the given name, grouped by the given type (which will become a directory).
     * Returns the full path the file was saved to in the out parameter
     */
    public static void WriteFile(string type, string name, string content, out string fullPath) {
        string folder = FILESTORE_ROOT + "/" + type;
        fullPath = Path.GetFullPath(folder + "/" + name);

        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(fullPath, content);
    }

    /**
     * Given a type and name return the content associated with the file. Returns null if the file does 
     * not exist
     */
    public static string ReadFile(string type, string name) {
        string fullPath;
        return ReadFile(type, name, out fullPath);
    }

    /**
     * Given a type and name return the content associated with the file. Returns null if the file does 
     * not exist. The full path used to load the file is returned in the out parameter.
     */
    public static string ReadFile(string type, string name, out string fullPath) {
        string folder = FILESTORE_ROOT + "/" + type;
        fullPath = Path.GetFullPath(folder + "/" + name);

        if (!File.Exists(fullPath)) {
            return null;
        }

        return File.ReadAllText(fullPath);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
}
