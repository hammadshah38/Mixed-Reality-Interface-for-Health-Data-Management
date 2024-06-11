using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GraphPanelScript : MonoBehaviour
{
    public GameObject GraphPanelObject;
    public GameObject MainPanelObject;
    public GameObject CameraObject;
    private Renderer thisRenderer;

    void Start()
    {
        GraphPanelObject.SetActive(false);
        thisRenderer = GraphPanelObject.transform.Find("Plane").gameObject.GetComponent<MeshRenderer>();
    }

    public void toggleEnableGraph(string serverAddress)
    {
        if(GraphPanelObject.activeInHierarchy)
        {
            disableGraph();
        }
        else
        {
            enableGraph(serverAddress);
        }
    }
    public void disableGraph()
    {
        GraphPanelObject.SetActive(false);
    }

    public void enableGraph(string serverAddress)
    {
        Vector3 direction = MainPanelObject.transform.position - CameraObject.transform.position;
        direction.y = 0;      
        float posX = MainPanelObject.transform.position.x;
        float posY = 0.2f;
        float posZ = MainPanelObject.transform.position.z;
        GraphPanelObject.transform.position = new Vector3(posX, posY, posZ) + direction;
        GraphPanelObject.transform.rotation = Quaternion.LookRotation(direction);
        GraphPanelObject.SetActive(true);
        StartCoroutine(GetGraphURL(serverAddress));
    }

    public void rotatePanel()
    {
        Vector3 direction = GraphPanelObject.transform.position - CameraObject.transform.position;
        GraphPanelObject.transform.rotation = Quaternion.LookRotation(direction);
    }

    IEnumerator GetGraphURL(string serverAddress) {
        string getURL = serverAddress + "/get_graph";
        UnityWebRequest www = UnityWebRequest.Get(getURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string image_url = www.downloadHandler.text;

            getImage(image_url,serverAddress);
        }
    }

    private void getImage(string imagefile, string serverAddress){
        StartCoroutine(GetTexture(imagefile, serverAddress));
    }
 
    IEnumerator GetTexture(string imagefile, string serverAddress) {
        string image_url = serverAddress + "/" + imagefile;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(image_url);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            thisRenderer.material.mainTexture = myTexture;
        }
    } 
}
