using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public float spawnInterval = 20f;
    public int maxCount = 2;

    Enemy[] spawnedEnemies;

    void Start()
    {
        spawnedEnemies = new Enemy[maxCount];

        spawnedEnemies[0] = SpawnEnemy();

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval + Random.Range(0, spawnInterval * .5f));

            for (int i = 0; i < spawnedEnemies.Length; i++)
                if(spawnedEnemies[i] == null)
                {
                    spawnedEnemies[i] = SpawnEnemy();
                    break;
                }
        }
    }

    Enemy SpawnEnemy()
    {
        return Instantiate(enemyPrefab, transform.position, transform.rotation);
    }

    void OnValidate()
    {
        if (enemyPrefab == null)
            return;

        gameObject.name = enemyPrefab.name + " Spawner";
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // draw text of enemy prefab name
        UnityEditor.Handles.Label(transform.position, enemyPrefab.name, new GUIStyle
        {
            fontSize = 8,
            normal = new GUIStyleState
            {
                textColor = Color.white
            },
            alignment = TextAnchor.MiddleCenter
        });

        // thing so select
        // draw a circle around the spawner
        Gizmos.color = new Color(1,0,0,.5f);
        Mesh sharedMesh;

        sharedMesh = enemyPrefab.GetComponentInChildren<MeshFilter>()?.sharedMesh;
        if(sharedMesh == null)
            sharedMesh = enemyPrefab.GetComponentInChildren<SkinnedMeshRenderer>()?.sharedMesh;

        //Gizmos.DrawMesh(sharedMesh, transform.position, transform.rotation, Vector3.one);
    }
#endif
}
