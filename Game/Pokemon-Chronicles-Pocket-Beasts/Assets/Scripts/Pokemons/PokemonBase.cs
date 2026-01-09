using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define las propiedades base de una especie de Pokémon.
/// Sirve como plantilla que contiene información estática compartida por todos
/// los ejemplares de esa especie (sprites, tipos, estadísticas base, movimientos aprendibles).
/// </summary>
[CreateAssetMenu(fileName = "PokemonBase", menuName = "Pokemons/New Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Estadísticas base que determinan el potencial del Pokémon
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int speed;
    [SerializeField] int specialAttack;
    [SerializeField] int specialDefense;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public PokemonType Type1
    {
        get { return type1; }
    }

    public PokemonType Type2
    {
        get { return type2; }
    }

    public int MaxHP
    {
        get { return maxHP; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public int SpecialAttack
    {
        get { return specialAttack; }
    }

    public int SpecialDefense
    {
        get { return specialDefense; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

/// <summary>
/// Estructura que asocia un movimiento con el nivel requerido para aprenderlo.
/// </summary>
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase MoveBase
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

/// <summary>
/// Enumerado que representa todos los tipos de Pokémon disponibles en el juego.
/// </summary>
public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

/// <summary>
/// Enumerado de las estadísticas modificables de un Pokémon.
/// </summary>
public enum Stat { Attack, Defense, Speed, SpAttack, SpDefense }

/// <summary>
/// Tabla estática que define la efectividad de cada tipo contra los demás.
/// Implementa el sistema de fortalezas y debilidades de tipo de Pokémon.
/// </summary>
public class TypeChart
{
    // Matriz de efectividad: filas representan el tipo atacante, columnas el tipo defensor
    static float[][] chart =
    {
        //                    NOR  FIR  WAT  ELE  GRA  ICE  FIG  POI  GRO  FLY  PSY  BUG  ROC  GHO  DRA
        /* NOR */ new float[] {1f, 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f, 0.5f, 0f,  1f},
        /* FIR */ new float[] {1f, 0.5f,0.5f,1f,  2f,  2f,  1f,  1f,  1f,  1f,  1f,  2f, 0.5f,1f,  0.5f},
        /* WAT */ new float[] {1f, 2f,  0.5f,1f,  0.5f,1f,  1f,  1f,  2f,  1f,  1f,  1f, 2f, 1f,  0.5f},
        /* ELE */ new float[] {1f, 1f,  2f,  0.5f,0.5f,1f,  1f,  1f,  0f,  2f,  1f,  1f, 1f, 1f,  0.5f},
        /* GRA */ new float[] {1f, 0.5f,2f,  1f,  0.5f,1f,  1f,  0.5f,2f,  0.5f,1f,  0.5f,2f, 1f,  0.5f},
        /* ICE */ new float[] {1f, 0.5f,0.5f,1f,  2f,  0.5f,1f,  1f,  2f,  2f,  1f,  1f, 1f, 1f,  2f},
        /* FIG */ new float[] {2f, 1f,  1f,  1f,  1f,  2f,  1f,  0.5f,1f,  0.5f,0.5f,0.5f,2f, 0f,  1f},
        /* POI */ new float[] {1f, 1f,  1f,  1f,  2f,  1f,  1f,  0.5f,0.5f,1f,  1f,  1f, 0.5f,0.5f,1f},
        /* GRO */ new float[] {1f, 2f,  1f,  2f,  0.5f,1f,  1f,  2f,  1f,  0f,  1f,  0.5f,2f, 1f,  1f},
        /* FLY */ new float[] {1f, 1f,  1f,  0.5f,2f,  1f,  2f,  1f,  1f,  1f,  1f,  2f, 0.5f,1f,  1f},
        /* PSY */ new float[] {1f, 1f,  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f,  0.5f,1f, 1f, 0f,  1f},
        /* BUG */ new float[] {1f, 0.5f,1f,  1f,  2f,  1f,  0.5f,0.5f,1f,  0.5f,2f,  1f, 1f, 0.5f,1f},
        /* ROC */ new float[] {1f, 2f,  1f,  1f,  1f,  2f,  0.5f,1f,  0.5f,2f,  1f,  2f, 1f, 1f,  1f},
        /* GHO */ new float[] {0f, 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f, 1f, 2f,  1f},
        /* DRA */ new float[] {1f, 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f, 1f, 1f,  2f},
    };

    /// <summary>
    /// Calcula el multiplicador de efectividad cuando un tipo ataca a otro.
    /// </summary>
    /// <param name="attackType">Tipo del movimiento atacante</param>
    /// <param name="defenseType">Tipo del Pokémon defensor</param>
    /// <returns>Multiplicador de daño (0, 0.5, 1, o 2)</returns>
    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}