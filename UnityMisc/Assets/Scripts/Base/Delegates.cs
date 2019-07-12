using UnityEngine;
using System.Collections;

public delegate void VoidFuncVoid();
public delegate void VoidFuncObject(object obj);
public delegate void VoidFuncString(string str);
public delegate void VoidFuncBytes(byte[] data);

public delegate object ObjectFuncVoid();

public delegate void VoidFunc3<T0, T1, T2>(T0 t0, T1 t1, T2 t2);
public delegate void VoidFunc2<T0, T1>(T0 t0, T1 t1);
public delegate void VoidFunc1<T0>(T0 t0);

public delegate void VoidFuncN<T0, T1, T2, T3, T4>(T0 t0, T1 t1, T2 t2, T3 t3, T4 t4);
public delegate void VoidFuncN<T0, T1, T2, T3>(T0 t0, T1 t1, T2 t2, T3 t3);
public delegate void VoidFuncN<T0, T1, T2>(T0 t0, T1 t1, T2 t2);
public delegate void VoidFuncN<T0, T1>(T0 t0, T1 t1);
public delegate void VoidFuncN<T0>(T0 t0);

public delegate bool BoolFuncN<T0>(T0 t0);

public delegate bool BoolFuncVoid();


