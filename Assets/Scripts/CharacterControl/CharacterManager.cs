using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterManager : MonoBehaviour {


    public CharacterInputController[] ControllableCharacters = {}; 

    public ThirdPersonCamera thirdPersonCamera;

    public string CameraPositionMarkerName = "CamPos";

    // Switches between the fixed overhead view and the follow camera
    public KeyCode cameraToggleKey = KeyCode.C;

    public float followFieldOfView = 60f;


    protected int nextCharacterIndex = 0;

    // Overhead pose, saved at start so it can be restored when the follow camera is turned off
    protected Vector3 overheadPosition;
    protected Quaternion overheadRotation;
    protected float overheadFieldOfView;
    protected Camera cam;

    protected void disableAllCharacters() {

        foreach (CharacterInputController c in ControllableCharacters)
        {
            c.enabled = false;
        }
            
    }

    protected void setCharacter(int charIndex) {

        disableAllCharacters();

        if (charIndex < 0)
            charIndex = 0;

        if (charIndex >= ControllableCharacters.Length)
            charIndex = ControllableCharacters.Length - 1;

        ControllableCharacters[charIndex].enabled = true;

        thirdPersonCamera.desiredPose = ControllableCharacters[charIndex].transform.Find(CameraPositionMarkerName);
        thirdPersonCamera.target = ControllableCharacters[charIndex].transform;

        Debug.Log("Character " + ControllableCharacters[charIndex].Name + " was selected.");
   
    }

    protected void incrementCharacterIndex() {

        ++nextCharacterIndex;

        if (nextCharacterIndex < 0 || nextCharacterIndex >= ControllableCharacters.Length)
            nextCharacterIndex = 0;
    }

    protected void toggleCharacter() {

        setCharacter(nextCharacterIndex);

        incrementCharacterIndex();
    }

    protected void toggleCamera() {

        bool follow = !thirdPersonCamera.enabled;

        thirdPersonCamera.enabled = follow;

        if (follow)
        {
            cam.fieldOfView = followFieldOfView;
        }
        else
        {
            thirdPersonCamera.transform.SetPositionAndRotation(overheadPosition, overheadRotation);
            cam.fieldOfView = overheadFieldOfView;
        }

        Debug.Log("Camera switched to " + (follow ? "follow" : "overhead") + " view.");
    }


	void Start () {
		
        if (thirdPersonCamera == null)
            Debug.LogError("camera must be set");

        cam = thirdPersonCamera.GetComponent<Camera>();
        overheadPosition = thirdPersonCamera.transform.position;
        overheadRotation = thirdPersonCamera.transform.rotation;
        overheadFieldOfView = cam.fieldOfView;

        setCharacter(nextCharacterIndex);
        incrementCharacterIndex();

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.T))
        {
            Debug.Log("Character toggled");
            toggleCharacter();

        }

        if (Input.GetKeyUp(cameraToggleKey))
        {
            toggleCamera();
        }
		
	}
}
