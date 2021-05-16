using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [Tooltip("点位预设")]
    public GameObject pointObj;

    public Transform scrollContent;
    public Toggle showFText;

    List<Point> mapAllPointList; //地图所有点位列表
    List<Point> obsPointList;    //障碍点位列表
    Point startPoint, endPoint;  //起点、终点
    public List<Point> openList, closeList;
    List<Point> showTextList;

    bool isFindPath = false;

    void Start()
    {
        mapAllPointList = new List<Point>();
        obsPointList = new List<Point>();
        openList = new List<Point>();
        closeList = new List<Point>();
        showTextList = new List<Point>();

        StartCoroutine(GenerateMap());
    }

    private void Update()
    {
        //x 0-29
        //y 0-20
        if (Input.GetKeyDown(KeyCode.J))
        {
            startPoint = new Point(0, 0);
            endPoint = new Point(5, 5);

            BeginPath();
        }
    }

    /// <summary>
    /// 生成地图
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateMap()
    {
        GameObject obj;
        for (int i = 0; i < 21; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                obj = Instantiate(pointObj, scrollContent);
                obj.transform.name = i + " ," + j;

                Point point = new Point();
                point.x = j;
                point.y = i;
                mapAllPointList.Add(point);
            }
        }
        yield return null;
    }

    /// <summary>
    /// 随机起点和终点
    /// </summary>
    public void RandomStartEndPoint()
    {
        if (mapAllPointList.Count > 0)
        {
            ClearStartEndPoint();

            do
            {
                //随机起点、终点
                startPoint = mapAllPointList[UnityEngine.Random.Range(0, mapAllPointList.Count / 2)];
                endPoint = mapAllPointList[UnityEngine.Random.Range(mapAllPointList.Count / 2, mapAllPointList.Count)];
            } while (startPoint == endPoint || IsObsPoint(startPoint) || IsObsPoint(endPoint));

            //Debug.Log("起点：" + startPoint.x + " " + startPoint.y);
            //Debug.Log("终点：" + endPoint.x + " " + endPoint.y);

            //设置起点、终点的颜色
            scrollContent.transform.Find(startPoint.y + " ," + startPoint.x).GetComponent<Image>().color = Color.green;
            scrollContent.transform.Find(endPoint.y + " ," + endPoint.x).GetComponent<Image>().color = Color.blue;
        }
    }

    void ClearStartEndPoint()
    {
        if (startPoint != null) scrollContent.transform.Find(startPoint.y + " ," + startPoint.x).GetComponent<Image>().color = Color.white;
        if (endPoint != null) scrollContent.transform.Find(endPoint.y + " ," + endPoint.x).GetComponent<Image>().color = Color.white;
        startPoint = null;
        endPoint = null;
    }

    /// <summary>
    /// 随机障碍物
    /// </summary>
    public void RandomObs()
    {
        if (startPoint != null && endPoint != null)
        {
            ClearObs();
            for (int i = 0; i < 60; i++)
            {
                int index = 0;
                do
                {
                    index = UnityEngine.Random.Range(0, mapAllPointList.Count);
                } while (mapAllPointList[index] == startPoint || mapAllPointList[index] == endPoint);

                //Debug.Log("障碍点:" + mapAllPointList[index].x + " " + mapAllPointList[index].y);
                scrollContent.Find(mapAllPointList[index].y + " ," + mapAllPointList[index].x).GetComponent<Image>().color = Color.red;
                obsPointList.Add(mapAllPointList[index]);
            }
        }
        else
        {
            Debug.LogError("起点和终点没有设置！");
        }
    }

    void ClearObs()
    {
        if (obsPointList.Count > 0)
        {
            for (int i = 0; i < obsPointList.Count; i++)
            {
                scrollContent.transform.Find(obsPointList[i].y + " ," + obsPointList[i].x).GetComponent<Image>().color = Color.white;
            }
            obsPointList.Clear();
        }
    }

    /// <summary>
    /// 是否为障碍点位
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    bool IsObsPoint(Point point)
    {
        bool isObs = false;
        if (obsPointList.Count > 0)
        {
            foreach (var item in obsPointList)
            {
                if (item.x == point.x && item.y == point.y) isObs = true;
            }
        }
        return isObs;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        isFindPath = false;

        ClearStartEndPoint();
        ClearObs();

        foreach (var point in closeList)
        {
            scrollContent.Find(point.y + " ," + point.x).GetComponent<Image>().color = Color.white;
        }
        openList.Clear();
        closeList.Clear();

        foreach (var point in showTextList)
        {
            scrollContent.Find(point.y + " ," + point.x).transform.Find("F").GetComponent<Text>().text = "";
            scrollContent.Find(point.y + " ," + point.x).transform.Find("G").GetComponent<Text>().text = "";
            scrollContent.Find(point.y + " ," + point.x).transform.Find("H").GetComponent<Text>().text = "";
            scrollContent.Find(point.y + " ," + point.x).transform.Find("ParentG").GetComponent<Text>().text = "";
        }
        showTextList.Clear();

    }

    /// <summary>
    /// 开始寻路
    /// </summary>
    public void BeginPath()
    {
        if (startPoint != null && endPoint != null)
        {
            openList.Add(startPoint);
            AStartPath();
        }
    }

    void AStartPath()
    {
        while (openList.Count != 0)
        {
            //获得openList中最小F值的点
            Point openList_MinF = GetPointInOpenList_MinF();

            if (openList_MinF.x == endPoint.x && openList_MinF.y == endPoint.y) isFindPath = true;
            else
            {
                openList.Remove(openList_MinF);
                //最小值F的点位加入closeList不再检测
                closeList.Add(openList_MinF);

                //获取最小值F的点位附件的点位
                List<Point> arroundPointList = ArroundPointList(openList_MinF);

                foreach (var point in arroundPointList)
                {
                    bool closeContain = false;
                    foreach (var item in closeList)
                    {
                        if (item.x == point.x && item.y == point.y)
                        {
                            closeContain = true;
                            break;
                        }
                    }
                    if (closeContain) continue;
                    

                    //如果openList包含，重新计算G值。
                    if (openList.Contains(point))
                    {
                        FoundPoint(openList_MinF, point);
                    }
                    else
                    {
                        NotFoundPoint(openList_MinF, endPoint, point);
                    }
                }
            }

            //检测openList中是否有终点
            //foreach (var point in openList)
            //{
            //    if (openList_MinF.x == endPoint.x && openList_MinF.y == endPoint.y) isFindPath = true;
            //}
            if (isFindPath) break;
        }

        if (closeList.Count > 0)
        {
            Debug.Log("找到路径！"+closeList.Count);
            for (int i = 0; i < closeList.Count; i++)
            {
                Debug.Log(closeList[i].x+" "+closeList[i].y);
            }

            //从终点逆向查找路径点
            Point p = closeList[closeList.Count-1];
            scrollContent.Find(p.y + " ," + p.x).GetComponent<Image>().color = Color.yellow;
            while (p.parentPoint != null)
            {
                p = p.parentPoint;
                //最后一个点的父节点为起点，需要判断
                if(p.x==startPoint.x && p.y == startPoint.y)
                {
                    Debug.Log("起点");
                }
                else
                {
                    scrollContent.Find(p.y + " ," + p.x).GetComponent<Image>().color = Color.yellow;
                }
            }
        }
        else
        {
            Debug.LogError("没有找到路径！");
        }
    }

    /// <summary>
    /// 获取openList中F最小值的点位
    /// </summary>
    /// <returns></returns>
    Point GetPointInOpenList_MinF()
    {
        List<Point> orderList = openList.OrderBy(p => p.F).ToList();
        Debug.Log(orderList[0].x + "," + orderList[0].y + "  " + orderList[0].F);
        return orderList[0];
    }

    /// <summary>
    /// 获取一个点位附件可到达的点
    /// 障碍点除外
    /// 拐角由bool值控制
    /// </summary>
    /// <param name="point"></param>
    /// <param name="isAllowCorner"></param>
    /// <returns></returns>
    List<Point> ArroundPointList(Point point)
    {
        List<Point> arroundPointList = new List<Point>();

        for (int i = point.x - 1; i <= point.x + 1; i++)
        {
            for (int j = point.y - 1; j <= point.y + 1; j++)
            {
                Point p = new Point();
                p.x = i;
                p.y = j;
                arroundPointList.Add(p);
            }
        }
        //清洗九宫格数据
        for (int i = arroundPointList.Count-1; i>=0 ; i--)
        {
            //超出地图范围、point点位、障碍点位去掉
            if (arroundPointList[i].x<0 || arroundPointList[i].x > 29 || arroundPointList[i].y < 0 || arroundPointList[i].y > 20
                || (arroundPointList[i].x==point.x && arroundPointList[i].y==point.y) || IsObsPoint(arroundPointList[i]))
                arroundPointList.RemoveAt(i);
        }
        //Debug.Log("周围点数:" + arroundPointList.Count);
        return arroundPointList;
    }

    void FoundPoint(Point minF_point,Point point)
    {
        int G = CalG(minF_point, point);
        if (G < point.G)
        {
            point.parentPoint = minF_point;
            point.G = G;
            point.H = CalH(endPoint, point); ;
            point.CalcF();

            if (showFText.isOn)
            {
                scrollContent.Find(point.y + " ," + point.x).transform.Find("F").GetComponent<Text>().text = point.F.ToString();
                scrollContent.Find(point.y + " ," + point.x).transform.Find("G").GetComponent<Text>().text = point.G.ToString();
                scrollContent.Find(point.y + " ," + point.x).transform.Find("H").GetComponent<Text>().text = point.H.ToString();
                scrollContent.Find(point.y + " ," + point.x).transform.Find("ParentG").GetComponent<Text>().text = point.parentPoint.G.ToString();
                showTextList.Add(point);
            }
        }
    }

    void NotFoundPoint(Point minF_point,Point endPoint,Point point)
    {
        point.parentPoint = minF_point;
        point.G = CalG(minF_point, point);
        point.H = CalH(endPoint, point);
        point.CalcF();

        if (showFText.isOn)
        {
            scrollContent.Find(point.y + " ," + point.x).transform.Find("F").GetComponent<Text>().text = point.F.ToString();
            scrollContent.Find(point.y + " ," + point.x).transform.Find("G").GetComponent<Text>().text = point.G.ToString();
            scrollContent.Find(point.y + " ," + point.x).transform.Find("H").GetComponent<Text>().text = point.H.ToString();
            scrollContent.Find(point.y + " ," + point.x).transform.Find("ParentG").GetComponent<Text>().text = point.parentPoint.G.ToString();
            showTextList.Add(point);
        }

        if (!IsObsPoint(point))
            openList.Add(point);
    }

    int CalG(Point start,Point point)
    {
        int G = (Math.Abs(point.x - start.x) + Math.Abs(point.y - start.y)) == 2 ? 14 : 10;
        int parentG = point.parentPoint != null ? point.parentPoint.G : 0;
        return G + parentG;
    }

    int CalH(Point endPoint,Point point)
    {
        int step = Math.Abs(point.x - endPoint.x) + Math.Abs(point.y - endPoint.y);
        return step * 10;
    }
}
