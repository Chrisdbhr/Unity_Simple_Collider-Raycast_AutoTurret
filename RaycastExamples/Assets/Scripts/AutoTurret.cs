using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turret with automatic fire.
/// </summary>
public class AutoTurret : MonoBehaviour
{

	[Range(1f, 25f)]
	[SerializeField]
	private float m_viewRange = 20f;									// enemy fov.
	[SerializeField] private LayerMask m_targetsLayer;					// layer where targets are.

	[System.Serializable]
	private class Gun
	{
		[Range(0.1f, 1f)]
		[HideInInspector] public float m_rateOfFire = 0.5f;				// how faster the turret fires. time in seconds.
		[HideInInspector] public float m_lastShootTime;					// time sinc last shoot.
		[HideInInspector] public float m_bulletStrengh = 2f;			// strengh of the bullet. To apply a impulse on hit.
		public Transform m_barel;										// gun barrel to point at target.
		public Transform m_barelTip;									// tip of the gun barrel.
		public GameObject m_muzzleFire;									// muzzle fire effect obj.
	}
	[SerializeField] private Gun m_gun;

	private Collider[] m_colliders;										// collider detected balls.
	private int m_closerColliderIndex;
	private float m_targetDistance;										// turret target distance at the raycast moment.
	private RaycastHit m_rayHit;
	private Ray m_ray;													// Common raycast ray.

	[SerializeField] private UnityEngine.UI.Text m_textDistance;		// canvas text to show distance.

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, m_viewRange);
	}

	void Update()
	{
		// Shot at target.
		if (m_gun.m_lastShootTime >= m_gun.m_rateOfFire)
		{
			m_colliders = Physics.OverlapSphere(m_gun.m_barelTip.position, m_viewRange, m_targetsLayer, QueryTriggerInteraction.Collide);

			FindNearestTarget(m_colliders);

			if (m_colliders != null)
			{
				m_ray = new Ray(m_gun.m_barelTip.position, m_colliders[m_closerColliderIndex].transform.position - m_gun.m_barelTip.transform.position);
				if (m_colliders[m_closerColliderIndex].Raycast(m_ray, out m_rayHit, m_viewRange))
				{
					if (m_rayHit.rigidbody)
					{
						Debug.Log("Object hit name: " + m_rayHit.collider.name +  ", distance from turret: " + m_targetDistance);
						Debug.DrawLine(m_gun.m_barelTip.position, m_colliders[m_closerColliderIndex].transform.position, Color.magenta, 2f);
						m_rayHit.rigidbody.AddForce((m_rayHit.rigidbody.position - transform.position) * m_gun.m_bulletStrengh, ForceMode.Impulse);
						m_gun.m_lastShootTime = 0;
						StartCoroutine(MuzzleFire());
					}
				}
			}
		}
		else
		{
			m_gun.m_lastShootTime += Time.deltaTime;
		}

		// Point to target
		if (m_colliders != null && m_colliders.Length > 0 && m_closerColliderIndex < m_colliders.Length)
		{
			Debug.DrawRay(m_gun.m_barelTip.position, m_colliders[m_closerColliderIndex].transform.position - m_gun.m_barelTip.transform.position, Color.red);
			m_gun.m_barel.LookAt(m_colliders[m_closerColliderIndex].transform);
			m_textDistance.text = m_targetDistance.ToString("#.##");
		}


	}

	/// <summary>
	/// Find nearest index from a colliders list.
	/// </summary>
	private void FindNearestTarget(Collider[] m_colliders)
	{
		m_closerColliderIndex = 0;
		for (int i = 0; i < m_colliders.Length; i++)
		{
			// Check if its the closest object.
			if (
			Vector3.Distance(m_gun.m_barelTip.transform.position, m_colliders[i].transform.position) <
			Vector3.Distance(m_gun.m_barelTip.transform.position, m_colliders[m_closerColliderIndex].transform.position)
			)
			{
				m_closerColliderIndex = i;
				m_targetDistance = Vector3.Distance(m_gun.m_barelTip.transform.position, m_colliders[m_closerColliderIndex].transform.position);
			}
		}
	}

	private IEnumerator MuzzleFire()
	{
		m_gun.m_muzzleFire.SetActive(true);
		// I know its better to cache this WaitForSeconds, but just using that way to simplify things.
		yield return new WaitForSeconds(0.1f);
		m_gun.m_muzzleFire.SetActive(false);
	}
}
