using System;
using System.Collections.Generic;
using static InventoryForm;
using GenshinArtifactTool;

// 圣遗物数据管理器（单例模式）
public sealed class ArtifactDataManager
{

    // 私有静态实例，使用Lazy<T>实现线程安全的延迟初始化
    private static readonly Lazy<ArtifactDataManager> _instance =
        new Lazy<ArtifactDataManager>(() => new ArtifactDataManager());

    // 公共静态访问点
    public static ArtifactDataManager Instance => _instance.Value;

    // 存储所有圣遗物的列表
    private readonly List<Artifact> _artifacts = new List<Artifact>();

    // 私有构造函数，防止外部实例化
    private ArtifactDataManager() { }

    // 事件：当添加新圣遗物时触发
    public event Action<Artifact> ArtifactAdded;

    // 事件：当圣遗物被更新时触发
    public event Action<Artifact> ArtifactUpdated;

    // 事件：当圣遗物被删除时触发
    public event Action<Artifact> ArtifactRemoved;

    // 获取所有圣遗物
    public List<Artifact> GetAllArtifacts()
    {
        return new List<Artifact>(_artifacts); // 返回副本，防止外部直接修改
    }

    // 添加新圣遗物
    public void AddArtifact(Artifact artifact)
    {
        if (artifact == null)
            throw new ArgumentNullException(nameof(artifact));

        _artifacts.Add(artifact);
        ArtifactAdded?.Invoke(artifact); // 触发事件通知观察者
    }

    // 更新圣遗物
    public void UpdateArtifact(Artifact artifact)
    {
        if (artifact == null)
            throw new ArgumentNullException(nameof(artifact));

        // 找到要更新的圣遗物
        var index = _artifacts.FindIndex(a => a.Id == artifact.Id);
        if (index >= 0)
        {
            _artifacts[index] = artifact;
            ArtifactUpdated?.Invoke(artifact); // 触发事件通知观察者
        }
    }

    // 删除圣遗物
    public void RemoveArtifact(Artifact artifact)
    {
        if (artifact == null)
            throw new ArgumentNullException(nameof(artifact));

        if (_artifacts.Remove(artifact))
        {
            ArtifactRemoved?.Invoke(artifact); // 触发事件通知观察者
        }
    }

    // 清空所有圣遗物
    public void ClearAllArtifacts()
    {
        _artifacts.Clear();
        // 可以触发一个单独的事件通知界面完全刷新
    }
}