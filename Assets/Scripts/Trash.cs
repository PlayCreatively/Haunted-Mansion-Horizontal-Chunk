using UnityEngine;

public class Trash : Carriable
{
    [SerializeField]
    Mesh[] meshStages;

    [SerializeField]
    Mesh[] trashVariations;

    MeshFilter meshFilter;

    private void Awake()
    {
        // Get the MeshFilter component attached to this GameObject
        meshFilter = GetComponent<MeshFilter>();

        //meshFilter.mesh = trashVariations[Random.Range(0, trashVariations.Length - 1)];
    }

    void Update()
    {
        
    }
}
