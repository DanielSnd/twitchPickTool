using UnityEngine;

public class ConstantRotation : MonoBehaviour {

	public float rotationSpeed;

	// Update is called once per frame
	void Update () {
		//Rotacionar baseado na float de rotacao.
		transform.Rotate (new Vector3 (0, 0, rotationSpeed * Time.deltaTime));
	}
}
