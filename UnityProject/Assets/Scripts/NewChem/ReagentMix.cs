
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chemistry
{
	[Serializable]
	public class ReagentMix : IEnumerable<KeyValuePair<Reagent, float>>
	{
		public const float ZERO_CELSIUS_IN_KELVIN = 273.15f;

		[SerializeField] private float temperature = ZERO_CELSIUS_IN_KELVIN;

		public float Temperature
		{
			get => temperature;
			set => temperature = value;
		}

		public DictionaryReagentFloat reagents;

		public ReagentMix(float temperature, DictionaryReagentFloat reagents)
		{
			Temperature = temperature;
			this.reagents = reagents;
		}

		public ReagentMix(Reagent reagent, float amount, float temperature = ZERO_CELSIUS_IN_KELVIN)
		{
			Temperature = temperature;
			reagents = new DictionaryReagentFloat {[reagent] = amount};
		}

		public ReagentMix(float temperature = ZERO_CELSIUS_IN_KELVIN)
		{
			Temperature = temperature;
			reagents = new DictionaryReagentFloat();
		}

		public float? this[Reagent reagent] => reagents.TryGetValue(reagent, out var val) ? val : (float?)null;

		public void Add(ReagentMix b)
		{
			Temperature = (
				              Temperature * CalculateTotal() +
				              b.Temperature * b.CalculateTotal()
				              ) /
			              (CalculateTotal() + b.CalculateTotal());

			foreach (var reagent in b.reagents)
			{
				if (reagents.TryGetValue(reagent.Key, out var value))
				{
					reagents[reagent.Key] = reagent.Value + value;
				}
				else
				{
					reagents[reagent.Key] = reagent.Value;
				}
			}
		}

		public void Subtract(ReagentMix b)
		{
			Temperature = (
				              Temperature * CalculateTotal() +
				              b.Temperature * b.CalculateTotal()
			              ) /
			              (CalculateTotal() - b.CalculateTotal());

			foreach (var reagent in b.reagents)
			{
				if (reagents.TryGetValue(reagent.Key, out var value))
				{
					reagents[reagent.Key] = reagent.Value - value;
				}
				else
				{
					reagents[reagent.Key] = reagent.Value;
				}
			}
		}

		public void Multiply(float multiplier)
		{
			foreach (var key in reagents.Keys.ToArray())
			{
				reagents[key] *= multiplier;
			}
		}

		public ReagentMix TransferTo(ReagentMix b, float amount)
		{
			var transferred = Clone();
			transferred.Max(amount, out _);
			Subtract(transferred);
			b.Add(transferred);
			return transferred;
		}

		public ReagentMix Take(float amount)
		{
			var taken = new ReagentMix();
			TransferTo(taken, amount);
			return taken;
		}

		public void RemoveVolume(float amount)
		{
			Multiply((CalculateTotal() - amount) / CalculateTotal());
		}

		public void Max(float max, out float removed)
		{
			removed = Math.Max(max - CalculateTotal(), 0);
			RemoveVolume(removed);
		}

		public void Clean()
		{
			foreach (var key in reagents.Keys.ToArray())
			{
				if (reagents[key] == 0)
				{
					reagents.Remove(key);
				};
			}
		}

		public void Clear()
		{
			reagents.Clear();
		}

		// Inefficient for now, can replace with a caching solution later.
		public float CalculateTotal()
		{
			return reagents.Sum(kvp => kvp.Value);
		}

		public bool Contains(Reagent reagent, float amount)
		{
			if (reagents.TryGetValue(reagent, out var value))
			{
				return value >= amount;
			}
			return false;
		}

		public bool ContainsMoreThan(Reagent reagent, float amount)
		{
			if (reagents.TryGetValue(reagent, out var value))
			{
				return value > amount;
			}
			return false;
		}

		public ReagentMix Clone()
		{
			return new ReagentMix(Temperature, new DictionaryReagentFloat(reagents));
		}

		public IEnumerator<KeyValuePair<Reagent, float>> GetEnumerator()
		{
			return reagents.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}


	[Serializable]
	public class DictionaryReagentInt : SerializableDictionary<Reagent, int>
	{

	}

	[Serializable]
	public class DictionaryReagentFloat : SerializableDictionary<Reagent, float>
	{
		public DictionaryReagentFloat()
		{
		}

		public DictionaryReagentFloat(IDictionary<Reagent, float> dict) : base(dict)
		{
		}
	}
}