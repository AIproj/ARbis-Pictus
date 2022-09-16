using UnityEngine;
using HoloToolkit.Unity;

using System.Text;
using System.Collections.Generic;
using System.Threading;

#if !UNITY_EDITOR
using System;
using Windows.Storage;
using System.Collections.Generic;
using System.Collections.Concurrent;
#endif


public class UserStudyLogger : Singleton<UserStudyLogger>
{
    
    public float samplesPerSecond = 1.0f;
    public float writeToFilePerSecond = 1.0f;
    public string FileName = "UserData";
    public string FolderName = "UserData";

    private bool EnableRecording = false;
    private float samplingDeltaTime = 0;
    private float writeToFileDeltaTime = 0;
    
#if !UNITY_EDITOR
    private StorageFolder saveFolder;
    private StorageFile saveFile;
    private bool haveFolderPath = false, haveFilePath = false;
    private ConcurrentQueue<Tuple<Vector3, Vector3>> transformQueue = new ConcurrentQueue<Tuple<Vector3, Vector3>>();
    private static SemaphoreSlim writeLogLock = new SemaphoreSlim(1, 1);
#endif

    void Start()
    {
    }

    public void CreateLogFile(string userID)
    {
#if !UNITY_EDITOR
        OpenStorageFolderAndFile(userID);
#endif
    }

#if !UNITY_EDITOR
    private async void OpenStorageFolderAndFile(string userID)
    {
        saveFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // http://stackoverflow.com/questions/33801760/universal-windows-platform-zipfile-createfromdirectory-creates-empty-zip-file
        saveFolder = await saveFolder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists);
        haveFolderPath = true;
        saveFile = await saveFolder.CreateFileAsync(FileName + userID + ".csv", CreationCollisionOption.GenerateUniqueName);
        await Windows.Storage.FileIO.AppendLinesAsync(saveFile, new string[] { "Object Name, Time In, Time Out" });
        haveFilePath = true;
    }

    private async void Record(string[] content)
    {
        if (!haveFolderPath || !haveFilePath)
        {
            return;
        }

        if (content.Length > 0)
        {
            await writeLogLock.WaitAsync();
            try
            {
                await Windows.Storage.FileIO.AppendLinesAsync(saveFile, content);
            }
            finally
            {
                writeLogLock.Release();
            }
        }
    }
#endif

    public void Record(string objectName, string timeIn, string timeOut)
    {
        string content = objectName + "," + timeIn + "," + timeOut;
#if !UNITY_EDITOR
        Record(new string[]{content});
#endif
    }
}
