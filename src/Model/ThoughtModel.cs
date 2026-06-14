using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model;

public abstract class ThoughtModel : AbstractModel, ISarkazModel
{
    private DynamicVarSet? _dynamicVars;

	private Player? _owner;

	protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected virtual string IconBaseName => base.Id.Entry.ToLowerInvariant();

	public string IconPath => ImageHelper.GetImagePath("thoughts/" + IconBaseName + ".png");

	public Texture2D Icon => ResourceLoader.Load<Texture2D>(IconPath);

    public virtual LocString Title => new LocString("thoughts", base.Id.Entry + ".title");

	public LocString Flavor => new LocString("thoughts", base.Id.Entry + ".flavor");

    public LocString Description => new LocString("thoughts", base.Id.Entry + ".description");

    public abstract ThoughtRarity Rarity {get; }

	public abstract ThoughtType ThoughtType { get; }

    private ThoughtStatus _status;

	private ThoughtModel? _canonicalInstance; 

	public bool HasBeenRemovedFromState { get; private set; }

	public override bool ShouldReceiveCombatHooks => true;

    private int _floorAddedToDeck;

    public DynamicVarSet DynamicVars
	{
		get
		{
			if (_dynamicVars != null)
			{
				return _dynamicVars;
			}
			_dynamicVars = new DynamicVarSet(CanonicalVars);
			_dynamicVars.InitializeWithOwner(this);
			return _dynamicVars;
		}
	}

    public Player Owner
	{
		get
		{
			AssertMutable();
			return _owner;
		}
		set
		{
			AssertMutable();
			if (_owner != null && _owner != value)
			{
				throw new InvalidOperationException("Cannot move thought from " + base.Id.Entry + " one owner to another");
			}
			_owner = value;
		}
	}

    public ThoughtModel CanonicalInstance
	{
		get
		{
			if (!base.IsMutable)
			{
				return this;
			}
			return _canonicalInstance;
		}
		private set
		{
			AssertMutable();
			_canonicalInstance = value;
		}
	}

    public ThoughtStatus Status
    {
        get
        {
            return _status;
        }
        set
        {
            AssertMutable();
			if (_status != value)
			{
				_status = value;
			}
        }
    }

    public int FloorAddedToDeck
	{
		get
		{
			return _floorAddedToDeck;
		}
		set
		{
			AssertMutable();
			_floorAddedToDeck = value;
		}
	}

    protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_dynamicVars = DynamicVars.Clone(this);
	}

    protected override void AfterCloned()
	{
		base.AfterCloned();
		if (_canonicalInstance == null)
		{
			CanonicalInstance = ModelDb.GetById<ThoughtModel>(base.Id);
		}
		HasBeenRemovedFromState = false;
	}

    public void RemoveInternal()
	{
		HasBeenRemovedFromState = true;
	}
    
    public bool UseThoughtInternal()
	{
        if (!CanBeUsed())
        {
            return false;
        }
		Status = ThoughtStatus.Used;
        return true;
	}

    public ThoughtModel ToMutable()
	{
		AssertCanonical();
		return (ThoughtModel)MutableClone();
	}

    public bool CanBeUsed()
    {
        return ThoughtType == ThoughtType.Inpirer && Status == ThoughtStatus.Unused;
    }

    public bool isUsed()
    {
        return Status == ThoughtStatus.Used;
    }

    public virtual Task AfterObtained()
    {
        return Task.CompletedTask;
    }
    
    public virtual Task AfterRemoved()
    {
        return Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used)
        {
            Status = ThoughtStatus.Discarded;
            Owner.ClearThoughtUsed();
            await ThoughtCmd.Remove(this);
        }
    }
} 