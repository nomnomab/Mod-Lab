    ����          PAssembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null   Plugins.ModLab.Mod   NameVersionAuthorDescription<Scripts>k__BackingField�System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]      
Sample Mod   1.0.0   Cassidy   Example	      �System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   VersionComparerHashSizeKeyValuePairs  �System.Collections.Generic.GenericEqualityComparer`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]�System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][]   	      		      �System.Collections.Generic.GenericEqualityComparer`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]    	          �System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]�����System.Collections.Generic.KeyValuePair`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   keyvalue   SampleBehaviour   �	using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Plugins.ModLab
{
    public class SampleBehaviour : MonoBehaviour
    {
        // edit this class by scrolling with the arrow keys
        private GameObject _cube;
        private Rigidbody _rBody;

        private void Start()
        {
            Debug.Log("Hi, I was dynamically created and placed into the scene.");
            var find = GameObject.Find("TestCube");
            if (find == null)
            {
                _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _cube.name = "TestCube";
            }
            else _cube = find;

            _rBody = _cube.AddComponent<Rigidbody>();
            _cube.AddComponent<BoxCollider>();

            StartCoroutine(BounceCube());
        }

        private IEnumerator BounceCube()
        {
            var waitTime = Random.Range(2, 5);
            yield return new WaitForSeconds(waitTime);
            _rBody.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
            StartCoroutine(BounceCube());
        }
    }
}