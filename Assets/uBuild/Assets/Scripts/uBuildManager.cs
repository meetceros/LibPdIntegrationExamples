using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//piece variables
[System.Serializable]
public class piece
{
	public GameObject prefab;
	public string name;
	public Sprite image;
	public bool floor;
	public bool disableYSnapping;
	public bool furniture;
	public bool wallpaper;
	//public string description;
	
	//button UI of this piece
	[HideInInspector]
	public GameObject button;
}

//face, to detect floor snapping
public enum face
{
	none, up, down, east, west, north, south
}

public class uBuildManager : MonoBehaviour {
	
	//variables visible in the inspector (under settings)
	//public Color green;
	//public Color red;
	//public Color selected;
	public Color buttonHighlight;
	public GameObject button;
	public GameObject piecesList;
	
	//not visibible
	[HideInInspector]
	public List<piece> pieces;
	
	//variables visible in the inspector (under settings)
	public string buildModeKey;
	public string placeKey;
	
	[TextArea]
	public string helpTextDefault;
	[TextArea]
	public string helpTextPlacing;
	[TextArea]
	public string helpTextSelected;
	
	[Header("Mobile")]
	public Animator mobileButtons;
	public GameObject mobileSelectedButtons;
	public GameObject[] extraButtons;
	
	//not visibible
	private GameObject currentObject;
	private bool isPlacing;
	private int selectedPiece = 0;
	private GameObject helpText;
	private face direction;
	private GameObject furnitureLabel;
	private float lastX;
	
	private int index;
	
	public static bool buildMode;
	public static bool furnitureMode;
	
	[HideInInspector]
	public GameObject pieceSelected;

	public Camera cam;

	private FCP_ExampleScript previousFCPScript;

	public int fluidCount, rubbleCount, slopeCount, speedAreaCount, stairCount, terrainCount, fogCount, grassCount, treeCount, rockCount = 0;

	void Awake()
	{
		//index = GameObject.FindObjectOfType<SaveAndLoad>().index;
		
		foreach(GameObject button in extraButtons)
		{
			button.SetActive(false);
		}
	}
	
	void Start()
	{
		//Find some objects and show the buttons to select pieces with
		//helpText = GameObject.Find("help text");
		//furnitureLabel = GameObject.Find("Furniture label");
		addDefaultButtons();
		
		if(mobileButtons != null)
			mobileButtons.SetBool("build mode", buildMode);

	//	buildModeTrue();

	}
	
	void Update()
	{
		if(Input.GetMouseButtonDown(0))
			lastX = Input.mousePosition.x;
		
		//check if build mode key is pressed, mouse is not over UI and the remove layer warning is not active
		//if(Input.GetKeyDown(buildModeKey) && !EventSystem.current.IsPointerOverGameObject())
		//	switchBuildMode();
		
		//check for buildmode
		if(buildMode)
		{
			//change help text
			//updateHelpText();
		
			//if place key is pressed, instantiate new piece (selected one)
			if(Input.GetKeyDown(placeKey) && !isPlacing && !EventSystem.current.IsPointerOverGameObject())
			{	
				createPiece();
			}
		
			RaycastHit hit;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			//ray from mouse position
			if(Physics.Raycast(ray, out hit))
            
			//check if mouse is over object
				if(hit.collider != null)
				{
					//check if we're placing a piece
					if(isPlacing)
					{
					Vector3 pos = Vector3.zero;	
					Vector3 objectScale = currentObject.GetComponent<PieceTrigger>().scale;
				
					//if piece is floor...
					if(pieces[selectedPiece].floor){
						//get face
						direction = GetHitFace(hit);
					
						//if there's a piece above this floor or underneath it, just move it normally
						if(direction == face.up || direction == face.down)
						{
							pos = new Vector3(hit.point.x, hit.point.y, hit.point.z); // hit.point.y + objectScale.y/2
						}
						else{
							//if object is not rotated, check where the other pieces are to move this floor accordingly
							if(currentObject.transform.rotation.y == 0 || currentObject.transform.rotation.y == -180 || currentObject.transform.rotation.y == 180)
							{
								if(direction == face.north)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(0, 0, objectScale.z/2);
						
								if(direction == face.south)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(0, 0, -objectScale.z/2);
							
								if(direction == face.east)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(objectScale.x/2, 0, 0);
						
								if(direction == face.west)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(-objectScale.x/2, 0, 0);
							}
							else
							{
								//if object is rotated, still check where the other pieces are to move this floor accordingly
								if(direction == face.north)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(0, 0, objectScale.x/2);
						
								if(direction == face.south)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(0, 0, -objectScale.x/2);
							
								if(direction == face.east)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(objectScale.z/2, 0, 0);
						
								if(direction == face.west)
									pos = new Vector3(hit.point.x, hit.point.y, hit.point.z) + new Vector3(-objectScale.z/2, 0, 0);
							}
						}
					}
					else
					{
						//if the current piece is not a floor, move it normally
						pos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					}
				
					//move piece with snapping
					pos -= Vector3.one;
					pos /= 0.5f;
				
					bool isWallpaper = pieces[currentObject.GetComponent<PieceTrigger>().type].wallpaper;
				
					if(!isWallpaper)
					{
						if(!pieces[currentObject.GetComponent<PieceTrigger>().type].disableYSnapping && !pieces[currentObject.GetComponent<PieceTrigger>().type].furniture)
						{
							//normally, use snapping for all directions
							pos = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));
						}
						else if(pieces[currentObject.GetComponent<PieceTrigger>().type].disableYSnapping && !pieces[currentObject.GetComponent<PieceTrigger>().type].furniture)
						{
							//disable y snapping
							pos = new Vector3(Mathf.Round(pos.x), pos.y, Mathf.Round(pos.z));
						}
					}
					else
					{
						if(currentObject.transform.rotation.y % 180 > 0.8f || currentObject.transform.rotation.y % 180 < -0.8f || currentObject.transform.rotation.y == 0)
						{
							pos = new Vector3(pos.x, Mathf.Round(pos.y), Mathf.Round(pos.z));
						}
						else
						{
							pos = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), pos.z);
						}
					}
				
					pos *= 0.5f;
					pos += Vector3.one;
				
					//apply position to current piece
					currentObject.transform.position = pos;
				
					GameObject closestWall = null;
				
					float yDistance = 0;
				
					if(closestWall != null)
						yDistance = Mathf.Abs(closestWall.transform.position.y - currentObject.transform.position.y);
				
					//if currentobject is not triggered by another object, make it green and placeable
					if(!currentObject.GetComponent<PieceTrigger>().triggered && (!isWallpaper || (isWallpaper && closestWall != null && yDistance < 0.1f)))
					{
					//	setRendererSettings(currentObject, null, green);
					
						//place piece on mouse click
						if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && mobileButtons == null)
						{
							StartCoroutine(place());
						}
						else if(Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended && mobileButtons != null)
						{
							if(!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && Input.mousePosition.x > lastX + 70)
							{
									StartCoroutine(place());       
							}
							else
							{
								cancel();
							}
						}
					}
					else
					{
						//make it red when it's not placeable
						//setRendererSettings(currentObject, null, red);
					
						if(Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended && mobileButtons != null)
							cancel();	
					}
				
					//cancel placing when key is pressed
					if(Input.GetKeyDown("delete"))
						cancel();


                        //update piece rotation
                        updateRotation();
					}
				else
				{
					if(mobileButtons == null)
					{
						//if(Input.GetMouseButtonDown(0) && hit.collider.gameObject.CompareTag("Piece") && !EventSystem.current.IsPointerOverGameObject())
						if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == "platform" && !EventSystem.current.IsPointerOverGameObject())
						{
							pieceSelected = hit.collider.gameObject;
							//set piece color to selected color
						//	setRendererSettings(hit.collider.gameObject, null, selected);
						}			
					}
					else if(Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
					{
						//if not placing a piece, use mouse button to select pieces
						//if(hit.collider.gameObject.CompareTag("Piece"))
						if (hit.collider.gameObject.name == "platform")
						{
							pieceSelected = hit.collider.gameObject;
							//set piece color to selected color
							//	setRendererSettings(hit.collider.gameObject, null, selected);
						}
						else
						{
							pieceSelected = null;      
						}
					}
				
					//if there is a selected piece
					if(pieceSelected != null)
					{
						int type = pieceSelected.GetComponent<PieceTrigger>().type;
					
						//check if we want to duplicate the piece
						if(Input.GetKeyDown(KeyCode.LeftControl))
						{
						
							//PlayerPrefs.SetFloat(index + " - " + pieces[type].resource, PlayerPrefs.GetFloat(index + " - " + pieces[type].resource) - pieces[type].resourceAmount);
						
							//instantiate selected piece to duplicate it
							currentObject = Instantiate(pieceSelected, pieceSelected.transform.position, pieceSelected.transform.rotation) as GameObject;
							//disable collider temporarily
							currentObject.GetComponentInChildren<Collider>().enabled = false;
							//make piece untagged
							currentObject.tag = "Untagged";
							//if(currentObject.GetComponent<PieceTrigger>().layer != 0)
							//{
							//	//add the duplicate to it's layer
							//	GetComponent<Layers>().layers[currentObject.GetComponent<PieceTrigger>().layer - 1].layerPieces.Add(currentObject);
							//   }

							//start placing duplicate
							isPlacing = true;
							pieceSelected = null;
					}
		
						//check if we want to move the piece
						if(Input.GetKeyDown("m"))
						{
							//set piece to selected piece to move it
							currentObject = pieceSelected;
							//disable collider temporarily
							currentObject.GetComponentInChildren<Collider>().enabled = false;

							//make piece untagged
							currentObject.tag = "Untagged";
							//start moving piece
							isPlacing = true;
							pieceSelected = null;
						}
						//check if we want to remove piece
						if(Input.GetKeyDown(KeyCode.R)) //if(Input.GetKeyDown("delete"))
							Delete();	
					
						//deselect piece with right mouse button
						if(Input.GetMouseButtonDown(1))
							pieceSelected = null;
					}
				}
			}
		}
		
		if(currentObject != null)
		{
			//give current piece alpha shader to make it transparent
		//	setRendererSettings(currentObject, Shader.Find("Unlit/UnlitAlphaWithFade"), Color.black);
		}
		
		if(mobileButtons != null)
		{
			if(pieceSelected != null)
			{
				mobileSelectedButtons.SetActive(true);
			}
			else
			{
				mobileSelectedButtons.SetActive(false);
			}
		}
	}
	
	public void Delete()
	{
		int type = pieceSelected.GetComponent<PieceTrigger>().type;
		//PlayerPrefs.SetFloat(index + " - " + pieces[type].resource, PlayerPrefs.GetFloat(index + " - " + pieces[type].resource) + pieces[type].resourceAmount);
						
		//set piece to selected piece
		currentObject = pieceSelected;
		//remove piece from it's layer
		if(currentObject.GetComponent<PieceTrigger>().layer != 0)
		{
			GetComponent<Layers>().layers[currentObject.GetComponent<PieceTrigger>().layer - 1].layerPieces.Remove(currentObject);
		}
		
		//destroy current piece and set selected piece to null
		Destroy(currentObject);
		pieceSelected = null;
	}
	
	void createPiece()
	{
		currentObject = Instantiate(pieces[selectedPiece].prefab, Vector3.zero, Quaternion.identity) as GameObject;
		
		//PlayerPrefs.SetFloat(index + " - " + pieces[selectedPiece].resource, PlayerPrefs.GetFloat(index + " - " + pieces[selectedPiece].resource) - pieces[selectedPiece].resourceAmount);
		
		//disable collider (temporarily) and set type and layer
		currentObject.GetComponentInChildren<Collider>().enabled = false;
		currentObject.GetComponent<PieceTrigger>().type = selectedPiece;
		//currentObject.GetComponent<PieceTrigger>().layer = PlayerPrefs.GetInt(index + " - " + "defaultLayer");
			
		//add piece to the correct layer
		//if(PlayerPrefs.GetInt(index + " - " + "defaultLayer") != 0)
		//{
		//	GetComponent<Layers>().layers[PlayerPrefs.GetInt(index + " - " + "defaultLayer") - 1].layerPieces.Add(currentObject);
		//}
			
		//start placing
		isPlacing = true;
		pieceSelected = null;
	}
	
	void updateRotation()
	{
		//if right mouse button gets pressed, rotate the piece 90 degrees
		//please do not change the rotation angle, this will disturb floor placement
		if((mobileButtons != null && Input.GetMouseButtonDown(0) && Input.touchCount > 1) || Input.GetMouseButtonDown(1))
		{
			currentObject.transform.Rotate(Vector3.up, 90, Space.World);
		}
	}
	
	public void buildModeTrue()
	{
		//set build mode true
		buildMode = true;
		
		//show build mode UI
		GameObject.Find("Pieces list").GetComponent<Animator>().SetTrigger("Slide in");
		//GameObject.Find("Help").GetComponent<Animator>().SetTrigger("Slide in");
		
		//GameObject.Find("Layers panel").GetComponent<Animator>().SetTrigger("Slide in");
		//GameObject.Find("Layers buttons").GetComponent<Animator>().SetTrigger("Slide in");
		
		if(GameObject.FindObjectOfType<MobileMoveZoom>())
			GameObject.FindObjectOfType<MobileMoveZoom>().gameObject.GetComponent<Animator>().SetTrigger("slide in");
		
		if(GameObject.FindObjectOfType<MobileRotate>())
			GameObject.FindObjectOfType<MobileRotate>().gameObject.GetComponent<Animator>().SetTrigger("slide in");
		
		//for each layer...
		//for(int i = 0; i < GetComponent<Layers>().layers.Count; i++){
		//	//show pieces of this layer if layer is active
		//	if(PlayerPrefs.GetInt(index + " - " + "layer" + i) == 0){
		//		foreach(GameObject piece in GetComponent<Layers>().layers[i].layerPieces){
		//		piece.SetActive(true);
		//		}
		//	}
		//	//do not show pieces of this layer when layer isn't active
		//	else{
		//		foreach(GameObject piece in GetComponent<Layers>().layers[i].layerPieces){
		//		piece.SetActive(false);
		//		}
		//	}
		//}
		
		foreach(GameObject button in extraButtons)
		{
			button.SetActive(true);
		}
	}
	
	public void buildModeFalse()
	{
		//set build mode false
		buildMode = false;
		
		//don't show build mode UI
		GameObject.Find("Pieces list").GetComponent<Animator>().SetTrigger("Slide out");
		//GameObject.Find("Help").GetComponent<Animator>().SetTrigger("Slide out");
		
		//GameObject.Find("Layers panel").GetComponent<Animator>().SetTrigger("Slide out");
		//GameObject.Find("Layers buttons").GetComponent<Animator>().SetTrigger("Slide out");
		
		if(GameObject.FindObjectOfType<MobileMoveZoom>())
			GameObject.FindObjectOfType<MobileMoveZoom>().gameObject.GetComponent<Animator>().SetTrigger("slide out");
		
		if(GameObject.FindObjectOfType<MobileRotate>())
			GameObject.FindObjectOfType<MobileRotate>().gameObject.GetComponent<Animator>().SetTrigger("slide out");
				
		//if(GameObject.Find("Layers panel").GetComponent<RectTransform>().anchoredPosition.x < 0){
		//	GameObject.Find("Layers panel").GetComponent<Animator>().SetTrigger("Slide out");
		//	GameObject.Find("Layers buttons").GetComponent<Animator>().SetTrigger("Slide out");
		//}
		
		//set all pieces of all layers active
		//for(int i = 0; i < GetComponent<Layers>().layers.Count; i++){
		//	foreach(GameObject piece in GetComponent<Layers>().layers[i].layerPieces){
		//	piece.SetActive(true);
		//	}
		//}
		
		foreach(GameObject button in extraButtons)
		{
			button.SetActive(false);
		}
	}
	
	public void switchBuildMode()
	{
		//check if build mode key is pressed, mouse is not over UI and the remove layer warning is not active
		//if(!Layers.removeLayerWarning.activeSelf && GameObject.FindObjectOfType<SaveAndLoad>().doneLoading){
		//	//change buildmode
		//	if(!buildMode){
		//		buildModeTrue();
		//	}
		//	else{
		//		buildModeFalse();
		//	}
		//}

		if (!buildMode)
		{
			buildModeTrue();
		}
		else
		{
			buildModeFalse();
		}

		if (mobileButtons != null)
			mobileButtons.SetBool("build mode", buildMode);
	}
	
	IEnumerator place()
	{
		// 충돌 감지를 위한 변수
		float detectionRadius = 0.5f; // 감지 반경 조정 가능
		LayerMask playerLayer = LayerMask.GetMask("player"); // Player 레이어 설정

		// 현재 위치에서 플레이어와의 충돌 확인
		Collider[] colliders = Physics.OverlapSphere(currentObject.transform.position, detectionRadius, playerLayer);
		if (colliders.Length > 0)
		{
			Debug.Log("Cannot place object here. Colliding with player.");
			cancel(); // 충돌 시 배치 취소
			yield break;
		}

		//set piece color to white
		// setRendererSettings(currentObject, null, Color.white);
		//enable piece collider
		currentObject.GetComponentInChildren<Collider>().enabled = true;
		
		//we're not placing anything anymore
		isPlacing = false;
		//wait a very small time
		yield return new WaitForSeconds(0.05f);
		//set piece tag to piece (also important to save it)
		// currentObject.tag = "Piece";

		currentObject.name = "platform";

		currentObject = null;
		// Debug.Log("platform!!");
		// Debug.Log(pieces[selectedPiece].prefab.name);

		if(pieces[selectedPiece].prefab.name == "PlatformFluid")
		{
			fluidCount++;
			// Debug.Log("PlatformFluid " + fluidCount);
		}
		if(pieces[selectedPiece].prefab.name == "PlatformRubble")
		{
			rubbleCount++;
		}
		if(pieces[selectedPiece].prefab.name == "PlatformSlope")
		{
			slopeCount++;
		}
		if(pieces[selectedPiece].prefab.name == "PlatformSpeedArea")
		{
			speedAreaCount++;
		}
		if(pieces[selectedPiece].prefab.name == "PlatformStairs")
		{
			stairCount++;
			// Debug.Log("PlatformStairs " + stairCount);
		}
		if(pieces[selectedPiece].prefab.name == "PlatformTerrain")
		{
			terrainCount++;
		}
		if(pieces[selectedPiece].prefab.name == "ObjFog")
		{
			fogCount++;
		}
		if(pieces[selectedPiece].prefab.name == "ObjGrass")
		{
			grassCount++;
		}
		if(pieces[selectedPiece].prefab.name == "ObjTree")
		{
			treeCount++;
		}
		if (pieces[selectedPiece].prefab.name == "ObjRock")
		{
			rockCount++;
		}
	}
	
	void cancel()
	{
		//remove piece from it's layer
		if(currentObject.GetComponent<PieceTrigger>().layer != 0)
		{
			GetComponent<Layers>().layers[currentObject.GetComponent<PieceTrigger>().layer - 1].layerPieces.Remove(currentObject);
		}
		//destroy piece
		Destroy(currentObject);
		//stop placing
		isPlacing = false;
		
		//PlayerPrefs.SetFloat(index + " - " + pieces[selectedPiece].resource, PlayerPrefs.GetFloat(index + " - " + pieces[selectedPiece].resource) + pieces[selectedPiece].resourceAmount);
	}
	
	void addDefaultButtons()
	{
		//for all piece types...
		for(int i = 0; i < pieces.Count; i++)
		{
			//add a button to the list of buttons
			GameObject newButton = Instantiate(button);
			RectTransform rectTransform = newButton.GetComponent<RectTransform>();
			rectTransform.SetParent(piecesList.transform, false);
			
			//set button outline
			newButton.GetComponent<Outline>().effectColor = buttonHighlight;
			
			//set the correct button sprite
			newButton.GetComponent<Image>().sprite = pieces[i].image;
			
			//only enable outline for the first button
			if(i == 0)
			{
				newButton.GetComponent<Outline>().enabled = true;
			}
			else
			{
				newButton.GetComponent<Outline>().enabled = false;	
			}
			
			//set button name to its position in the list(important for the button to work later on)
			newButton.name = "" + i;
			
			//show piece name
			newButton.GetComponentInChildren<Text>().text = "" + pieces[i].name;
			
			//this is the new button ui
			pieces[i].button = newButton;
		}
	}
	
	//update the help text
	void updateHelpText()
	{
		////show normal help text
		//if(pieceSelected == null && !isPlacing){
		//	helpText.GetComponent<Text>().text = "" + helpTextDefault;
		//}
		////show help text for placing a piece
		//if(isPlacing){
		//	helpText.GetComponent<Text>().text = "" + helpTextPlacing;
		//}
		////show help text for selected piece
		//if(pieceSelected != null){
		//	helpText.GetComponent<Text>().text = "" + helpTextSelected;
		//}
	}
	
	//return faces that were hit to place floors
	static face GetHitFace(RaycastHit hit)
	{
		Vector3 incoming = hit.normal - Vector3.up;
		//south
		if(incoming == new Vector3(0, -1, -1))
			return face.south;
		//north
		if(incoming == new Vector3(0, -1, 1))
			return face.north;
		//up
		if(incoming == new Vector3(0, 0, 0))
			return face.up;
		//down
		if(incoming == new Vector3(1, 1, 1))
			return face.down;
		//west
		if(incoming == new Vector3(-1, -1, 0))
			return face.west;
		//east
		if(incoming == new Vector3(1, -1, 0))
			return face.east;
		//no face
		return face.none;
	}	
	
	void selectFirstPiece()
	{
		//first button that is currently active
		int firstActiveButton = 0;
		for(int i = 0; i < pieces.Count; i++)
		{
			if(pieces[i].button.gameObject.activeSelf)
			{
				//if this is the active button, set it to be the active one
				firstActiveButton = i;
				//break the loop since we've already got the active button
				break;
			}
		}
		//disable button outline...
		for(int i = 0; i < pieces.Count; i++)
		{
			pieces[i].button.GetComponent<Outline>().enabled = false;	
		}
		//... and enable it for the first button
		pieces[firstActiveButton].button.gameObject.GetComponent<Outline>().enabled = true;
		//set the first active button to be the selected button
		selectedPiece = firstActiveButton;
	}
	
	//select a new piece
	public void selectPiece(int piece)
	{
		//disable all button outlines...
		for(int i = 0; i < pieces.Count; i++)
		{
			pieces[i].button.GetComponent<Outline>().enabled = false;	
		}
		
		//... and enable it for the selected button
		pieces[piece].button.GetComponent<Outline>().enabled = true;
		//set selected piece to piece
		selectedPiece = piece;
		
		if(mobileButtons != null && !isPlacing)
			createPiece();
	}
	
	void setRendererSettings(GameObject piece, Shader shader, Color color)
	{
		foreach(Renderer renderer in piece.GetComponentsInChildren<Renderer>())
		{
			if(shader != null && color != Color.black)
			{
				renderer.material.color = color;
				renderer.material.shader = shader;
			}
			else if(shader != null)
			{
				renderer.material.shader = shader;
			}
			else
			{
				renderer.material.color = color;
			}
		}
	}
}
