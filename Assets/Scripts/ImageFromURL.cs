using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class ImageFromURL : MonoBehaviour
{
    //private string url = "http://127.0.0.1:5000/static/images/profile_pics/";

    public Renderer thisRenderer;

    // Start is called before the first frame update
    void Start() {
        //StartCoroutine(GetTexture());
    }

    public void getImage(string imagefile, string serverAddress){
        StartCoroutine(GetTexture(imagefile, serverAddress));
    }
 
    IEnumerator GetTexture(string imagefile, string serverAddress) {
        string image_url = serverAddress + "/static/images/profile_pics/" + imagefile;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(image_url);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            thisRenderer.material.color = Color.white;
            thisRenderer.material.mainTexture = myTexture;
        }
    }

}
