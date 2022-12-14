// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// Sets Force Text Serialization and visible meta files in all projects that use the HoloToolkit.
    /// </summary>
    [InitializeOnLoad]
    public class EnforceEditorSettings
    {
        static EnforceEditorSettings()
        {
            #region Editor Settings

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorSettings.serializationMode = SerializationMode.ForceText;
                UnityEngine.Debug.Log("Setting Force Text Serialization");
            }

            if (!EditorSettings.externalVersionControl.Equals("Visible Meta Files"))
            {
                EditorSettings.externalVersionControl = "Visible Meta Files";
                UnityEngine.Debug.Log("Updated external version control mode: " + EditorSettings.externalVersionControl);
            }

            #endregion
        }
    }
}