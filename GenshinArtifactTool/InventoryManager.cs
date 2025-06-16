using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

public class InventoryManager
{
    public static InventoryManager Instance { get; } = new InventoryManager();

    public List<Relic> Relics { get; private set; } = new List<Relic>();

    private readonly string savePath = "relic_inventory.json";

    private InventoryManager()
    {
        LoadInventory();
    }

    public void AddRelic(Relic newRelic)
    {
        Relics.Add(newRelic);
        SaveInventory();
    }

    public void UpdateRelic(Relic updatedRelic)
    {
        var index = Relics.FindIndex(r => r.id == updatedRelic.id);
        if (index != -1)
        {
            Relics[index] = updatedRelic;
            SaveInventory();
        }
    }

    private void SaveInventory()
    {
        using (var stream = File.Create(savePath))
        {
            var serializer = new DataContractJsonSerializer(typeof(List<Relic>));
            serializer.WriteObject(stream, Relics);
        }
    }

    private void LoadInventory()
    {
        if (File.Exists(savePath))
        {
            using (var stream = File.OpenRead(savePath))
            {
                var serializer = new DataContractJsonSerializer(typeof(List<Relic>));
                Relics = (List<Relic>)serializer.ReadObject(stream);
            }
        }
    }
}