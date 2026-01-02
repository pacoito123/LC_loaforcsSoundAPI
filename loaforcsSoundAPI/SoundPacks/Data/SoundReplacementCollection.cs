using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using Newtonsoft.Json;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class SoundReplacementCollection : Conditional, IFilePathAware, IPackData {
	[JsonConstructor]
	internal SoundReplacementCollection() { }

	public SoundReplacementCollection(SoundPack pack) {
		Pack = pack;
		pack.ReplacementCollections.Add(this);
	}

	internal void AddSoundReplacementGroup(SoundReplacementGroup group) {
		Replacements.Add(group);
	}

	[field: NonSerialized]
	public override SoundPack Pack { get; set; }

	public bool UpdateEveryFrame { get; private set; }


	public bool Synced { get; private set; }

	public List<SoundReplacementGroup> Replacements { get; private set; } = [];


	public string FilePath { get; set; } = string.Empty;

	public string RelativePath {
		get {
			if (field.Length == 0) {
				int index = FilePath.IndexOf("replacers", StringComparison.InvariantCultureIgnoreCase);
				if (index != -1)
					field = FilePath[index..];
			}
			return field;
		}
		set;
	} = string.Empty;
}