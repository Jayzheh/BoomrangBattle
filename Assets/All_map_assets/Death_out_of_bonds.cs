using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathOutOfBounds : MonoBehaviour
{
    // Définir les limites de la carte
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;
    public float minZ = -10f;
    public float maxZ = 10f;

    // Update is called once per frame
    void Update()
    {
        // Vérifier si le personnage est hors des limites
        if (IsOutOfBounds())
        {
            // Déclencher la mort du personnage
            Die();
        }
    }

    // Méthode pour vérifier si le personnage est hors des limites
    bool IsOutOfBounds()
    {
        Vector3 position = transform.position;
        return position.x < minX || position.x > maxX || 
               position.y < minY || position.y > maxY ||
               position.z < minZ || position.z > maxZ;
    }

    // Méthode pour gérer la mort du personnage
    void Die()
    {
        // Ici, vous pouvez ajouter le code pour gérer la mort du personnage
        // Par exemple, réinitialiser la position, jouer une animation, afficher un message, etc.
        Debug.Log("OUT OF BONDS!");
        // Vous pouvez aussi désactiver le personnage ou charger une nouvelle scène
        gameObject.SetActive(false); // Désactive le personnage
        // ou
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Recharge la scène actuelle
    }
}

