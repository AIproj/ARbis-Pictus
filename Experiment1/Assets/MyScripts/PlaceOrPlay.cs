// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HoloToolkit.Unity.SpatialMapping
{
    /// <summary>
    /// TapToPlace if in admin mode, PlaySound if in user mode
    /// </summary>

    public class PlaceOrPlay : MonoBehaviour, IInputClickHandler
    {
        [Tooltip("Supply a friendly name for the anchor as the key name for the WorldAnchorStore.")]
        private string SavedAnchorFriendlyName;

        [Tooltip("Place parent on tap instead of current game object.")]
        public bool PlaceParentOnTap;

        [Tooltip("Specify the parent game object to be moved on tap, if the immediate parent is not desired.")]
        public GameObject ParentGameObjectToPlace;

        /// <summary>
        /// Keeps track of if the user is moving the object or not.
        /// Setting this to true will enable the user to move and place the object in the scene.
        /// Useful when you want to place an object immediately.
        /// </summary>
        [Tooltip("Setting this to true will enable the user to move and place the object in the scene without needing to tap on the object. Useful when you want to place an object immediately.")]
        public bool IsBeingPlaced;

        /// <summary>
        /// Manages persisted anchors.
        /// </summary>
        protected WorldAnchorManager anchorManager;

        /// <summary>
        /// Controls spatial mapping.  In this script we access spatialMappingManager
        /// to control rendering and to access the physics layer mask.
        /// </summary>
        protected SpatialMappingManager spatialMappingManager;

        private AudioSource wordSpoken;

        // Keeps track of when the user starts looking at the label
        private long timeIn;

        // Safety to make sure user mode is not switched on while a user's looking at a panel (leading to inaccurate time tracking)
        private bool timeSafety = true;

        // Use this for initialization
        void Awake()
        {
            wordSpoken = GetComponent<AudioSource>();
            SavedAnchorFriendlyName = gameObject.name;
        }

        protected virtual void Start()
        {
            // Make sure we have all the components in the scene we need.
            anchorManager = WorldAnchorManager.Instance;
            if (anchorManager == null)
            {
                Debug.LogError("This script expects that you have a WorldAnchorManager component in your scene.");
            }

            spatialMappingManager = SpatialMappingManager.Instance;
            if (spatialMappingManager == null)
            {
                Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
            }

            if (anchorManager != null && spatialMappingManager != null)
            {
                anchorManager.AttachAnchor(gameObject, SavedAnchorFriendlyName);
            }
            else
            {
                // If we don't have what we need to proceed, we may as well remove ourselves.
                Destroy(this);
            }

            if (PlaceParentOnTap)
            {
                if (ParentGameObjectToPlace != null && !gameObject.transform.IsChildOf(ParentGameObjectToPlace.transform))
                {
                    Debug.LogError("The specified parent object is not a parent of this object.");
                }

                DetermineParent();
            }
        }

        protected virtual void Update()
        {
            // If the user is in placing mode,
            // update the placement to match the user's gaze.
            if (IsBeingPlaced)
            {
                // Do a raycast into the world that will only hit the Spatial Mapping mesh.
                Vector3 headPosition = Camera.main.transform.position;
                Vector3 gazeDirection = Camera.main.transform.forward;

                RaycastHit hitInfo;
                if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 30.0f, spatialMappingManager.LayerMask))
                {
                    // Rotate this object to face the user.
                    Quaternion toQuat = Camera.main.transform.localRotation;
                    toQuat.x = 0;
                    toQuat.z = 0;

                    // Move this object to where the raycast
                    // hit the Spatial Mapping mesh.
                    // Here is where you might consider adding intelligence
                    // to how the object is placed.  For example, consider
                    // placing based on the bottom of the object's
                    // collider so it sits properly on surfaces.
                    if (PlaceParentOnTap)
                    {
                        // Place the parent object as well but keep the focus on the current game object
                        Vector3 currentMovement = hitInfo.point - gameObject.transform.position;
                        ParentGameObjectToPlace.transform.position += currentMovement;
                        ParentGameObjectToPlace.transform.rotation = toQuat;
                    }
                    else
                    {
                        gameObject.transform.position = hitInfo.point;
                        gameObject.transform.rotation = toQuat;
                    }
                }
            }
        }

        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            if (SpeechManager.Instance.clickMode)
            {// On each tap gesture, toggle whether the user is in placing mode.
                IsBeingPlaced = !IsBeingPlaced;

                // If the user is in placing mode, display the spatial mapping mesh.
                if (IsBeingPlaced)
                {
                    spatialMappingManager.DrawVisualMeshes = true;

                    Debug.Log(gameObject.name + " : Removing existing world anchor if any.");

                    anchorManager.RemoveAnchor(gameObject);
                }
                // If the user is not in placing mode, hide the spatial mapping mesh.
                else
                {
                    spatialMappingManager.DrawVisualMeshes = false;
                    // Shift gameObject position up so that the label floats above the real world object
                    gameObject.transform.position += new Vector3(0, MySceneManager.Instance.yShift, 0);
                    // Add world anchor when object placement is done.
                    anchorManager.AttachAnchor(gameObject, SavedAnchorFriendlyName);
                }
            }
            else
            {
                wordSpoken.Stop();
                wordSpoken.Play();
            }
        }

        private void DetermineParent()
        {
            if (ParentGameObjectToPlace == null)
            {
                if (gameObject.transform.parent == null)
                {
                    Debug.LogError("The selected GameObject has no parent.");
                    PlaceParentOnTap = false;
                }
                else
                {
                    Debug.LogError("No parent specified. Using immediate parent instead: " + gameObject.transform.parent.gameObject.name);
                    ParentGameObjectToPlace = gameObject.transform.parent.gameObject;
                }
            }
        }

        //private void OnMouseEnter()
        //{
        //    MySceneManager.Instance.cursorIsSpeaker = true;
        //}

        //private void OnMouseExit()
        //{
        //    MySceneManager.Instance.cursorIsSpeaker = false;
        //}
        private void OnCollisionEnter(Collision col)
        {
            MySceneManager.Instance.cursorIsSpeaker = true;
            MySceneManager.Instance.currentLabel = gameObject.name;
            timeIn = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            if (SpeechManager.Instance.recordTime)
            {
                timeSafety = false;
            }
        }

        private void OnCollisionExit(Collision col)
        {
            MySceneManager.Instance.cursorIsSpeaker = false;
            MySceneManager.Instance.lastLabel = gameObject.name;

            // Report time spent looking at the label to the study logger
            //long deltaT = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds - timeIn;
            if (SpeechManager.Instance.recordTime && !timeSafety)
            {
                timeSafety = true;

                UserStudyLogger.Instance.Record(gameObject.name, timeIn.ToString(), ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());
            }

            // Safety in case two labels are intersecting so switching between the two may cause a race condition
            if (MySceneManager.Instance.currentLabel != MySceneManager.Instance.lastLabel)
            {
                MySceneManager.Instance.cursorIsSpeaker = true;
            }
        }
    }
}
