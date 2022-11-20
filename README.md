# 简单 Hit UFO 小游戏
## 游戏规则
鼠标点击射击10个小飞碟，每个飞碟根据颜色、大小不同获得不同分数，飞碟的初始速度、发射角度与位置随机。
## 游戏要求
使用带缓存的工厂模式管理不同飞碟的生产与回收，该工厂必须是场景单实例的！具体实现见参考资源Singleton 模板类

尽可能使用前面的 MVC 结构实现人机交互与游戏模型分离

## 游戏实现
### 基本思路
需要实现```DiskFactory```管理飞碟，由```GameJudge```进行游戏记分与游戏状态判断，```SceneController```进行游戏管理。至于飞碟的大小与颜色使用预制的飞碟和```Material```。工厂会对这两种预制进行基于一定概率的组合组装。

本次飞碟的动作使用了```rigidbody```自带的重力系统，所以没有专门实现飞碟的动作，射击则是使用课上讲到的射线进行处理。

### 部分代码实现
#### 工厂```DiskFactory```
由于游戏实现思路是随机生成飞碟，所以没有使用列表储存可用飞碟，而只储存了已经生产使用的飞碟。
```
public class DiskFactory: MonoBehaviour
{
    private static DiskFactory instance;
    float disk_border = 6.0f;
    float color_border1 = 5.0f;
    float color_border2 = 8.0f;
    public List<Disk> used_disks =new List<Disk>();

    public static DiskFactory getInstance()
    {
        if (instance == null)
        {
            instance = (DiskFactory)FindObjectOfType(typeof(DiskFactory));
            if (instance == null)
            {
                Debug.LogError("An instance of " + typeof(DiskFactory)
                    + " is needed in the scene, but there is none.");
            }
        }
        return instance;
    }

    public Disk getDisk()
    {
        int disk_num = Random.Range(0,10);
        int color_num = Random.Range(0, 10);
        //float x_speed = Random.Range(-25, -15);
        //float y_speed = Random.Range(0, 5);
        float y_pos = Random.Range(8, 15);

        GameObject ufo;
        Material mat;
        // get disk prefab
        if (disk_num < disk_border)
        {
            ufo = Instantiate(Resources.Load<GameObject>("Prefabs/UFO_1"));
            disk_border -= 0.5f;
        }
        else
        {
            ufo = Instantiate(Resources.Load<GameObject>("Prefabs/UFO_2"));
            disk_border += 0.5f;
        }
        // get material prefab
        if (color_num < color_border1)
        {
            mat = Resources.Load<Material>("Prefabs/UFO_C1");
            color_border1 -= 0.5f;
            color_border2 -= 0.25f;
        }
        else if(color_num > color_border2)
        {
            mat = Resources.Load<Material>("Prefabs/UFO_C3");
            color_border1 += 0.25f;
            color_border2 += 0.5f;
        }
        else
        {
            mat = Resources.Load<Material>("Prefabs/UFO_C2");
            color_border1 += 0.25f;
            color_border2 -= 0.25f;
        }

        // twerking attributes 
        ufo.transform.localPosition = new Vector3(20, y_pos, 0);
        ufo.GetComponent<Renderer>().material = mat;
        ufo.gameObject.SetActive(false);
        //ufo.GetComponent<Rigidbody>().velocity = new Vector3(x_speed, y_speed, 0); 

        
        Disk disk = new Disk(ufo,mat);
        used_disks.Add(disk);
        return disk;
    }

    public void destroyDisk(Disk disk)
    {
        for(int i = 0; i < used_disks.Count; i++)
        {
            if (disk.ufo.GetInstanceID() == used_disks[i].ufo.GetInstanceID())
            {
                used_disks[i].ufo.gameObject.SetActive(false);
                Destroy(used_disks[i].ufo);
                used_disks.RemoveAt(i);
            }
        }
    }
    private void Start()
    {

    }
}
```

#### 计分员```GameJudge```
```
public class GameJudge : MonoBehaviour
{   
    private static GameJudge instance;
    public int score = 0;
    public int trials = 0;
    public int max_trials = 1;

    public static GameJudge getInstance()
    {
        if (instance == null)
        {
            instance = (GameJudge)FindObjectOfType(typeof(GameJudge));
            if (instance == null)
            {
                Debug.LogError("An instance of " + typeof(GameJudge)
                    + " is needed in the scene, but there is none.");
            }
        }
        return instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    public void scoring(Disk disk)
    {
        score += disk.score;
    }

    public bool over()
    {
        if( trials > max_trials)
        {
            return true;
        }
        return false;
    } 
    public void Reset()
    {
        score = 0;
        trials = 0;
    }
}
```

#### 场记```SceneController```
指挥计分员与工厂，控制游戏进程
```
public class SceneController : MonoBehaviour, IPlayerAction, ISceneController
{
    private static SceneController instance;

    public GameJudge gamejudge;
    public DiskFactory factory;


    private Queue<Disk> disk_queue=new Queue<Disk>();
    private bool isStart = false;
    private bool isPlaying = false;
    private bool isOver = false;

    public static SceneController getInstance()
    {
        if(instance == null)
        {
            instance = (SceneController)FindObjectOfType(typeof(SceneController));
            if (instance == null)
            {
                Debug.LogError("An instance of " + typeof(SceneController)
                    + " is needed in the scene, but there is none.");
            }
        }
        return instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        SSDirector director = SSDirector.GetInstance();
        director.CurrentScenceController = this;
        gamejudge = GameJudge.getInstance();
        factory = DiskFactory.getInstance();
        loadResources();
    }

    // Update is called once per frame
    void Update()
    {
        if (isStart)
        {
            if (isOver)
            {
                CancelInvoke("loadResources");
            }
            if (!isPlaying)
            {
                // 定时调用
                InvokeRepeating("loadResources", 2f, 2f);
                isPlaying = true;
            }
            LaunchDisk();
        }
        if (gamejudge.over())
        {
            isOver = true;
        }
    }
    public void loadResources()
    {
        disk_queue.Enqueue(factory.getDisk());
    }


    public void LaunchDisk()
    {
        if(disk_queue.Count > 0 && gamejudge.trials++<gamejudge.max_trials)
        {
            Disk disk = disk_queue.Dequeue();
            Debug.Log("Launching...");
            float x_speed = Random.Range(-25, -15);
            float y_speed = Random.Range(0, 5);
            disk.ufo.gameObject.SetActive(true);
            disk.ufo.GetComponent<Rigidbody>().velocity = new Vector3(x_speed, y_speed, 0);
        }       
    }
    
    public void gameStart()
    {
        isStart = true;
    }

    public void shoot(GameObject cam)
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 mp = Input.mousePosition; //get Screen Position

            //create ray, origin is camera, and direction to mousepoint
            Camera ca;
            if (cam != null) ca = cam.GetComponent<Camera>();
            else ca = Camera.main;

            Ray ray = ca.ScreenPointToRay(Input.mousePosition);

            //Return the ray's hit
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                for(int i = 0; i < factory.used_disks.Count; i++)
                {
                    if (factory.used_disks[i].ufo == hit.transform.gameObject)
                    {
                        Vector3 pos = hit.transform.gameObject.transform.localPosition;
                        gamejudge.scoring(factory.used_disks[i]);
                        factory.destroyDisk(factory.used_disks[i]);
                        Object exp=Instantiate(Resources.Load("Prefabs/Exploson6"),pos,Quaternion.identity);
                        Destroy(exp, 1f);
                    }
                } 
            }
        }
    }

    public int getScore()
    {
        return gamejudge.score;
    }
}
```
```shoot()```使用了爆炸的粒子特效，资源来自Asset Store。
```
Object exp=Instantiate(Resources.Load("Prefabs/Exploson6"),pos,Quaternion.identity);
Destroy(exp, 1f);
```

#### ```TerrainBehaviour```
飞碟掉在地上要消失，正好飞碟使用了rigidbody，于是使用碰撞检测。```TerrainBehaviour```挂载在地形上。
```
public class TerrainBehaviour : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody)
        {
            Destroy(collision.rigidbody.gameObject);
        }
    }
}
```
#
完整代码见项目文件（仅上传了Asset）。

演示视频同样见上文件。
