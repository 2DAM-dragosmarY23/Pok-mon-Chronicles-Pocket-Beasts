using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase que representa una instancia individual de un Pokémon en el juego.
/// Gestiona sus estadísticas, movimientos, puntos de salud y modificadores temporales.
/// </summary>
[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base
    {
        get { return _base; }
    }

    public int Level
    {
        get { return level; }
    }

    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    /// <summary>
    /// Inicializa el Pokémon generando sus movimientos, calculando estadísticas
    /// y estableciendo su HP al máximo. Debe llamarse antes de usar el Pokémon.
    /// </summary>
    public void Init()
    {
        // Generar movimientos basados en el nivel
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.MoveBase));
            }

            if (Moves.Count >= 4)
            {
                break;
            }
        }

        CalculateStats();
        HP = MaxHP;

        // Inicializar los modificadores de estadísticas en 0 (sin cambios)
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 }
        };
    }

    /// <summary>
    /// Calcula las estadísticas del Pokémon basándose en sus stats base y nivel.
    /// Utiliza fórmulas simplificadas inspiradas en los juegos oficiales.
    /// </summary>
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpecialAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpecialDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10;
    }

    /// <summary>
    /// Obtiene el valor efectivo de una estadística aplicando los modificadores temporales.
    /// Los boosts pueden multiplicar o dividir la estadística hasta en un rango de -6 a +6.
    /// </summary>
    /// <param name="stat">Estadística a consultar</param>
    /// <returns>Valor de la estadística con modificadores aplicados</returns>
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Aplicar efectos de boosteo
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    /// <summary>
    /// Aplica una lista de modificadores de estadísticas al Pokémon.
    /// Los valores se acumulan pero están limitados entre -6 y +6.
    /// </summary>
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    // Propiedades que devuelven las estadísticas con modificadores aplicados
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHP { get; private set; }

    /// <summary>
    /// Calcula y aplica el daño recibido de un movimiento enemigo.
    /// Implementa la fórmula de daño de Pokémon incluyendo críticos, efectividad de tipo y variación aleatoria.
    /// </summary>
    /// <param name="move">Movimiento que causa el daño</param>
    /// <param name="attacker">Pokémon que ejecuta el ataque</param>
    /// <returns>Detalles del daño para retroalimentación visual</returns>
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        // Calcular probabilidad de golpe crítico (6.25% como en generaciones clásicas)
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        // Calcular efectividad de tipo considerando ambos tipos del defensor
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) *
                     TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Critical = critical,
            TypeEffectiveness = type,
            Fainted = false
        };

        // Seleccionar estadísticas según la categoría del movimiento
        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? this.SpDefense : this.Defense;

        // Fórmula de daño con variación aleatoria
        float modifier = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifier);

        HP -= damage;
        if (HP < 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    /// <summary>
    /// Selecciona aleatoriamente uno de los movimientos disponibles del Pokémon.
    /// Utilizado por la IA enemiga para elegir ataques.
    /// </summary>
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

/// <summary>
/// Clase que encapsula los detalles del daño causado por un ataque.
/// Permite comunicar información adicional para mostrar mensajes apropiados.
/// </summary>
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}