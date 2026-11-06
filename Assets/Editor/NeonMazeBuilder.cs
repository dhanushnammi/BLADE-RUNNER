using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

[InitializeOnLoad]
public static class NeonMazeBuilder
{
    const int W = 25, H = 25;
    static NeonMazeBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying && !System.IO.File.Exists("Assets/Scenes/NeonMaze.unity"))
            {
                // Scene is generated from the menu to avoid modifying projects unexpectedly.
            }
        };
    }

    [MenuItem("Neon Maze Runner/Generate Playable Maze Scene")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("NEON MAZE WORLD");
        var grid = GenerateMaze(W,H);
        Material floorMat = Mat(new Color(.025f,.035f,.09f), .25f, .7f);
        Material wallMat = Mat(new Color(.02f,.75f,1f), .35f, 1f);
        Material trimMat = Mat(new Color(1f,.05f,.65f), .3f, 1f);
        Material coinMat = Mat(new Color(1f,.78f,.05f), .25f, 1f);
        Material exitMat = Mat(new Color(.1f,1f,.35f), .3f, 1f);

        MakeCube("Arena Floor", new Vector3(W/2f-.5f,-.25f,H/2f-.5f), new Vector3(W,.5f,H), floorMat, root.transform);
        for(int z=0;z<H;z++) for(int x=0;x<W;x++)
        {
            if(grid[z,x]==1)
            {
                MakeCube("Neon Wall", new Vector3(x,1.15f,z), new Vector3(.96f,2.3f,.96f), wallMat, root.transform);
                MakeCube("Pink Wall Stripe", new Vector3(x,2.27f,z), new Vector3(.98f,.06f,.98f), trimMat, root.transform);
            }
        }

        // Outer boundary
        for(int x=-1;x<=W;x++){ MakeCube("Boundary",new Vector3(x,1.2f,-1),Vector3.one*1.1f,trimMat,root.transform); MakeCube("Boundary",new Vector3(x,1.2f,H),Vector3.one*1.1f,trimMat,root.transform);}
        for(int z=0;z<H;z++){ MakeCube("Boundary",new Vector3(-1,1.2f,z),Vector3.one*1.1f,trimMat,root.transform); MakeCube("Boundary",new Vector3(W,1.2f,z),Vector3.one*1.1f,trimMat,root.transform);}

        var player = new GameObject("Runner");
        player.transform.position = new Vector3(1,0.15f,1);
        var cc = player.AddComponent<CharacterController>(); cc.height=1.8f; cc.radius=.32f; cc.center=new Vector3(0,.9f,0);
        player.AddComponent<NeonPlayer>();
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name="Runner Visual"; body.transform.SetParent(player.transform); body.transform.localPosition=new Vector3(0,.9f,0); body.transform.localScale=new Vector3(.55f,.85f,.55f);
        Object.DestroyImmediate(body.GetComponent<Collider>()); body.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.2f,.95f,1f),.2f,1f);
        var camObj = new GameObject("Player Camera"); camObj.tag="MainCamera";
        camObj.transform.SetParent(player.transform); camObj.transform.localPosition=new Vector3(0,1.55f,0);
        var cam=camObj.AddComponent<Camera>(); cam.fieldOfView=78; camObj.AddComponent<AudioListener>();
        var pp=player.GetComponent<NeonPlayer>(); pp.cameraPivot=camObj.transform;

        List<GameObject> coins=new List<GameObject>();
        for(int z=1;z<H-1;z++) for(int x=1;x<W-1;x++)
        {
            if(grid[z,x]==0 && !(x==1&&z==1) && (x*7+z*13)%17==0)
            {
                var c=GameObject.CreatePrimitive(PrimitiveType.Cylinder); c.name="Energy Core";
                c.transform.position=new Vector3(x,.55f,z); c.transform.localScale=new Vector3(.48f,.12f,.48f);
                c.GetComponent<Renderer>().sharedMaterial=coinMat;
                c.GetComponent<Collider>().isTrigger=true; c.AddComponent<NeonCoin>(); coins.Add(c);
            }
        }
        // Exit at far corner, with a visible green beacon
        var exit=GameObject.CreatePrimitive(PrimitiveType.Cube); exit.name="EXIT PORTAL";
        exit.transform.position=new Vector3(W-2,1.2f,H-2); exit.transform.localScale=new Vector3(1.4f,2.4f,.25f);
        exit.GetComponent<Renderer>().sharedMaterial=exitMat;
        var trigger=exit.AddComponent<BoxCollider>(); trigger.isTrigger=true;
        exit.AddComponent<NeonExit>();

        // Ambient lighting and fog
        RenderSettings.ambientLight=new Color(.25f,.28f,.42f);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.015f,.02f,.07f); RenderSettings.fogMode=FogMode.Exponential; RenderSettings.fogDensity=.018f;
        var lightObj=new GameObject("Neon Key Light"); var light=lightObj.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=.65f; light.color=new Color(.55f,.7f,1f); lightObj.transform.rotation=Quaternion.Euler(50,-30,0);

        BuildUI(coins.Count);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/NeonMaze.unity");
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Neon Maze Runner","Scene generated. Open Assets/Scenes/NeonMaze.unity and press Play.","OK");
    }

    static int[,] GenerateMaze(int w,int h)
    {
        int[,] a=new int[h,w]; for(int z=0;z<h;z++)for(int x=0;x<w;x++)a[z,x]=1;
        System.Random r=new System.Random(42);
        Stack<Vector2Int> st=new Stack<Vector2Int>(); st.Push(new Vector2Int(1,1)); a[1,1]=0;
        int[] dx={0,2,0,-2}, dz={2,0,-2,0};
        while(st.Count>0)
        {
            var p=st.Peek(); List<int> dirs=new List<int>();
            for(int d=0;d<4;d++){int nx=p.x+dx[d],nz=p.y+dz[d];if(nx>0&&nz>0&&nx<w-1&&nz<h-1&&a[nz,nx]==1)dirs.Add(d);}
            if(dirs.Count==0){st.Pop();continue;}
            int k=dirs[r.Next(dirs.Count)],nx2=p.x+dx[k],nz2=p.y+dz[k];
            a[p.y+dz[k]/2,p.x+dx[k]/2]=0; a[nz2,nx2]=0; st.Push(new Vector2Int(nx2,nz2));
        }
        // Add loops and alternate routes
        for(int i=0;i<45;i++){int x=r.Next(1,w-1),z=r.Next(1,h-1);if(a[z,x]==1 && ((a[z,x-1]==0&&a[z,x+1]==0)||(a[z-1,x]==0&&a[z+1,x]==0)))a[z,x]=0;}
        a[h-2,w-2]=0; a[h-2,w-3]=0; a[h-3,w-2]=0;
        return a;
    }
    static Material Mat(Color c,float metallic,float emission)
    {
        var m=new Material(Shader.Find("Standard"));m.color=c;m.SetFloat("_Metallic",metallic);
        m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*emission);return m;
    }
    static void MakeCube(string n,Vector3 p,Vector3 s,Material m,Transform parent)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.position=p;o.transform.localScale=s;o.transform.SetParent(parent);o.GetComponent<Renderer>().sharedMaterial=m;
    }
    static void BuildUI(int count)
    {
        var game=new GameObject("Game Manager");var gm=game.AddComponent<NeonMazeGame>();gm.SetTotalCoins(count);
        var canvasObj=new GameObject("HUD");var canvas=canvasObj.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvasObj.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;canvasObj.AddComponent<GraphicRaycaster>();
        gm.scoreText=TextUI(canvas.transform,"CORES 0 / "+count,new Vector2(20,-20),new Vector2(0,1),new Vector2(0,1),26);
        gm.timerText=TextUI(canvas.transform,"TIME 180",new Vector2(-20,-20),new Vector2(1,1),new Vector2(1,1),26);
        gm.messageText=TextUI(canvas.transform,"WASD MOVE  •  MOUSE LOOK  •  SHIFT SPRINT  •  SPACE JUMP",new Vector2(0,25),new Vector2(.5f,0),new Vector2(.5f,0),17);
        var panel=new GameObject("End Panel");panel.transform.SetParent(canvas.transform,false);var img=panel.AddComponent<Image>();img.color=new Color(.01f,.02f,.08f,.94f);
        var rt=panel.GetComponent<RectTransform>();rt.anchorMin=new Vector2(.2f,.25f);rt.anchorMax=new Vector2(.8f,.75f);rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        gm.endPanel=panel;gm.endTitle=TextUI(panel.transform,"MAZE CLEARED!",new Vector2(0,-35),new Vector2(.5f,1),new Vector2(.5f,1),38);
        gm.endDetails=TextUI(panel.transform,"Press R to restart",new Vector2(0,35),new Vector2(.5f,.5f),new Vector2(.5f,.5f),22);
        panel.SetActive(false);
    }
    static Text TextUI(Transform parent,string value,Vector2 pos,Vector2 anchor,Vector2 pivot,int size)
    {
        var o=new GameObject("Text");o.transform.SetParent(parent,false);var t=o.AddComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;
        var r=t.rectTransform;r.anchorMin=anchor;r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=new Vector2(600,55);return t;
    }
}
