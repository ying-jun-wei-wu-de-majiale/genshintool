using System;
using System.Collections.Generic;


[Serializable]
public class Relic
{
    public string id; // 唯一标识符
    public string name; // 圣遗物名称
    public int level; // 等级
    public RelicType type; // 类型(花/羽/沙/杯/头)
    public MainStat mainStat; // 主属性
    public List<SubStat> subStats; // 副属性列表

    public void LevelUp()
    {
        level++;
        // 升级逻辑...
    }
}

public enum RelicType { Flower, Plume, Sands, Goblet, Circlet }

[Serializable]
public class MainStat
{
    public StatType type;
    public float value;
}

[Serializable]
public class SubStat
{
    public StatType type;
    public float value;
}

public enum StatType { HP, ATK, DEF, HPPercent, ATKPercent, DEFPercent, ElementalMastery, EnergyRecharge, CritRate, CritDMG }