using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace loaforcsSoundAPI.SoundPacks.Data;

[JsonConverter(typeof(RegistryConverter<,,>))]
public abstract class Registry<T, TCollection>() : IEnumerable<T> where TCollection : ICollection<T>, new() {
	readonly TCollection _items = []; // Generic ICollection holding entries in this Registry.
	internal JToken[] _temp; // Array holding JSON tokens to be parsed, in case it needs to be delayed.

	public IEnumerator<T> GetEnumerator() {
		return _items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	public virtual void PopulateRegistry() {
		for(int i = 0; i < _temp?.Length; i++) {
			if(TryParse(out T value, _temp[i])) {
				AddValue(value);
			}
		}
		_temp = null;
		OnRegistryPopulated();
	}

	public virtual void OnRegistryPopulated() { }

	public virtual bool TryParse(out T value, JToken token) {
		value = token.ToObject<T>();
		return value != null;
	}

	public virtual void AddValue(T value) => _items.Add(value);
	public virtual bool RemoveValue(T value) => _items.Remove(value);
	public virtual bool ContainsValue(T value) => _items.Contains(value);
	public virtual T FindValue(Predicate<T> match) {
		foreach(T value in _items) {
			if(match(value)) {
				return value;
			}
		}
		return default;
	}
}

class RegistryConverter<T, TCollection, TRegistry> : JsonConverter<TRegistry> where TCollection : ICollection<T>, new() where TRegistry : Registry<T, TCollection>, new() {
	public override TRegistry ReadJson(JsonReader reader, Type objectType, TRegistry existingValue, bool hasExistingValue, JsonSerializer serializer) {
		JToken token = JToken.Load(reader);

		if(token.Type is JTokenType.String) {
			return new TRegistry() { _temp = [token] };
		}

		if(token is JArray jsonArray) {
			return new TRegistry() { _temp = [.. jsonArray] };
		}

		IJsonLineInfo lineInfo = reader as IJsonLineInfo;
		throw new JsonReaderException($"Value '{token}' is not valid for a registry.", reader.Path, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0, null);
	}

	public override void WriteJson(JsonWriter writer, TRegistry value, JsonSerializer serializer) {
		throw new NotImplementedException("no.");
	}
}