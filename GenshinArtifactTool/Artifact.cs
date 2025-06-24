using System;
using System.Collections.Generic;

namespace GenshinArtifactTool
{
    // 圣遗物类定义
    public class Artifact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // 唯一标识
        public string Position { get; set; } // 部位（生之花、死之羽等）
        public string MainStat { get; set; } // 主词条
        public List<string> SubStats { get; set; } = new List<string>(); // 副词条
        public List<string> SelectedSubstats { get; set; }// 仅对祝圣之霜有效
        public ArtifactSource Source { get; set; } // 新增来源属性
        public int Level { get; set; } = 0; // 等级
        public int SelectedSubstatsUpgradedCount { get; set; } // 已强化次数

        public string SetName { get; set; } // 圣遗物套装名称
        public int StarRank { get; set; } = 5; // 星级（默认5星）
        public override string ToString()
        {
            return $"{Id} - {Position} - {MainStat}";
        }
        public enum ArtifactSource
        {
            Dropped,    // 刷本掉落
            Customized   // 祝圣之霜生成
        }
    }
}