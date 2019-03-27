
namespace GoogleARCore.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.EventSystems;

#if UNITY_EDITOR
    using Input = InstantPreviewInput;
#endif

    public class Manager : Photon.MonoBehaviour
    {

        public Camera FirstPersonCamera;

        public GameObject TrackedPlanePrefab;

        public GameObject GreenAndyAndroidPrefab;

        public GameObject YellowAndyAndroidPrefab;

        public GameObject SearchingForPlaneUI;

        private List<TrackedPlane> m_NewPlanes = new List<TrackedPlane>();

        private List<TrackedPlane> m_AllPlanes = new List<TrackedPlane>();

        private bool m_IsQuitting = false;

        public string verNum = "0.1";

        public string roomName = "room01";

        public string playerName = "user 420";

        public Transform spawnPoint;

        public GameObject playerPref;

        public bool isConnected = false;

        public bool yellowandy = false;

        public bool greenandy = false;

        public GameObject NoSelection;

        public GameObject YellowSelection;

        public GameObject GreenSelection;

        public void Start()
        {
            roomName = "Room " + Random.Range(0, 999);
            playerName = "User " + Random.Range(0, 999);
            PhotonNetwork.ConnectUsingSettings(verNum);
            Debug.Log("Starting Connection!");
            NoSelection.SetActive(false);
            YellowSelection.SetActive(false);
            GreenSelection.SetActive(false);
        }

        public void OnJoinedLobby()
        {
            //PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
            isConnected = true;
            Debug.Log("Starting Server!");
        }

        public void OnJoinedRoom()
        {
            PhotonNetwork.playerName = playerName;
            isConnected = true;
            SpawnPlayer();
        }

        public void SpawnPlayer()
        {
            GameObject pl = PhotonNetwork.Instantiate(playerPref.name, spawnPoint.position, spawnPoint.rotation, 0) as GameObject;
            pl.GetComponent<ARCoreSession>().enabled = true;
            pl.GetComponent<ARCoreSession>().FirstPersonCamera.SetActive(true);
            NoSelection.SetActive(true);
            YellowSelection.SetActive(false);
            GreenSelection.SetActive(false);
        }

        void OnGUI()
        {

            if (isConnected)
            {
                GUI.skin.textField.fontSize = 40; 
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 500));
                playerName = GUILayout.TextField(playerName);
                roomName = GUILayout.TextField(roomName);

                if (GUILayout.Button("Create", GUILayout.Width(500), GUILayout.Height(100)))
                {
                    PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
                }

                foreach (RoomInfo game in PhotonNetwork.GetRoomList())
                {
                    if (GUILayout.Button(game.name + " " + game.playerCount + "/" + game.maxPlayers, GUILayout.Width(500), GUILayout.Height(100)))
                    {
                        PhotonNetwork.JoinOrCreateRoom(game.name, null, null);
                    }
                }
                GUILayout.EndArea();
            }
        }


        //Choosing the prefab.

        public void YellowAndy()
        {
            greenandy = false;
            yellowandy = true;
            NoSelection.SetActive(false);
            YellowSelection.SetActive(true);
            GreenSelection.SetActive(false);
        }

        //Choosing the prefab

        public void GreenAndy()
        {
            greenandy = true;
            yellowandy = false;
            NoSelection.SetActive(false);
            YellowSelection.SetActive(false);
            GreenSelection.SetActive(true);
        }


       public void Update()
        {
          
            _QuitOnConnectionErrors();

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
                if (!m_IsQuitting && Session.Status.IsValid())
                {
                    SearchingForPlaneUI.SetActive(true);
                }

                return;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Iterate over planes found in this frame and instantiate corresponding GameObjects to visualize them.
            Session.GetTrackables<TrackedPlane>(m_NewPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < m_NewPlanes.Count; i++)
            {
                // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
                // the origin with an identity rotation since the mesh for our prefab is updated in Unity World
                // coordinates.
                GameObject planeObject = Instantiate(TrackedPlanePrefab, Vector3.zero, Quaternion.identity,
                    transform);
                planeObject.GetComponent<TrackedPlaneVisualizer>().Initialize(m_NewPlanes[i]);
            }

            // Disable the snackbar UI when no planes are valid.
            Session.GetTrackables<TrackedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
                {
                    if (greenandy == true)
                    {
                        //GameObject o = PhotonNetwork.Instantiate(AndyAndroidPrefab.name, spawnPoint.position, spawnPoint.rotation, 0) as GameObject;
                        //var andyObject = Instantiate(AndyAndroidPrefab, hit.Pose.position, hit.Pose.rotation);
                        var o = PhotonNetwork.Instantiate(GreenAndyAndroidPrefab.name, hit.Pose.position, hit.Pose.rotation, 0);
                        // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                        // world evolves.
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                        // Andy should look at the camera but still be flush with the plane.
                        if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
                        {
                            // Get the camera position and match the y-component with the hit position.
                            Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
                            cameraPositionSameY.y = hit.Pose.position.y;

                            // Have Andy look toward the camera respecting his "up" perspective, which may be from ceiling.
                            o.transform.LookAt(cameraPositionSameY, o.transform.up);
                        }

                        // Make Andy model a child of the anchor.
                        o.transform.parent = anchor.transform;
                    }

                    if (yellowandy == true)
                    {
                        //GameObject o = PhotonNetwork.Instantiate(AndyAndroidPrefab.name, spawnPoint.position, spawnPoint.rotation, 0) as GameObject;
                        //var andyObject = Instantiate(AndyAndroidPrefab, hit.Pose.position, hit.Pose.rotation);
                        var o = PhotonNetwork.Instantiate(YellowAndyAndroidPrefab.name, hit.Pose.position, hit.Pose.rotation, 0);
                        // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                        // world evolves.
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                        // Andy should look at the camera but still be flush with the plane.
                        if ((hit.Flags & TrackableHitFlags.PlaneWithinPolygon) != TrackableHitFlags.None)
                        {
                            // Get the camera position and match the y-component with the hit position.
                            Vector3 cameraPositionSameY = FirstPersonCamera.transform.position;
                            cameraPositionSameY.y = hit.Pose.position.y;

                            // Have Andy look toward the camera respecting his "up" perspective, which may be from ceiling.
                            o.transform.LookAt(cameraPositionSameY, o.transform.up);
                        }

                        // Make Andy model a child of the anchor.
                        o.transform.parent = anchor.transform;
                    }
                }
            }
        }


        private void _QuitOnConnectionErrors()
        {
            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }


        private void _DoQuit()
        {
            Application.Quit();
        }

        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
