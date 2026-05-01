using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatEventArgs : EventArgs
{
    public float Percent { get; set; }
    public float Current { get; set; }
    public float Max { get; set; }
    public float Delta { get; set; }
}