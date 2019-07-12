
using UnityEngine;

namespace Misc
{
    public class SingletonBehavior<T> : MonoBehaviour where T : Component {
        static T _instance;

        public static T instance
        {
            get { return getInstance(); }
        }

		public static T getInstance() {
			if (_instance == null) {
				GameObject go = new GameObject(typeof(T).Name);
				_instance = go.AddComponent<T>();
			}
			return _instance;
		}

		protected virtual void Awake() {
			if (_instance == null) {
				_instance = this.GetComponent<T>();
			}
			DontDestroyOnLoad(this.gameObject);
		}
	}
}
