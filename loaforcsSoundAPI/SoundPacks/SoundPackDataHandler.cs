using System;
using System.Collections.Generic;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks;

static class SoundPackDataHandler {
	static List<SoundPack> _loadedPacks = [];
	internal static IReadOnlyList<SoundPack> LoadedPacks => _loadedPacks.AsReadOnly();

	internal static Dictionary<string, List<SoundReplacementGroup>> SoundReplacements = [];

	// this seems kinda in-efficent but i dont really care
	internal static Dictionary<string, Func<Condition>> conditionFactories = new();

	internal static void Register(string id, Func<Condition> factory) {
		conditionFactories[id] = factory;
	}

	public static Condition CreateCondition(string id) {
		if(conditionFactories.TryGetValue(id, out var factory)) {
			return factory();
		}

		return new InvalidCondition(id);
	}

	internal static void AddLoadedPack(SoundPack pack) {
		_loadedPacks.Add(pack);
	}

	internal static void AddReplacement(SoundReplacementGroup group) {
		foreach(string match in group.Matches) {
			string[] splitMatch = match.Split(":", StringSplitOptions.RemoveEmptyEntries);
			if(splitMatch.Length == 0) continue;
			string clipName = splitMatch[^1];
			if(!SoundReplacements.TryGetValue(clipName, out List<SoundReplacementGroup> existingGroups)) {
				existingGroups = [];
			}
			if(existingGroups.Contains(group)) continue;
			if(existingGroups.Count == 0 || existingGroups[0].Priority >= group.Priority) {
				existingGroups.Insert(0, group);
			} else if(existingGroups[^1].Priority <= group.Priority) {
				existingGroups.Add(group);
			} else {
				int index = existingGroups.FindIndex(existingGroup => existingGroup.Priority >= group.Priority);
				existingGroups.Insert(index, group);
			}
			SoundReplacements[clipName] = existingGroups; // Why is this needed...?
		}
	}

	internal static List<SoundReplacementGroup> GetReplacements(string match) {
		return SoundReplacements[match];
	}
}