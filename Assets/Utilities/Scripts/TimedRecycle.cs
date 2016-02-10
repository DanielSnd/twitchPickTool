using UnityEngine;
using System.Collections;

public class TimedRecycle : MonoBehaviour {

	public float timeToDespawn=1f;

	ParticleSystem particle;

	void Awake() {
		particle = GetComponent<ParticleSystem> ();
		OnEnable ();
	}

	void OnEnable() {
		if (particle) {
						particle.Clear ();
						particle.Play ();
				}
		StartCoroutine (Despawn ());
	}

	void OnDisable() {
		if (particle) {
			particle.Clear ();
			particle.Stop ();
		}
	}

	IEnumerator Despawn() {
		yield return new WaitForSeconds(timeToDespawn);

		this.transform.Recycle ();
	}

}
