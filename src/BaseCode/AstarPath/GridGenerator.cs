using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[AddComponentMenu("")]
public class GridGenerator : MonoBehaviour 
{
	public GameObject whiteCube;
	public GameObject blackCube;
	public GameObject pathObject;
	public MeshRenderer gridPlane;
	public string outFileName = "";
	public string loadFileName = "";
	// Use this for initialization
	bool bStart = false;
	bool bRaying = false;
	bool bRayEnd = false;
	Terrain terrain;

	private AStarUtils aStarUtils;

	private AStarNode beginNode;

	private int cols = 0;
	private int rows = 0;
	Texture2D textureGrid;
	private IList<GameObject> pathList;

	void Start () 
	{
		pathList = new List<GameObject> ();
	}

	// Update is called once per frame
	void Update () 
	{
		if (bStart == true) 
		{
			if (bRaying == false) 
			{
				bRaying = true;
				StartCoroutine (GenerateGrid ());
			}
			if(bRayEnd == true)
			{
				bStart = false;
			}
		}
		if(bRayEnd == true)
			UpdatePath ();
	}

	bool InitPathData()
	{
		GameObject terrainObj = GameObject.Find("Terrain");
		terrain = terrainObj.GetComponent<Terrain>();
		if (terrain == null)
			return false;
		float tWidth = terrain.terrainData.size.x;
		float tLength = terrain.terrainData.size.z;

		//-----------
		GameObject gridPlane = GameObject.Find ("GridPlane");
		gridPlane.transform.localScale = new Vector3(tWidth*0.1f,1f,tLength*0.1f);
		gridPlane.transform.localPosition = new Vector3(tWidth*0.5f,0f,tLength*0.5f);

		//-----------
		Camera mainCamera = Camera.main;
		mainCamera.orthographicSize = Mathf.Max (tWidth*0.5f, tLength*0.5f);
		mainCamera.transform.position = new Vector3 (tWidth * 0.5f, 100f, tLength * 0.5f);
		//-----------
		cols = (int)(tWidth*(1f/MapVO.s_NodeSize));
		rows = (int)(tLength*(1f/MapVO.s_NodeSize));
		//----------
		if (outFileName == "") 
		{
			GameObject map = GameObject.FindWithTag ("Map");
			if (map) {
				outFileName = map.name;
				loadFileName = map.name;
			} else {
				MyLogger.LogError ("Not Set Map");		
				return false;
			}
		}
		return true;
	}

	public void StartRay()
	{
		if (bStart == false) 
		{
			if (InitPathData () == true) 
			{
				aStarUtils = new AStarUtils(cols,rows);
				textureGrid = new Texture2D (cols, rows);
				bStart = true;
				bRaying = false;
				bRayEnd = false;
			}
		}
	}

	IEnumerator GenerateGrid()
	{
		Debug.Log ("GenerateGrid Begin!!!!");
		Vector3 rayPoint = Vector3.zero;
		int index = 0;
		int x, z;
		bool bPassable;
		for(x = 0;x<cols;x++)
		{
			for(z = 0;z<rows;z++)
			{
				rayPoint =  MapVO.GetWorldPos(x,z);
				bPassable = RaycatMap(rayPoint);
				textureGrid.SetPixel (x, z, bPassable ? Color.white : Color.black);
				aStarUtils.GetNode (x, z).walkable = bPassable;
				index++;
				if(index%50 == 0)
					yield return 1;
			}
		}
		textureGrid.Apply ();
		gridPlane.material.mainTexture = textureGrid;
		bRayEnd = true;
		Debug.Log ("GenerateGrid End!!!!");
		yield return bRayEnd;
	}

	bool RaycatMap(Vector3 point)
	{
		RaycastHit hit;  
		Vector3 rayPoint = point;
		Vector3 normalAngle;
		rayPoint.y = 100f;
		if (Physics.Raycast(rayPoint,Vector3.down,out hit,200f,MapVO.s_layerMasks.value))
		{
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer (MapVO.buildinglayer))
				return false;
			else if (hit.collider.gameObject.layer == LayerMask.NameToLayer (MapVO.goundlayer)) 
			{
				normalAngle = Quaternion.FromToRotation (Vector3.up, hit.normal).eulerAngles;
//				Debug.Log (normalAngle);
				if (normalAngle.z >= 60f && normalAngle.z <= 180f)
					return false;
				if (normalAngle.z >= 180f && normalAngle.z <= 300f)
					return false;
				if (normalAngle.x >= 60f && normalAngle.x <= 180f)
					return false;
				if (normalAngle.x >= 180f && normalAngle.x <= 300f)
					return false;
			}				
			Debug.DrawLine(rayPoint,hit.point);
			return true;
		}  
		return false;
	}

	public void LoadFromJPG()
	{
		if (InitPathData () == false)
			return;
		GameObject map = GameObject.FindWithTag ("Map");
		if (map) {
			textureGrid = PathData.LoadPathJPG (loadFileName);
			gridPlane.material.mainTexture = textureGrid;
			aStarUtils = new AStarUtils(textureGrid.width,textureGrid.height);
			int x, z;
			bool bPassable;
			for(x = 0;x<cols;x++)
			{
				for(z = 0;z<rows;z++)
				{
					bPassable = textureGrid.GetPixel (x, z) == Color.white ? true : false;
					aStarUtils.GetNode (x, z).walkable = bPassable;
				}
			}
			bRayEnd = true;
		}
	}

	public void LoadFromPD()
	{
		if (InitPathData () == false)
			return;
		GameObject map = GameObject.FindWithTag ("Map");
		if (map) {
			byte[] bytes = PathData.LoadPathData(loadFileName);
			ByteArray.StartRead (bytes);
			int width = ByteArray.ReadInt();
			int height = ByteArray.ReadInt();
			textureGrid = new Texture2D (width, height,TextureFormat.ARGB32,false);	
			aStarUtils = new AStarUtils(textureGrid.width,textureGrid.height);
			int x, z;
			bool bPassable;
			for(x = 0;x<textureGrid.width;x++)
			{
				for(z = 0;z<textureGrid.height;z++)
				{
					bPassable = ByteArray.ReadByte() == 0 ? true : false;
					textureGrid.SetPixel (x, z, bPassable ? Color.white : Color.black);
					aStarUtils.GetNode (x, z).walkable = bPassable;
				}
			}
			textureGrid.Apply ();
			gridPlane.material.mainTexture = textureGrid;
			bRayEnd = true;
			Debug.Log (width + "-" + height);
		}
	}

	public void SavePathUtils()
	{
		if (textureGrid == null)
			return;
		PathData.SavePathJPG (outFileName, textureGrid);
		PathData.SavePathData (outFileName, textureGrid);
	}

	void UpdatePath()
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit raycastHit = new RaycastHit();
			if(Physics.Raycast(ray, out raycastHit))
			{
				if(raycastHit.collider.gameObject.tag == "GridPlane")
				{
					Vector2 pointItem = MapVO.GetNodeXY(raycastHit.point);
					this.FindPath((int)pointItem.x, (int)pointItem.y);
				}
			}
		}
	}


	private void FindPath(int x, int y)
	{
		AStarNode endNode = this.aStarUtils.GetNode(x, y);

		if (this.beginNode == null) 
		{
			this.beginNode = endNode;
			return;
		}

		if (pathList != null && pathList.Count > 0) 
		{
			foreach (GameObject xxObject in pathList) 
			{
				Destroy(xxObject);
			}
		}

		if(endNode != null && endNode.walkable)
		{
			System.DateTime dateTime = System.DateTime.Now;

			IList<AStarNode> pathNodes = this.aStarUtils.FindPath(this.beginNode, endNode);

			System.DateTime currentTime = System.DateTime.Now;

			System.TimeSpan timeSpan = currentTime.Subtract(dateTime);

			Debug.Log(timeSpan.Seconds + "秒" + timeSpan.Milliseconds + "毫秒");

			if(pathNodes != null && pathNodes.Count > 0)
			{
				foreach(AStarNode nodeItem in pathNodes)
				{
					GameObject gameObject = (GameObject)Instantiate(this.pathObject);
					pathList.Add(gameObject);
					float offsetX = nodeItem.nodeX * MapVO.s_NodeSize + MapVO.s_NodeSize;
					float offsetZ = nodeItem.nodeY * MapVO.s_NodeSize + MapVO.s_NodeSize;
					gameObject.transform.localPosition = new Vector3(offsetX, 40f, offsetZ);
				}
			}
			this.beginNode = endNode;
		}
	}
}
