using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StreamingAssetsManager : MonoBehaviour
{
    [SerializeField] private bool copyOnStart = true;
    [SerializeField] private bool overwriteExistingFiles = true;
    [SerializeField] private List<string> fileExtensionsToSkip = new List<string>();

    private int totalFiles = 0;
    private int copiedFiles = 0;
    int skippedFiles = 0;
    private bool isCopyingComplete = false;
    AudioLibrary library;
    // List of paths to copy (filled at runtime)
    private List<string> filesToCopy = new List<string>();

    void Start()
    {
        library = FindAnyObjectByType<AudioLibrary>();
        if (copyOnStart)
        {
            Debug.Log("HURO: Start file copy");
            StartCoroutine(CopyAllFilesFromStreamingAssets());
        }
    }

    public IEnumerator CopyAllFilesFromStreamingAssets()
    {
        Debug.Log("Starting to copy files from StreamingAssets to PersistentDataPath");
        isCopyingComplete = false;
        copiedFiles = 0;

        // For Android, we need to read the manifest file
        if (Application.platform == RuntimePlatform.Android)
        {
            yield return StartCoroutine(ReadManifestFile());
        }
        else
        {
            // For other platforms, we can scan the directory directly
            ScanDirectory(Application.streamingAssetsPath, "");
        }

        totalFiles = filesToCopy.Count;
        Debug.Log($"Found {totalFiles} files to copy");

        // Now copy all the files
        foreach (string relativePath in filesToCopy)
        {
            
            yield return StartCoroutine(CopyFileFromStreamingAssets(relativePath));
        }

        isCopyingComplete = true;
        Debug.Log($"HURO: Completed copying {copiedFiles}/{totalFiles} files to {Application.persistentDataPath}");
    }

    private IEnumerator ReadManifestFile()
    {
        string manifestPath = Path.Combine(Application.streamingAssetsPath, "manifest.txt");
        UnityWebRequest www = UnityWebRequest.Get(manifestPath);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string[] lines = www.downloadHandler.text.Split('\n');
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    if (!ShouldSkipFile(trimmedLine))
                    {
                        filesToCopy.Add(trimmedLine);
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Failed to load manifest file: {www.error}");
        }
    }

    private void ScanDirectory(string fullPath, string relativePath)
    {
        foreach (string file in Directory.GetFiles(fullPath))
        {
            string fileName = Path.GetFileName(file);
            string relativeFilePath = string.IsNullOrEmpty(relativePath) ? fileName : Path.Combine(relativePath, fileName);

            if (fileName == "manifest.txt")
                continue;

            if (!ShouldSkipFile(relativeFilePath))
            {
                filesToCopy.Add(relativeFilePath);
            }
        }

        foreach (string directory in Directory.GetDirectories(fullPath))
        {
            string dirName = Path.GetFileName(directory);
            string relativeDirPath = string.IsNullOrEmpty(relativePath) ? dirName : Path.Combine(relativePath, dirName);
            ScanDirectory(directory, relativeDirPath);
        }
    }

    private bool ShouldSkipFile(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        return fileExtensionsToSkip.Contains(extension);
    }

    private IEnumerator CopyFileFromStreamingAssets(string relativeFilePath)
    {
        string sourceUri = Path.Combine(Application.streamingAssetsPath, relativeFilePath);
        string destPath = Path.Combine(Application.persistentDataPath, relativeFilePath);

        // Ensure the destination directory exists
        string destDir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        // Check if we should skip this file (if it already exists)
        if (File.Exists(destPath) && !overwriteExistingFiles)
        {
            Debug.Log($"Skipping file (already exists): {relativeFilePath}");
            copiedFiles++;
            skippedFiles++;
            yield break;
        }

        // Use UnityWebRequest for all platforms for consistency
        UnityWebRequest www = UnityWebRequest.Get(sourceUri);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                File.WriteAllBytes(destPath, www.downloadHandler.data);
                copiedFiles++;
                Debug.Log($"Copied file: {relativeFilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error writing file {destPath}: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"Error downloading file {sourceUri}: {www.error}");
        }
    }

    public bool IsCopyingComplete()
    {
        return isCopyingComplete;
    }

    public float GetCopyProgress()
    {
        return totalFiles > 0 ? (float)copiedFiles / totalFiles : 0f;
    }
}

#if UNITY_EDITOR
// Simple editor script to generate manifest.txt
public class ManifestGenerator
{
    [MenuItem("Tools/Generate StreamingAssets Manifest")]
    public static void GenerateManifest()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;

        if (!Directory.Exists(streamingAssetsPath))
        {
            Directory.CreateDirectory(streamingAssetsPath);
        }

        List<string> allFiles = new List<string>();
        CollectFiles(streamingAssetsPath, "", allFiles);

        string manifestPath = Path.Combine(streamingAssetsPath, "manifest.txt");

        // Remove the manifest itself from the list
        allFiles.RemoveAll(path => path == "manifest.txt");
        allFiles.ForEach(path => path.Replace("\\", "/"));

        File.WriteAllLines(manifestPath, allFiles.ToArray());

        Debug.Log($"Generated manifest.txt with {allFiles.Count} files");

        // Refresh the assets database
        AssetDatabase.Refresh();
    }

    private static void CollectFiles(string fullPath, string relativePath, List<string> files)
    {
        foreach (string file in Directory.GetFiles(fullPath))
        {
            string fileName = Path.GetFileName(file);
            string relativeFilePath = string.IsNullOrEmpty(relativePath) ? fileName : Path.Combine(relativePath, fileName);
            files.Add(relativeFilePath);
        }

        foreach (string directory in Directory.GetDirectories(fullPath))
        {
            string dirName = Path.GetFileName(directory);
            string relativeDirPath = string.IsNullOrEmpty(relativePath) ? dirName : Path.Combine(relativePath, dirName);
            CollectFiles(directory, relativeDirPath, files);
        }
    }
}
#endif