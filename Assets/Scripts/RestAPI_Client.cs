using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class RestAPI_Client : MonoBehaviour
{
    public GameObject profilePicture;
    public GameObject textObject;
    public TextMeshPro messageText;

    //public readonly string serverAddress = "http://localhost:5000";
    public readonly string serverAddress = "http://192.168.0.199:5000";

    public ImageFromURL imageFromURL;
    private GraphPanelScript graphPanel;

    private int currentProfile = 1000;

    // Start is called before the first frame update
    void Start()
    {
        profilePicture.SetActive(false);
        messageText = textObject.GetComponent<TextMeshPro>();
        messageText.text = "Press buttons to interact with web server";

        imageFromURL = this.GetComponent<ImageFromURL>();
        graphPanel = this.GetComponent<GraphPanelScript>();
    }

    public void OnButtonGetPerson(int number)
    {
        messageText.text = "Getting person";
        profilePicture.SetActive(false);
        StartCoroutine(PersonGetRequest(number));
    }

    IEnumerator PersonGetRequest(int number)
    {
        string getURL = serverAddress + "/person/" + number.ToString();
        UnityWebRequest www = UnityWebRequest.Get(getURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            messageText.text = www.error;
        }
        else
        {
            updateTextFromJSON(www.downloadHandler.text);
        }
    }

    public void OnButtonGetNextPerson()
    {
        messageText.text = "Getting person";
        profilePicture.SetActive(false);
        StartCoroutine(PersonGetNextRequest());
    }

    IEnumerator PersonGetNextRequest()
    {
        string getURL = serverAddress + "/person/" + currentProfile + "/next";
        UnityWebRequest www = UnityWebRequest.Get(getURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            messageText.text = www.error;
        }
        else
        {
            updateTextFromJSON(www.downloadHandler.text);
        }
    }

    public void updateTextFromJSON(string jsonString)
    {
        Person person = JsonUtility.FromJson<Person>(jsonString);
        messageText.text = "ID: " + person.id + "\n";
        messageText.text += "First Name: " + person.first_name + "\n";
        messageText.text += "Last Name: " + person.last_name + "\n";
        messageText.text += "Gender: " + person.gender + "\n";
        messageText.text += "Date of Birth: " + person.date_of_birth + "\n";
        messageText.text += "E-mail: " + person.email + "\n";

        imageFromURL.getImage(person.image_file, serverAddress);
        profilePicture.SetActive(true);
        currentProfile = person.id;
    }

    IEnumerator PersonsGetRequest()
    {
        string getURL = serverAddress + "/person";
        UnityWebRequest www = UnityWebRequest.Get(getURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            messageText.text = www.downloadHandler.text;
            //list of persons needs to have a referance name in json file
            Persons items = JsonUtility.FromJson<Persons>("{\"persons\":" + messageText.text + "}");
            Debug.Log(items.persons[0].date_of_birth);
        }
    }

    public void enableGraph()
    {
        graphPanel.toggleEnableGraph(serverAddress);
    }
}

[System.Serializable]
public class Person
{
    public int id;
    public string first_name;
    public string last_name;
    public string gender;
    public string date_of_birth;
    public string email;
    public string image_file;
}

[System.Serializable]
public class Persons
{
    public Person[] persons;
}