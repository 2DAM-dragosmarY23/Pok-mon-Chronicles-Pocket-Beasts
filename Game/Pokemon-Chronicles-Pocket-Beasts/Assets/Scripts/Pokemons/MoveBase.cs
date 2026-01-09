using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define las propiedades base de un movimiento de Pokémon.
/// Contiene información estática como poder, precisión, tipo y efectos secundarios.
/// </summary>
[CreateAssetMenu(fileName = "MoveBase", menuName = "Pokemons/New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;

    public string MoveName
    {
        get { return moveName; }
    }

    public string Description
    {
        get { return description; }
    }

    public PokemonType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int PP
    {
        get { return pp; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects
    {
        get { return effects; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

/// <summary>
/// Define los efectos secundarios que puede tener un movimiento.
/// Actualmente soporta modificadores de estadísticas (boosts).
/// </summary>
[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
}

/// <summary>
/// Representa un incremento o decremento de una estadística específica.
/// </summary>
[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

/// <summary>
/// Categoría del movimiento que determina cómo se calcula el daño.
/// Physical usa Ataque/Defensa, Special usa Ataque Especial/Defensa Especial,
/// y Status no causa daño directo.
/// </summary>
public enum MoveCategory
{
    Physical,
    Special,
    Status
}

/// <summary>
/// Define el objetivo del movimiento: el oponente o el propio usuario.
/// </summary>
public enum MoveTarget
{
    Foe,
    Self
}