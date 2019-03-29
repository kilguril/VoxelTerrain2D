using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelTerrain2D.Utils
{
    public class CameraControls : MonoBehaviour
    {
        [SerializeField]
        private float m_moveSpeed = default( float );

        void Update()
        {
            Vector3 position = transform.position;

            if ( Input.GetKey( KeyCode.D ) ){ position += transform.right * m_moveSpeed * Time.deltaTime; }
            if ( Input.GetKey( KeyCode.A ) ){ position -= transform.right * m_moveSpeed * Time.deltaTime; }

            if ( Input.GetKey( KeyCode.W ) ){ position += transform.up * m_moveSpeed * Time.deltaTime; }
            if ( Input.GetKey( KeyCode.S ) ){ position -= transform.up * m_moveSpeed * Time.deltaTime; }

            if ( Input.GetKey( KeyCode.Q ) ){ position += transform.forward * m_moveSpeed * Time.deltaTime; }
            if ( Input.GetKey( KeyCode.E ) ){ position -= transform.forward * m_moveSpeed * Time.deltaTime; }

            transform.position = position;
        }
    }
}