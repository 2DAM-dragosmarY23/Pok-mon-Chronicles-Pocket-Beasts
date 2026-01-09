using UnityEngine;

/// <summary>
/// Representa una instancia de un movimiento que posee un Pokémon.
/// Mantiene el estado de los PP (Power Points) actuales del movimiento.
/// </summary>
public class Move
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    /// <summary>
    /// Constructor que inicializa un movimiento con sus PP al máximo.
    /// </summary>
    /// <param name="pBase">Datos base del movimiento (plantilla ScriptableObject)</param>
    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }
}