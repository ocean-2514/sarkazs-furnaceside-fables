using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Commands;
using Skz.Ocean.DynamicVars;
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

    // 带 DynamicVar 的描述：{Toil}/{Rarity} 等会被替换（裸 Description 不带 vars，不会替换）。
    public LocString DynamicDescription
    {
        get
        {
            LocString description = Description;
            DynamicVars.AddTo(description);
            return description;
        }
    }

    // 思绪的悬浮提示（标题 + 解析后的描述 + 图标），供奖励按钮等复用。
    public HoverTip HoverTip => new HoverTip(Title, DynamicDescription, Icon);

    protected abstract int InitialRarity { get; }
    protected abstract int InitialToil { get; }

    private int _rarity;
    private int _toil;

    public int Rarity
    {
        get => _rarity;
        set
        {
            AssertMutable();
            if (_rarity == value) return;
            _rarity = value;
            if (_dynamicVars != null) _dynamicVars[RarityVar.defaultName].BaseValue = value;
            OnRarityChanged();
        }
    }

    public int Toil
    {
        get => _toil;
        set
        {
            AssertMutable();
            if (_toil == value) return;
            _toil = value;
            if (_dynamicVars != null) _dynamicVars[ToilVar.defaultName].BaseValue = value;
            OnToilChanged();
        }
    }

	public abstract ThoughtType ThoughtType { get; }
	public abstract ThoughtRarity ThoughtRarity { get; }

    // canonical 由 ModelDb 经无参构造创建；这里把初始值灌进可变字段（不走属性 setter，避免 AssertMutable）。
    // ToMutable 用 MemberwiseClone 复制字段、不调构造，故克隆体继承当前值，之后可被改动。
    protected ThoughtModel()
    {
        _rarity = InitialRarity;
        _toil = InitialToil;
    }

    private ThoughtStatus _status = ThoughtStatus.Unused;

	private ThoughtModel? _canonicalInstance; 

	public bool HasBeenRemovedFromState { get; private set; }

	public override bool ShouldReceiveCombatHooks => true;

    private int _floorAddedToDeck;

	public virtual bool CanBeUpgraded => false;

	public virtual Type UpgradedType => null;

	public event Action? RarityChanged;
	public event Action? ToilChanged;

    public DynamicVarSet DynamicVars
	{
		get
		{
			if (_dynamicVars != null)
			{
				return _dynamicVars;
			}
			// 把 Toil / Rarity 作为 DynamicVar 暴露出来，供描述文本 {Toil}/{Rarity} 引用
			List<DynamicVar> vars = new List<DynamicVar>(CanonicalVars)
			{
				new ToilVar(Toil),
				new RarityVar(Rarity)
			};
			_dynamicVars = new DynamicVarSet(vars);
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
        // 灵感思绪 + 未使用 + 仅限战斗外
        return ThoughtType == ThoughtType.Inspirer
            && Status == ThoughtStatus.Unused
            && !CombatManager.Instance.IsInProgress;
    }

	public bool CanBeDiscarded()
	{
		return !CombatManager.Instance.IsInProgress && Status == ThoughtStatus.Unused;
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

    // 实现 ISarkazModel.AfterThoughtUsed，并设为 virtual 方便思绪子类重写。
    // “当（任意）思绪被使用时”的反应（例如某些遗愿/遗物联动）。
    // 注意：签名必须与 ISarkazModel 完全一致——默认接口方法只有签名相同的
    // public 方法才算实现，签名不一致会静默走接口的空默认实现（重写永不触发）。
    public virtual Task AfterThoughtUsed(IRunState runState, ThoughtModel thought)
    {
        return Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used)
        {
            Status = ThoughtStatus.Discarded;
            Owner.ClearThoughtUsed();
        }
    }

	protected void OnRarityChanged()
	{
		_dynamicVars = null;
		RarityChanged?.Invoke();
	}

	protected void OnToilChanged()
	{
		_dynamicVars = null;
		ToilChanged?.Invoke();
	}
} 