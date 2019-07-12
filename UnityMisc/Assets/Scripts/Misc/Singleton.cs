using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc
{
    public class Singleton<T> where T : new()
    {
        public static T ins
        {
            get
            {
                return Singleton<T>.GetInstance();
            }
        }

        private static T _ins = default(T);

        public static T GetInstance()
        {
            if (_ins == null)
            {
                _ins = new T();
            }
            return _ins;
        }
        public static void DestroyInstance()
        {
            _ins = default(T);
        }
        //create none singleton instance
        public static T Create()
        {
            return new T();
        }
    }
}