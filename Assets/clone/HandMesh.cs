using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System.Collections.Specialized;

using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;


public class HandMesh : MonoBehaviour, IMixedRealityHandMeshHandler, IMixedRealitySpeechHandler
{
    private Handedness myHandedness = Handedness.None;
    //public Mesh MyMesh;
    public Quaternion rotationpose;
    public Quaternion rotation;
    public bool save = false;
    public bool recording = false;
    private GameObject Hand;
    public Material ActiveMesh2;
    public Material PassiveMesh2;
    public Material ActiveMesh;
    public Material PassiveMesh;
    public Material ActiveMesh_active;
    public Material PassiveMesh_active;
    public bool ColorState = true;
    public bool ReplayInit = true;
    public GameObject cameraPose;

    public GameObject ButtonReplay;
    public GameObject ButtonRecord;

    public GameObject HomeMenu;
    public GameObject RouteMenu;
    public GameObject StartMenu;

    public GameObject StartRecordFeedback;
    public GameObject PauseFeedback;
    public GameObject CelebFeedback;
    public GameObject GoFeedback;
    public GameObject VoiceCommands;
    public GameObject feedback_to_vis;

    public bool ReplayActive;
    public bool RecordActive;
    public bool replaying;
    public int current_grasp;
    public int frame_counter;
    public int frame_count_start;
    public int visualize_frames;
    public int route_id = 0;
    public float posDiffThreshold = 0.15f;


    public string filename;

    public Vector3 lastRightPosition = new Vector3(0, 0, 0);
    public Vector3 lastLeftPosition = new Vector3(0, 0, 0);
    public int bin_right_hand = 0;
    public int bin_left_hand = 0;
    public int threshold = 20;
    public float movingThreshold = 0.2f;
    public float posThreshold = 0.1f;
    public bool right_hand_grasp = false;
    public bool left_hand_grasp = false;

    //private Stack<GameObject> Handstack = new Stack<GameObject>();

    public List<GameObject> Handlist = new List<GameObject>();
    //Spatial Anchors
    public int Count = 0;

    public Dictionary<String, Mesh> sessionindependance = new Dictionary<string, Mesh>();
    private SpatialAnchorManager _spatialAnchorManager = null;
    private List<GameObject> _foundOrCreatedAnchorGameObjects = new List<GameObject>();
    private List<String> _createdAnchorIDs = new List<String>();
    public Dictionary<String, Mesh> CloudMeshDict = new Dictionary<string, Mesh>();
    public List<int> HandednesList = new List<int>();

    //storing in JSON file
    MeshSerializer meshserializer;
    //public GameObject SpawnCube;
    //public GameObject camera;
    public void Start()
    {
        Debug.Log("Losgehts");
        //ButtonReplay.GetComponent
        //GetComponent<MeshFilter>().mesh = MyMesh;
        ReplayActive = false;
        RecordActive = false;
        replaying = false;
        current_grasp = 0;
        frame_counter = 0;
        feedback_to_vis = null;
        frame_count_start=0;
        StartRecordFeedback.SetActive(false);
        PauseFeedback.SetActive(false); 
        CelebFeedback.SetActive(false);
        GoFeedback.SetActive(false);    
        RouteMenu.SetActive(false);
        VoiceCommands.SetActive(false);
        StartMenu.SetActive(false);

        ActiveMesh_active = ActiveMesh2;
        PassiveMesh_active = PassiveMesh2;
        ReplayInit = false;

        visualize_frames = 60;

        _spatialAnchorManager = GetComponent<SpatialAnchorManager>();
        _spatialAnchorManager.LogDebug += (sender, args) => Debug.Log($"ASA - Debug: {args.Message}");
        _spatialAnchorManager.Error += (sender, args) => Debug.LogError($"ASA - Error: {args.ErrorMessage}");
        _spatialAnchorManager.AnchorLocated += SpatialAnchorManager_AnchorLocated;
        meshserializer = this.GetComponent<MeshSerializer>();


        meshserializer.SetPaths();
    }
    //public void handstackToggle(bool boolean)
    //{
    //    for (int i = 0; i < Handstack.Count; i++)
    //    {
    //        Handstack.ElementAt(i).SetActive(boolean);
    //    }
    //}
    public void deleteHandlist()
    {
        foreach(var objToDel in Handlist)
        {
            Destroy(objToDel);
        }
        Handlist.Clear();
        HandednesList.Clear();
    }
    public void handlistPassive()
    {
        for (int i = 0; i < Handlist.Count; i++)
        {
            Handlist[i].GetComponent<MeshRenderer>().material = PassiveMesh_active;
        }
    }
    public void replayHandler()
    {
        if (ReplayActive == false)
        {
            ReplayActive = true;
            RouteMenu.SetActive(true);
        }
        else 
        {
            ReplayActive=false;
        }
        

    }
    public void recordHandler()
    {
        if (RecordActive == false)
        {
            RouteMenu.SetActive(true);
            RecordActive = true;
            
        }
        else
        {
            RecordActive = false;
        }
    }
    public void Route1Handler()
    {
        route_id = 1;
        filename = "Route1";
        PostRouteMenu();
    }
    public void Route2Handler()
    {
        route_id = 2;
        filename = "Route2";
        PostRouteMenu();
    }
    public void Route3Handler()
    {
        route_id = 3;
        filename = "Route3";
        PostRouteMenu();
    }
    public void PostRouteMenu()
    {
        visualize_frames = 360;
        frame_count_start = frame_counter;
        feedback_to_vis = VoiceCommands;
        RouteMenu.SetActive(false);
        if (ReplayActive == true)
        {
            
            ReplayInit = true;
            Locate();
            //Thread.Sleep(6000);
            
            //Debug.Log("!!!!!!!!!!!!!!!!this is Handlist Count" + Handlist.Count.ToString());
            //if (Handlist.Count>0)
            //{

            //    handlistPassive();
            //    current_grasp = 0;
            //    Handlist[current_grasp].GetComponent<MeshRenderer>().material = ActiveMesh;
            //    //handstackToggle(true); //if replay get handsstack from path
            //    Vector3 startmenuPosition = Handlist[current_grasp].transform.position;
            //    startmenuPosition.y -= 0.05f;
            //    StartMenu.transform.position = startmenuPosition;
                
                
                
            //}
            //StartMenu.SetActive(true);
        }
        
    }
    public void playButtonHandler()
    {
        frame_count_start = frame_counter;
        feedback_to_vis = GoFeedback;   
        replaying = true;
        StartMenu.SetActive(false);
    }
    public void changeColorButtonHandler()
    {
        if (ColorState == true)
        {
            ActiveMesh_active = ActiveMesh;
            PassiveMesh_active = PassiveMesh;
            ColorState = false;
        }
        else
        {
            ActiveMesh_active = ActiveMesh2;
            PassiveMesh_active = PassiveMesh2;
            ColorState = true;
        }
        handlistPassive();
        Handlist[current_grasp].GetComponent<MeshRenderer>().material = ActiveMesh_active;

    }
    public void visualizeFeedback()
    {
        if (feedback_to_vis != null)
        {
            if (frame_counter - frame_count_start < visualize_frames)
            {
                feedback_to_vis.SetActive(true);
            }
            else
            {
                feedback_to_vis.SetActive(false);
                visualize_frames = 60;
                feedback_to_vis = null;
            }
        }
    }


    private void OnEnable()
    {
        // Instruct Input System that we would like to receive all input events of type
        // IMixedRealitySourceStateHandler and IMixedRealityHandJointHandler
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandMeshHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler> (this);

    }
    public void OnHandMeshUpdated(InputEventData<HandMeshInfo> eventData)
    {
        if (eventData.Handedness == myHandedness && save == true && recording == true)
        {
            Mesh MyMesh = new Mesh();
            MyMesh.Clear();
            MyMesh.vertices = eventData.InputData.vertices;
            MyMesh.normals = eventData.InputData.normals;
            MyMesh.triangles = eventData.InputData.triangles;

            if (eventData.InputData.uvs != null && eventData.InputData.uvs.Length > 0)
            {
                MyMesh.uv = eventData.InputData.uvs;
                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, myHandedness, out MixedRealityPose pose))
                {
                    rotation = Quaternion.Euler(-67.5f, 0, 180);
                    //transform.position = pose.Position;
                    //transform.rotation = pose.Rotation * rotation;
                    Hand = new GameObject("Hand");
                    Hand.AddComponent<MeshRenderer>();
                    Hand.GetComponent<MeshRenderer>().material = ActiveMesh2;
                    //if (myHandedness == Handedness.Right)
                    //{
                    //    Hand.GetComponent<MeshRenderer>().material = NiceHandRight;
                    //}
                    //else
                    //{
                    //    Hand.GetComponent<bool>().handedness = false;
                    //}
                    Hand.AddComponent<MeshFilter>();
                    Hand.GetComponent<MeshFilter>().mesh = MyMesh;
                    Hand.transform.position = pose.Position;
                    Hand.transform.rotation = pose.Rotation * rotation;
                    //= Instantiate(Hand, pose.Position, pose.Rotation* rotation);
                    GraspDetected(pose.Position, pose.Rotation, MyMesh, Hand, myHandedness);
                    //Handstack.Push(Hand);
                    //saveleft = false;
                    //saveright = false;
                    save = false;
                    myHandedness = Handedness.None;
                }
            }
        }
    }

    public void Update()
    {
        if (ReplayInit==true)
        {
            
            if (frame_counter - frame_count_start > 300)
            {
                //Debug.Log("!!!!!!!!!!!!!!!!this is Handlist Count" + Handlist.Count.ToString());
                if (Handlist.Count > 0)
                {
                    handlistPassive();
                    current_grasp = 0;
                    Handlist[current_grasp].GetComponent<MeshRenderer>().material = ActiveMesh_active;
                    //handstackToggle(true); //if replay get handsstack from path
                    Vector3 startmenuPosition = Handlist[current_grasp].transform.position;
                    startmenuPosition.y -= 0.05f;
                    StartMenu.transform.position = startmenuPosition;
                }
                StartMenu.SetActive(true);
                ReplayInit = false;
            }
        }
        if (StartMenu.activeSelf == true)
        {
            StartMenu.transform.rotation = Quaternion.Euler(20f, cameraPose.transform.rotation.eulerAngles.y, 0);
        }
        frame_counter += 1;
        if (RecordActive==true || ReplayActive == true)
        {
            HomeMenu.SetActive(false);
        }
        else 
        {
            HomeMenu.SetActive(true);
        }
        visualizeFeedback();

            
        
        var right_hand = HandJointUtils.FindHand(Handedness.Right);
        var left_hand = HandJointUtils.FindHand(Handedness.Left);
        if (right_hand != null)
        {
            if (right_hand.Velocity.magnitude < movingThreshold) bin_right_hand += 1;
            else bin_right_hand = 0;
        }
        if (left_hand != null)
        {
            if (left_hand.Velocity.magnitude < movingThreshold) bin_left_hand += 1;
            else bin_left_hand = 0;
        }
        if (bin_right_hand == threshold)
        {
            myHandedness = Handedness.Right;
            if (recording == true)
            {
                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, myHandedness, out MixedRealityPose pose))
                {
                    //bene und felix 
                    if ((pose.Position - lastRightPosition).magnitude > posDiffThreshold)
                    {
                        save = true;
                        lastRightPosition = pose.Position;
                        
                    } 
                }
            }  
        }
        
        if (replaying == true)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Right, out MixedRealityPose pose))
            {
                //bene und felix 
                if (((pose.Position - Handlist[current_grasp].transform.position).magnitude) < 0.075f && HandednesList[current_grasp] == 1)
                {

                    if (current_grasp == Handlist.Count - 1)
                    {
                        Handlist[current_grasp].GetComponent<MeshRenderer>().material = PassiveMesh_active;
                        current_grasp = 0;
                        frame_count_start = frame_counter;
                        feedback_to_vis = CelebFeedback;
                        replaying = false;
                    }
                    else
                    {
                        Handlist[current_grasp].GetComponent<MeshRenderer>().material = PassiveMesh_active;
                        current_grasp += 1;
                        Handlist[current_grasp].GetComponent<MeshRenderer>().material = ActiveMesh_active;
                    }
                }

            }
        }
        
        if (bin_left_hand == threshold)
        {
            myHandedness = Handedness.Left;
            if (recording == true)
            {
                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, myHandedness, out MixedRealityPose pose))
                {
                    //bene und felix 
                    if ((pose.Position - lastLeftPosition).magnitude > posDiffThreshold)
                    {
                        save = true;
                        lastLeftPosition = pose.Position;

                    }
                }
            }
            
        }
        
        if (replaying == true)
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Left, out MixedRealityPose pose))
            {

                if (((pose.Position - Handlist[current_grasp].transform.position).magnitude) < 0.075f && HandednesList[current_grasp] == 0)
                {
                    if (current_grasp == Handlist.Count - 1)
                    {
                        Handlist[current_grasp].GetComponent<MeshRenderer>().material = PassiveMesh_active;
                        current_grasp = 0;
                        frame_count_start = frame_counter;
                        feedback_to_vis = CelebFeedback;
                        replaying = false;
                    }
                    else
                    {
                        Handlist[current_grasp].GetComponent<MeshRenderer>().material = PassiveMesh_active;
                        current_grasp += 1;
                        Handlist[current_grasp].GetComponent<MeshRenderer>().material = ActiveMesh_active;
                    }

                }

            }
        }
     }
        
    
    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        if (eventData.Command.Keyword == "Save Right")
        {
            save = true;
            myHandedness = Handedness.Right;
        }
        if (eventData.Command.Keyword == "Save Left")
        {
            save = true;
            myHandedness = Handedness.Left;
        }
        if (eventData.Command.Keyword == "Threshold increase")
        {
            threshold = threshold + 10;
        }
        if (eventData.Command.Keyword == "Threshold down")
        {
            threshold = threshold - 10;
        }
        if (eventData.Command.Keyword == "Delete last")
        {

            string IDtoRemove = _createdAnchorIDs[_createdAnchorIDs.Count - 1];
            _createdAnchorIDs.RemoveAt(_createdAnchorIDs.Count - 1);
            meshserializer.Dict.Remove(IDtoRemove);
            GameObject destroy_me = _foundOrCreatedAnchorGameObjects[_foundOrCreatedAnchorGameObjects.Count - 1];
            _foundOrCreatedAnchorGameObjects.RemoveAt(_foundOrCreatedAnchorGameObjects.Count - 1);
            Destroy(destroy_me);
            Count -= 1;

            //GameObject destroyMe = Handlist.Pop();
            //Destroy(destroyMe);
            //To DO
        }
        if (eventData.Command.Keyword == "Start")
        {
            if (RecordActive==true && route_id != 0)
            {
                InitRecordSession();
                recording = true;
                frame_count_start = frame_counter;
                feedback_to_vis = StartRecordFeedback;
            }
            if (ReplayActive == true && route_id != 0)
            {
                StartMenu.SetActive(false);
                frame_count_start = frame_counter;
                feedback_to_vis = GoFeedback;
                replaying = true;
            }

        }
        if (eventData.Command.Keyword == "Pause")
        {

            if (RecordActive==true && route_id!=0)
            {
                frame_count_start = frame_counter;
                feedback_to_vis = PauseFeedback;
                recording = false;
            }
            if (ReplayActive == true && route_id != 0)
            {
                frame_count_start = frame_counter;
                feedback_to_vis = PauseFeedback;
                //handstackToggle(true);
                replaying = false;
            }
        }
        if (eventData.Command.Keyword == "Finish")
        {
            if (ReplayActive == true && route_id != 0)
            {
                ReplayActive = false;
                RouteMenu.SetActive(false);
                StartMenu.SetActive(false);
                replaying = false;
                deleteHandlist();
                EndAzureSession();
            }
            if (RecordActive==true && route_id != 0)
            {
                recording = false;
                RecordActive = false;
                RouteMenu.SetActive(false);
                //string filename = "Route" + route_id.ToString();
                meshserializer.SaveData(filename);
                Count = 0;
                EndAzureSession();
                //handstackToggle(false); //handstack save at path with route_id
            }
            
        }
        if (eventData.Command.Keyword == "Change color")
        {
            changeColorButtonHandler();
        }
    }
    ////////////////////////////////////////////////////////////
    //Spatial Anchors
    ////////////////////////////////////////////////////////////
    private async void InitRecordSession()
    {
        _spatialAnchorManager.StartSessionAsync();
    }

    private async void GraspDetected(Vector3 handPosition, Quaternion handRotation, Mesh MyMesh, GameObject hand, Handedness Myhandedness)
    {

        //await _spatialAnchorManager.StartSessionAsync();    //fix try 1
        await CreateAnchor(handPosition, handRotation, MyMesh, hand, Myhandedness);
    }

    private async Task CreateAnchor(Vector3 handPosition, Quaternion handRotation, Mesh MyMesh, GameObject hand, Handedness myHandedness)
    {
        CloudNativeAnchor cloudNativeAnchor = hand.AddComponent<CloudNativeAnchor>();
        await cloudNativeAnchor.NativeToCloud();
        CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;
        cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(3);

        while (!_spatialAnchorManager.IsReadyForCreate)
        {
            float createProgress = _spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
            Debug.Log($"ASA - Move your device to capture more environment data: {createProgress:0%}");
        }
        Debug.Log($"ASA - Saving cloud anchor... ");

        try
        {
            // Now that the cloud spatial anchor has been prepared, we can try the actual save here.
            await _spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);

            bool saveSucceeded = cloudSpatialAnchor != null;
            if (!saveSucceeded)
            {
                Debug.LogError("ASA - Failed to save, but no exception was thrown.");
                return;
            }

            Debug.Log($"ASA - Saved cloud anchor with ID: {cloudSpatialAnchor.Identifier}");
            _foundOrCreatedAnchorGameObjects.Add(hand);
            _createdAnchorIDs.Add(cloudSpatialAnchor.Identifier);

            int handed = new int();
            if (myHandedness == Handedness.Left)
            {
                handed = 0;
            }
            if (myHandedness == Handedness.Right)
            {
                handed = 1;
            }
            meshserializer.SerializableMeshInfo(MyMesh, cloudSpatialAnchor.Identifier, handed, Count);
            hand.GetComponent<MeshRenderer>().material.color = Color.green;
            Count++;


            // Felix Bene passt bestimmt (nicht gut!)
            //_spatialAnchorManager.DestroySession();
            //await _spatialAnchorManager.StartSessionAsync();

        }
        catch (Exception exception)
        {
            Debug.Log("ASA - Failed to save anchor: " + exception.ToString());
            Debug.LogException(exception);
        }
    }
    private async void EndAzureSession()
    {
        if (_spatialAnchorManager.IsSessionStarted)
        {
            // Stop Session and remove all GameObjects. This does not delete the Anchors in the cloud
            _spatialAnchorManager.DestroySession();
            RemoveAllAnchorGameObjects();
            Debug.Log("ASA - Stopped Session and removed all Anchor Objects");

        }
    }
    private async void Locate()
    {
        Debug.Log("ASA - Session was not started, going forward with locatAnchor");
        await _spatialAnchorManager.StartSessionAsync();
        
        LocateAnchor();
    }
        
    private void RemoveAllAnchorGameObjects()
    {
        foreach (var hand in _foundOrCreatedAnchorGameObjects)
        {
            Destroy(hand);
        }
        _foundOrCreatedAnchorGameObjects.Clear();
    }

    private async void DeleteAnchor(GameObject anchorGameObject)
    {
        CloudNativeAnchor cloudNativeAnchor = anchorGameObject.GetComponent<CloudNativeAnchor>();
        CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;

        Debug.Log($"ASA - Deleting cloud anchor: {cloudSpatialAnchor.Identifier}");

        //Request Deletion of Cloud Anchor
        await _spatialAnchorManager.DeleteAnchorAsync(cloudSpatialAnchor);

        //Remove local references
        _createdAnchorIDs.Remove(cloudSpatialAnchor.Identifier);
        _foundOrCreatedAnchorGameObjects.Remove(anchorGameObject);
        Destroy(anchorGameObject);

        Debug.Log($"ASA - Cloud anchor deleted!");
    }

    private void LocateAnchor()
    {
        CloudMeshDict = meshserializer.GetMesh(filename);
        string[] keys = new string[CloudMeshDict.Keys.Count];
        CloudMeshDict.Keys.CopyTo(keys, 0);
        if (keys.Count() > 0)
        {
            //Create watcher to look for all stored anchor IDs
            //Debug.Log($"ASA - Creating watcher to look for {_createdAnchorIDs.Count} spatial anchors");
            AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
            //CloudMeshDict = meshserializer.GetMesh();
            foreach (var key in keys)
            {
                GameObject Meshobject = new GameObject();
                Handlist.Add(Meshobject);

                //felix und bene waren am werk ;)
                HandednesList.Add(0);
            }
            anchorLocateCriteria.Identifiers = keys;
            _spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);
            Debug.Log($"ASA - Watcher created!");
        }
    }

    private void SpatialAnchorManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        Debug.Log($"ASA - Anchor recognized as a possible anchor {args.Identifier} {args.Status}");

        if (args.Status == LocateAnchorStatus.Located)
        {
            //Creating and adjusting GameObjects have to run on the main thread. We are using the UnityDispatcher to make sure this happens.
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                // Read out Cloud Anchor values
                try
                {
                    CloudSpatialAnchor cloudSpatialAnchor = args.Anchor;
                    GameObject anchorGameObject = new GameObject("anchorGameObject");
                    anchorGameObject.AddComponent<MeshRenderer>();
                    anchorGameObject.AddComponent<MeshFilter>();
                    anchorGameObject.GetComponent<MeshFilter>().mesh = CloudMeshDict[cloudSpatialAnchor.Identifier];
                    Debug.Log(cloudSpatialAnchor.Identifier);
                    anchorGameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                    anchorGameObject.GetComponent<MeshRenderer>().material = ActiveMesh_active;
                    //Create GameObject
                    //anchorGameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                    // Link to Cloud Anchor
                    anchorGameObject.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                    Debug.Log("here");
                    Dictionary<string, List<int>> Metadatadict = meshserializer.GetMetadata(filename);
                    Debug.Log(Metadatadict[cloudSpatialAnchor.Identifier][0]);
                    Debug.Log(Handlist.Count);
                    Handlist[Metadatadict[cloudSpatialAnchor.Identifier][0]] = anchorGameObject;
                    //< Metadatadict[cloudSpatialAnchor.Identifier][0] > = ;
                    //_foundOrCreatedAnchorGameObjects.Add(anchorGameObject);

                    //felix und bene 
                    HandednesList[Metadatadict[cloudSpatialAnchor.Identifier][0]] = Metadatadict[cloudSpatialAnchor.Identifier][1];
                }
                catch (Exception exception)
                {
                    Debug.Log("failed to create hand");
                    Debug.LogException(exception);
                }
            });
        }
    }
}

