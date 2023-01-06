using Microsoft.Azure.SpatialAnchors;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Schema;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Unity.VisualScripting;

/// <summary>
/// Used to serialize and deserialize meshes. In this case hand meshes provided by the grasp detection.
/// Used to store and recall serialized data in and from JSON files.
/// Inputs for serializing:
///     unity mesh object
///     Azure Spatial Anchor ID
///     Handedness (left or right, represented as 0 or 1 integers)
///     ID of Hand (first hand in route gets ID = 0, second gets ID = 1 and so on)
/// Outputs:
///     public dictionary key: Azure Spatial Anchor ID, value: [vertices(float list), triangles(int list), metadata(ID of Hand, Handedness)]
///     
/// Inputs for deserializing:
///     filename of JSON file
/// Outputs:
///     dictionary key: Azure Spatial Anchor ID, value: [unity mesh object]
/// </summary>


[System.Serializable]
class MeshSerializer : MonoBehaviour
{
    [SerializeField]
    public float[] vertices;
    [SerializeField]
    public int[] triangles;
    [SerializeField]
    public float[] uv;
    [SerializeField]
    public float[] uv2;
    [SerializeField]
    public float[] normals;
    [SerializeField]
    public Color[] colors;
    public Dictionary<string, dynamic> Dict = new Dictionary<string, dynamic>();
    private string persistentPath = "";


    //serializes mesh and stores it in a public dict
    public void SerializableMeshInfo(Mesh m, string AnchorID,int myHandedness, int Count)
    {
        vertices = new float[m.vertexCount * 3]; // initialize vertices array.
        for (int i = 0; i < m.vertexCount; i++) // Serialization: Vector3's values are stored sequentially.
        {
            vertices[i * 3] = m.vertices[i].x;
            vertices[i * 3 + 1] = m.vertices[i].y;
            vertices[i * 3 + 2] = m.vertices[i].z;
        }
        triangles = new int[m.triangles.Length]; // initialize triangles array
        for (int i = 0; i < m.triangles.Length; i++) // Mesh's triangles is an array that stores the indices, sequentially, of the vertices that form one face
        {
            triangles[i] = m.triangles[i];
        }
        List<int> metadata_creation = new List<int>();
        metadata_creation.Add(Count);
        metadata_creation.Add(myHandedness);
        List<IList> Meshdata = new List<IList>() { vertices, triangles, metadata_creation };
        Dict.Add(AnchorID, Meshdata);
        }

    //Saves the serialized public dict as a JSON file
    public void SaveData(string filename_savedata)
    {
        SetPaths();
        string savePath = persistentPath + filename_savedata + ".json";

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(Dict);
        Debug.Log(json);

        using StreamWriter writer = new StreamWriter(savePath);
        writer.WriteLine(json);
        Dict.Clear();
    }

    public void SetPaths()
    {
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar;
    }
    
    //converts the Dictionary stored in the JSON file to a dict of Unity Mesh object and returns it
    public Dictionary<string, Mesh> GetMesh(string filename_getMesh)
    {
        
        byte[] bytes = UnityEngine.Windows.File.ReadAllBytes(persistentPath + filename_getMesh + ".json");
        string fileData = System.Text.Encoding.ASCII.GetString(bytes);
        Dictionary<string, List<float[]>> values = JsonConvert.DeserializeObject<Dictionary<string, List<float[]>>>(fileData);
        Dictionary<string,Mesh> CloudMeshDict = new Dictionary<string,Mesh>();
        string[] keys = new string[values.Keys.Count];
        values.Keys.CopyTo(keys, 0);
        foreach (var key in keys)
        {
            List<Vector3> verticesList = new List<Vector3>();
            Mesh m = new Mesh();
            vertices = values[key][0];
            List<int> trianglesintlist = new List<int>();
            foreach (var f in values[key][1])
            {
                int i = new int();
                i = (int)f;
                trianglesintlist.Add(i); 
            }
            triangles = trianglesintlist.ToArray();
            for (int i = 0; i < vertices.Length / 3; i++)
            {
                verticesList.Add(new Vector3(
                        vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]
                    ));
            }
            m.SetVertices(verticesList);
            m.triangles = triangles;
            CloudMeshDict.Add(key, m);
        }
        Debug.Log("end of getMeshfunction");
        return CloudMeshDict;
    }

    // converts the Dictionary stored in the JSON file to a dict of metadata such as ID and handedness
    public Dictionary<string, List<int>> GetMetadata(string filename_getmetadata)
    {
        byte[] bytes = UnityEngine.Windows.File.ReadAllBytes(persistentPath + filename_getmetadata + ".json");
        //string fileData = System.Text.Encoding.ASCII.GetString(jsonfile.bytes);
        string fileData = System.Text.Encoding.ASCII.GetString(bytes);
        Dictionary<string, List<List<float>>> values = JsonConvert.DeserializeObject<Dictionary<string, List<List<float>>>>(fileData);
        Dictionary<string, List<int>> Metadatadict = new Dictionary<string, List<int>>();
        string[] keys = new string[values.Keys.Count];
        values.Keys.CopyTo(keys, 0);
        foreach (var key in keys)
        {
            List<int> metadata = new List<int>();
            foreach (var metavalue in values[key][2])
            {
                float f = metavalue;
                int i = (int)f;
                metadata.Add(i);
            }
            Metadatadict.Add(key, metadata);
        }
        return Metadatadict;
    }
}

