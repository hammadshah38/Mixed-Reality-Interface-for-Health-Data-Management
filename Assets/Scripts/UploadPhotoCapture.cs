using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Windows.WebCam;
using TMPro;
using UnityEngine.Networking;

public class UploadPhotoCapture : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;

    public GameObject debugTextObject;
    private TextMeshPro debugText;
    private string serverAddress;
    public RestAPI_Client rest;

    void Start()
    {
        rest = this.GetComponent<RestAPI_Client>();
        serverAddress = rest.serverAddress;
    }

    public void TakePhoto()
    {
        if (photoCaptureObject != null)
        {
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode); // Clean up in case of previous failure
        }
        debugText = debugTextObject.GetComponent<TextMeshPro>();
        debugText.text = "DebugText: Start";
        rest.messageText.text = "Start taking photo";
        rest.profilePicture.SetActive(false);
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        debugText.text = "OnPhotoCaptureCreated";
        photoCaptureObject = captureObject;

        //Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        //Resolution cameraResolution = PhotoCapture.SupportedResolutions.Where((res) => (res.width == 1920) & (res.height == 1080)).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        //c.cameraResolutionWidth = cameraResolution.width;
        //c.cameraResolutionHeight = cameraResolution.height;
        c.cameraResolutionWidth = 1920;
        c.cameraResolutionHeight = 1080;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            debugText.text = "Saved";
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
            debugText.text = "Failed";
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Create our Texture2D for use and set the correct resolution
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            // Do as we wish with the texture such as apply it to a material, etc.
            StartCoroutine(UploadWebcamPhoto(targetTexture));
        }
        // Clean up
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    IEnumerator UploadWebcamPhoto(Texture2D targetTexture)
    {
        rest.messageText.text = "Sending image";
        // Encode texture into PNG
        byte[] bytes = targetTexture.EncodeToPNG();
        Destroy(targetTexture);

        // Create a Web Form
        WWWForm form = new WWWForm();
        // Add image to form
        form.AddBinaryData("file", bytes, "WebcamPhoto.png", "image/png");
        // Create 
        string screenShotURL = serverAddress + "/uploader";
        UnityWebRequest www = UnityWebRequest.Post(screenShotURL, form);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
            rest.messageText.text = www.error;
        }
        else
        {
            string str = www.downloadHandler.text;
            if (str.Substring(0, 1) != "E") rest.updateTextFromJSON(str);
            else rest.messageText.text = str;
        }
    }
}