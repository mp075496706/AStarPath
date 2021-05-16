using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Point
{
    [SerializeField]
    public int x;
    [SerializeField]
    public int y;
    [SerializeField]
    public Point parentPoint;
    [SerializeField]
    public int F;
    [SerializeField]
    public int G;
    [SerializeField]
    public int H;
    [SerializeField]
    public int parentG;

    [SerializeField]
    public Point()
    {

    }

    [SerializeField]
    public Point(int x,int y)
    {
        this.x = x;
        this.y = y;
    }

    [SerializeField]
    public Point(int x,int y,Point parentPoint,int F,int G,int H,int parentG)
    {
        this.x = x;
        this.y = y;
        this.parentPoint = parentPoint;
        this.F = F;
        this.G = G;
        this.H = H;
        this.parentG = parentG;
    }

    public void CalcF()
    {
        F = G + H;
    }
}
