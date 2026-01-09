using System.Collections;
using UnityEngine;

/// <summary>
/// Componente que gestiona la visualización de la barra de salud.
/// Permite actualizar el valor de forma instantánea o con una animación suave.
/// </summary>
public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    /// <summary>
    /// Establece inmediatamente el valor de la barra de salud.
    /// </summary>
    /// <param name="hpNormalized">Valor normalizado entre 0 y 1 que representa el porcentaje de HP</param>
    public void SetHp(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    /// <summary>
    /// Actualiza el valor de la barra de salud de forma progresiva y suave.
    /// La animación se ejecuta hasta que la diferencia con el valor objetivo es despreciable.
    /// </summary>
    /// <param name="newHp">Nuevo valor normalizado de HP (entre 0 y 1)</param>
    public IEnumerator SetHpSmooth(float newHp)
    {
        float currentHp = health.transform.localScale.x;
        float changeAmt = currentHp - newHp;

        while (currentHp - newHp > Mathf.Epsilon)
        {
            currentHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHp, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);
    }
}